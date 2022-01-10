using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

public class GameManagement : MonoBehaviourPunCallbacks, IOnEventCallback
{
    [SerializeField] private GameCanvas _gameCanvas;
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private int _ticks = 0;
    [SerializeField] private List<PlayerStatusInfo> _leaderboardList = new List<PlayerStatusInfo>();
    [SerializeField] private Dictionary<int, Player> _playersList = new Dictionary<int, Player>();
    [SerializeField] private long _roomCreationTime;

    private List<Player> newPlayers = new List<Player>();
    private bool _initializedByMasterClient = false;

    public List<PlayerStatusInfo> LeaderboardList {get{return _leaderboardList;}}
    public Dictionary<int, Player> PlayersList {get{return _playersList;}}
    public GameCanvas GameCanvas {get{return _gameCanvas;}}
    public PlayerManager PlayerManager{get; private set;}
    public PlayerMovement PlayerMovement{get; private set;}
    public long RoomCreationTime {get{return _roomCreationTime;}}

    private const byte RequestInitializationEvent = 1;
    private const byte InitializeEvent = 2;
    private const byte UpdatePlayerInfoOnMasterClientEvent = 3;
    private const byte RenderLeaderboardEvent = 4;
    private const byte UpdateNewPlayerViewIdEvent = 5;

    private GameObject SpawnPlayer()
    {
        Vector3 origin = new Vector3(0, 0, 0);
        GameObject player =  MasterManager.NetworkInstantiate(_playerPrefab, origin, Quaternion.identity, false);

        System.Random randomGenerator = new System.Random();
        
        float xOffset = 0.0f;
        float yOffset = 0.0f;

        //Light Green Coordinates
        if(randomGenerator.Next(0, 2) != 0)
        {
            xOffset = 3.0f * randomGenerator.Next(-10, 11);
            yOffset = 3.0f * randomGenerator.Next(-10, 11);
        }
        //Dark Green Coordinates
        else
        {
            xOffset = 1.5f * (2*randomGenerator.Next(-11, 10)+1);
            yOffset = 1.5f * (2*randomGenerator.Next(-11, 10)+1);
        }

        player.transform.position += new Vector3(xOffset, yOffset, 0);

        return player;
    }
    
    public override void OnEnable()
    {
        base.OnEnable();

        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        base.OnDisable();

        PhotonNetwork.RemoveCallbackTarget(this);
    }

    private void Start()
    {
        GameObject player = SpawnPlayer();

        if(!player.GetPhotonView().IsMine) return;

        GameCanvas.FirstInitialize(this);

        PlayerManager = player.GetComponent<PlayerManager>();
        if(PlayerManager != null) PlayerManager.FirstInitialize(this);

        PlayerMovement = player.GetComponent<PlayerMovement>();
        if(PlayerMovement != null) PlayerMovement.FirstInitialize(this);


        GameObject camera = GameObject.FindWithTag("MainCamera");

        if(camera != null)
        {
            CameraMovement cameraScript = camera.GetComponent<CameraMovement>();
            if(cameraScript != null)
            {
                cameraScript.SetTarget(player.transform);
            }
        }

        if(PhotonNetwork.IsMasterClient)
        {
            _roomCreationTime = MasterManager.GetCurrentTimestamp();

            PlayerStatusInfo playerStatusInfo = new PlayerStatusInfo(
                MasterManager.GameSettings.InitialHealth, 
                MasterManager.GameSettings.InitialExp,
                PhotonNetwork.LocalPlayer.NickName, 
                PhotonNetwork.LocalPlayer.ActorNumber);

            LeaderboardList.Add(playerStatusInfo);
            PlayersList.Add(player.GetPhotonView().ViewID, PhotonNetwork.LocalPlayer);

            _initializedByMasterClient = true;
        }
        else
        {
            RequestInitialization(PhotonNetwork.LocalPlayer, player.GetPhotonView().ViewID);
        }

        GameCanvas.LeaderboardPanel.RenderLeaderboard(LeaderboardList);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) 
    {
        newPlayers.Add(newPlayer);
    }

    private int ToModuledTick(int tick)
    {
        return tick%MasterManager.GameSettings.TickCountReset+1;
    }

    private int TicksToReset(int moduledTick)
    {
        if(moduledTick > MasterManager.GameSettings.TickCountReset)
        {
            moduledTick = ToModuledTick(moduledTick);
        }

        return moduledTick-MasterManager.GameSettings.TickCountReset;
    }

    private void UpdateCurrentPlayerStats()
    {
        //If is MasterClient Update his own values
        if(PhotonNetwork.IsMasterClient)
        {
            LeaderboardList
            .SingleOrDefault(x => x.PlayerID == PhotonNetwork.LocalPlayer.ActorNumber)
            .UpdateStats(PlayerManager.Health, PlayerManager.Exp);
        }
        //Send his values to master client and requests him to update
        else
        {
            UpdatePlayerInfoOnMasterClient(PhotonNetwork.LocalPlayer.ActorNumber, PlayerManager.Health, PlayerManager.Exp);
        }
    }

    private void UpdateLeaderboardValues()
    {
        if(!PhotonNetwork.IsMasterClient) return;

        byte[] byteArrayPlayerInfoList = MasterManager.ToByteArray<List<PlayerStatusInfo>>(LeaderboardList);
        RenderLeaderboard(byteArrayPlayerInfoList);
        GameCanvas.LeaderboardPanel.RenderLeaderboard(LeaderboardList);
    }

    void Update()
    {
        if(!_initializedByMasterClient) return;

        int currentTicks = (int) (MasterManager.GetCurrentTimestamp() - _roomCreationTime)/MasterManager.GameSettings.EachTickTime;

        //Verify if another tick has passed
        if(currentTicks > _ticks)
        { 
            //Calculate moduled tick
            int moduledTicks = ToModuledTick(currentTicks);
            GameCanvas.SetTicksText(moduledTicks.ToString());
            
            //Calculate Ticks to Reset
            int ticksToReset = TicksToReset(moduledTicks);

            switch(ticksToReset)
            {
                //Ticks were reset: Update leaderboard
                case 0:
                    UpdateLeaderboardValues();
                    break;
                //Only one tick left to reset: Update Current Player Stats   
                case 1:
                    UpdateCurrentPlayerStats();
                    break;
                default:
                    break;
            }
        }
    }

    private void RequestInitialization(Player player, int viewID)
    {
        object[] content = new object[]{
            player,
            viewID
        };

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions{
            Receivers = ReceiverGroup.MasterClient
        };

        PhotonNetwork.RaiseEvent(RequestInitializationEvent, content, raiseEventOptions, SendOptions.SendReliable);
    }

    private void Initialize(int playerID, long roomCreationTime, byte[] byteArrayHeartList, byte[] byteArrayLeaderboardList, byte[] byteArrayPlayerList)
    {
        object[] content = new object[]{
            playerID,
            roomCreationTime,
            byteArrayHeartList,
            byteArrayLeaderboardList,
            byteArrayPlayerList
        };

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions{
            Receivers = ReceiverGroup.Others
        };

        PhotonNetwork.RaiseEvent(InitializeEvent, content, raiseEventOptions, SendOptions.SendReliable);
    }

    private void RenderLeaderboard(byte[] byteArrayPlayerInfoList)
    {
        object[] content = new object[]{
            byteArrayPlayerInfoList
        };

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions{
            Receivers = ReceiverGroup.Others
        };

        PhotonNetwork.RaiseEvent(RenderLeaderboardEvent, content, raiseEventOptions, SendOptions.SendReliable);
    }

    private void UpdatePlayerInfoOnMasterClient(int playerID, int health, int exp)
    {
        object[] content = new object[]{
            playerID,
            health,
            exp
        };

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions{
            Receivers = ReceiverGroup.MasterClient
        };

        PhotonNetwork.RaiseEvent(UpdatePlayerInfoOnMasterClientEvent, content, raiseEventOptions, SendOptions.SendReliable);
    }

    private void UpdateNewPlayerViewId(int playerID, int viewID)
    {
        object[] content = new object[]{
            playerID,
            viewID
        };

        RaiseEventOptions raiseEventOptions = new RaiseEventOptions{
            Receivers = ReceiverGroup.Others
        };

        PhotonNetwork.RaiseEvent(UpdatePlayerInfoOnMasterClientEvent, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void OnEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        object[] data;

        switch(eventCode)
        {
            case RequestInitializationEvent:
                data = (object[])photonEvent.CustomData;
                OnRequestInitialization(data);
                break;
            
            case InitializeEvent:
                data = (object[])photonEvent.CustomData;
                OnInitialize(data);
                break;
            
            case UpdatePlayerInfoOnMasterClientEvent:
                data = (object[])photonEvent.CustomData;
                OnUpdatePlayerInfoOnMasterClient(data);
                break;

            case RenderLeaderboardEvent:
                data = (object[])photonEvent.CustomData;
                OnRenderLeaderboard(data);
                break;
            case UpdateNewPlayerViewIdEvent:
                data = (object[])photonEvent.CustomData;
                OnUpdateNewPlayerViewId(data);
                break;
        }
    }

    private void OnRequestInitialization(object[] data)
    {
        Player player = (Player) data[0];
        int viewID = (int) data[1];
        int playerID = player.ActorNumber;
        string nickname = player.NickName;

        PlayerStatusInfo playerStatusInfo = new PlayerStatusInfo(
            MasterManager.GameSettings.InitialHealth, 
            MasterManager.GameSettings.InitialExp,
            nickname, 
            playerID);

        LeaderboardList.Add(playerStatusInfo);
        PlayersList.Add(viewID, player);

        byte[] byteArrayHeartList = MasterManager.ToByteArray<List<HeartInfo>>(PlayerManager.HeartList);
        byte[] byteArrayLeaderboardList = MasterManager.ToByteArray<List<PlayerStatusInfo>>(LeaderboardList);

        Dictionary<int, int> playersList = new Dictionary<int, int>();

        foreach(KeyValuePair<int, Player> playerInfo in PlayersList)
        {
            playersList.Add(playerInfo.Key, playerInfo.Value.ActorNumber);
        }

        byte[] byteArrayPlayerList = MasterManager.ToByteArray<Dictionary<int, int>>(playersList);

        Initialize(playerID, _roomCreationTime, byteArrayHeartList, byteArrayLeaderboardList, byteArrayPlayerList);
    }

    private void OnInitialize(object[] data)
    {
        int playerID = (int) data[0];
        if(playerID != PhotonNetwork.LocalPlayer.ActorNumber) return;
        
        _roomCreationTime = (long) data[1];
        byte[] byteArrayHeartList = (byte[]) data[2];

        List<HeartInfo> heartInfoList = MasterManager.FromByteArray<List<HeartInfo>>(byteArrayHeartList);
        PlayerManager.HeartList = heartInfoList;

        byte[] byteArrayLeaderboardList = (byte[]) data[3];
        List<PlayerStatusInfo> leaderboardList = MasterManager.FromByteArray<List<PlayerStatusInfo>>(byteArrayLeaderboardList);
        GameCanvas.LeaderboardPanel.RenderLeaderboard(leaderboardList);

        byte[] byteArrayPlayerList = (byte[]) data[4];
        Dictionary<int, int> playerList = MasterManager.FromByteArray<Dictionary<int, int>>(byteArrayPlayerList);

        foreach(KeyValuePair<int, Player> playerInfo in PhotonNetwork.CurrentRoom.Players)
        {
            foreach(KeyValuePair<int, int> player in playerList)
            {
                if(playerInfo.Value.ActorNumber == player.Value)
                {
                    PlayersList.Add(player.Key, playerInfo.Value);
                }
            }
        }

        _initializedByMasterClient = true;
    }
    
    private void OnRenderLeaderboard(object[] data)
    {
        byte[] byteArrayPlayerInfo = (byte[]) data[0];
        List<PlayerStatusInfo> playerInfoList = MasterManager.FromByteArray<List<PlayerStatusInfo>>(byteArrayPlayerInfo);
        GameCanvas.LeaderboardPanel.RenderLeaderboard(playerInfoList);
    }
    
    private void OnUpdatePlayerInfoOnMasterClient(object[] data)
    {
        int playerID = (int) data[0]; 
        int health = (int) data[1];
        int exp = (int) data[2];

        LeaderboardList
        .SingleOrDefault(x => x.PlayerID == playerID)
        .UpdateStats(health, exp);
    }

    private void OnUpdateNewPlayerViewId(object[] data)
    {
        if(!_initializedByMasterClient) return;
        
        int playerID = (int)data[0];
        int viewID = (int)data[1];

        Player newPlayer = newPlayers.SingleOrDefault(x => x.ActorNumber == playerID);
        PlayersList.Add(viewID, newPlayer);
        newPlayers.Remove(newPlayer);
    }
}
