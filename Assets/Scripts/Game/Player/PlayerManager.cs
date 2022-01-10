using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerManager : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private GameObject _healthPrefab;
    [SerializeField] private List<HeartInfo> _heartList = new List<HeartInfo>();
    [SerializeField] private List<GameObject> _currentCollisions = new List<GameObject>();
    
    private bool healthUpdated;
    private bool expUpdated;
    
    private int scaleMap = 3;
    private float halfMovement;
    private float fullMovement;

    private double percentageSow = 0.33;
    private double percentageUp = 0.5;
    
    private float tilemapOffsetX = 0.25f;
    private float tilemapOffsetY = 2.75f;

    public GameManagement GameManagement {get; private set;}
    public List<HeartInfo> HeartList {get{ return _heartList;} set{_heartList = value;}}
    
    public int Health {get; private set;}
    public int Exp {get; private set;}
    public bool IsFirst {get; private set;}
    public bool InEncounter{get; set;}
    public int PlayerID{get;set;}

    private void Start()
    {
        if(!this.photonView.IsMine) return;
        
        halfMovement = 0.5f * scaleMap;
        fullMovement = 1f * scaleMap;
        IsFirst = false;
        InEncounter = false;
        PlayerID = this.photonView.ViewID;

        AddHealth(MasterManager.GameSettings.InitialHealth);
        AddExp(MasterManager.GameSettings.InitialExp);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) 
    {
        if(stream.IsWriting)
        {
            stream.SendNext(Health);
            stream.SendNext(Exp);
            stream.SendNext(IsFirst);
            stream.SendNext(InEncounter);
            stream.SendNext(PlayerID);
        }
        else
        {
            Health = (int)stream.ReceiveNext();
            Exp = (int)stream.ReceiveNext();
            IsFirst = (bool)stream.ReceiveNext();
            InEncounter = (bool)stream.ReceiveNext();
            PlayerID = (int)stream.ReceiveNext();
        }
    }

    public void FirstInitialize(GameManagement gameManagement)
    {
        GameManagement = gameManagement;
    }

    void Update()
    {   
        if(!this.photonView.IsMine) return;
        
        UpdateStats();
    }

    public void UpdateStats()
    {
        if(!healthUpdated)
        {
            healthUpdated = true;
            GameManagement.GameCanvas.PlayerStats.SetHealth(Health);
        }
        
        if(!expUpdated)
        {
            expUpdated = true;
            GameManagement.GameCanvas.PlayerStats.SetExp(Exp);
        }
    }

    public void OnClickSow()
    {
        if(!this.photonView.IsMine) return;

        float posX = GetPlayerPositionX();
        float posY = GetPlayerPositionY();

        int healthGiven;
        int aux = 10; 

        //TODO LOSE HEALTH, CHOOSE PERCENTAGE OF HEALTH GIVEN
        if (Health <= aux)
        {
            healthGiven = (int) Math.Ceiling(aux * percentageSow);
        }
        else
        {
            healthGiven = (int) Math.Ceiling(Health * percentageSow);
            AddHealth(-healthGiven);
        }

        List<HeartInfo> heartInfoList = new List<HeartInfo>();
        
        heartInfoList.Add(new HeartInfo(posX + fullMovement, posY));
        heartInfoList.Add(new HeartInfo(posX - fullMovement, posY));
        heartInfoList.Add(new HeartInfo(posX + halfMovement, posY + halfMovement));
        heartInfoList.Add(new HeartInfo(posX + halfMovement, posY - halfMovement));
        heartInfoList.Add(new HeartInfo(posX - halfMovement, posY + halfMovement));
        heartInfoList.Add(new HeartInfo(posX - halfMovement, posY - halfMovement));

        heartInfoList = FilterDuplicateHearts(heartInfoList);

        if(heartInfoList.Count != 0)
        {
            byte[] byteArrayHeartList = MasterManager.ToByteArray<List<HeartInfo>>(heartInfoList);
            this.photonView.RPC("RpcHeartInstantiate", RpcTarget.All, byteArrayHeartList, healthGiven);
        }
    }

    public void OnClickHarvest()
    {
        if(!this.photonView.IsMine) return;

        float posX = GetPlayerPositionX();
        float posY = GetPlayerPositionY();

        GameObject heart = _currentCollisions.SingleOrDefault(x => x.gameObject.tag == "HealthItem");

        if(heart != null)
        {
            int playerId = this.photonView.ViewID;
            
            if(PhotonNetwork.IsMasterClient) 
                HeartDestroy(posX, posY, playerId, false);
            else
                this.photonView.RPC("RpcHeartDestroy", RpcTarget.MasterClient, posX, posY, playerId);
        }
    }

    public void OnClickLevelUp()
    {
        if(!this.photonView.IsMine) return;

        int healthToLevel = (int) Math.Ceiling(Health * percentageUp);

        AddHealth(-healthToLevel);
        AddExp(healthToLevel);

        healthUpdated = expUpdated = true;
    }

    private void CreateHeart(float posX, float posY, int health)
    {  
        String name =  "Heart_" + posX  + "_" + posY;

        Vector2 position = new Vector2(posX, posY);

        GameObject heart = MasterManager.NetworkInstantiate(_healthPrefab, position, Quaternion.identity, true);
        heart.name = name;
        heart.GetComponent<HeartManager>().FirstInitialize(health);
    }

    private float GetPlayerPositionX()
    {
        return transform.position.x - tilemapOffsetX;
    }

    private float GetPlayerPositionY()
    {
        return transform.position.y - tilemapOffsetY;
    }

    [PunRPC]
    private void RpcHeartInstantiate(byte[] byteArrayHeartList, int health)
    {
        List<HeartInfo> heartInfoList = MasterManager.FromByteArray<List<HeartInfo>>(byteArrayHeartList);

        heartInfoList = FilterDuplicateHearts(heartInfoList);

        if(PhotonNetwork.IsMasterClient)
        {
            heartInfoList.ForEach(delegate(HeartInfo heartInfo){
                CreateHeart(heartInfo.X, heartInfo.Y, health);
            });
        }

        HeartList.AddRange(heartInfoList);
    }

    private void HeartDestroy(float x, float y, int playerId, bool remote)
    {
        String name =  "Heart_" + x  + "_" + y;
        GameObject heart = GameObject.Find(name);

        if(!remote)AddHealth(heart.GetComponent<HeartManager>().Health);
        
        PhotonNetwork.Destroy(heart);
        HeartList.Remove(HeartList.SingleOrDefault(r => r.X == x && r.Y == y));
        
        this.photonView.RPC("RpcUpdatePlayers", RpcTarget.Others, x, y, playerId, heart.GetComponent<HeartManager>().Health);
    }

    [PunRPC]
    private void RpcHeartDestroy(float x, float y, int playerId)
    {
        HeartDestroy(x, y, playerId, true);
    }

    [PunRPC]
    private void RpcUpdatePlayers(float x, float y, int playerID, int health)
    {
        if(this.photonView.ViewID == playerID)
        {
            AddHealth(health);
        }

        HeartList.Remove(HeartList.SingleOrDefault(r => r.X == x && r.Y == y));
    }

    private List<HeartInfo> FilterDuplicateHearts(List<HeartInfo> heartInfoList)
    {
        return heartInfoList.Where(
            localHeartInfo => HeartList.All(
                globalHeartInfo =>
                    localHeartInfo.X != globalHeartInfo.X || 
                    localHeartInfo.Y != globalHeartInfo.Y)
        ).ToList();
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(!this.photonView.IsMine) return;

        _currentCollisions.Add(col.gameObject);

        int players = _currentCollisions.Where(x => x.tag == "Player").Count();

        if(players == 1)
        {
            GameManagement.GameCanvas.EnableEncounter(); 
            InEncounter = true;
        }
        else if(players > 1)
        {    
            transform.position -= GameManagement.PlayerMovement.Direction;
        }
    }
 
    void OnTriggerExit2D(Collider2D col)
    {
        if(!this.photonView.IsMine) return;

        if(col.gameObject.tag == "Player")
        {
            GameManagement.GameCanvas.DisableEncounter();
            InEncounter = false;
            IsFirst = false;
        }

        _currentCollisions.Remove(col.gameObject);
    }

    private (int, int) CheckIfFirst()
    {
        GameObject opponentObject = _currentCollisions.SingleOrDefault(x => x.tag == "Player"); 
        PlayerManager opponent = opponentObject.GetComponent<PlayerManager>();

        IsFirst = !opponent.IsFirst;

        return (opponentObject.GetComponent<PhotonView>().ViewID, opponent.Health);
    }

    public void AddHealth(int health)
    {
        Health = Math.Max(Health+health, 0);
        healthUpdated = false;
    }

    private void AddExp(int exp)
    {
        Exp += exp;
        expUpdated = false;
    }

    private void OnWonFight(int health)
    {
        AddHealth(health - Health/2);
        InEncounter = false;
        IsFirst = false;
    }

    private void OnLostFight()
    {
        AddHealth(-Health/2);
        GetComponent<PlayerMovement>().GoToRandomPosition();
        InEncounter = false;
        IsFirst = false;
    }

    public void OnClickFight()
    {
        if(!this.photonView.IsMine) return;

        (int opponentID, int opponentHealth) = CheckIfFirst();

        if(IsFirst)
        {
            int totalHealth = (opponentHealth/2+Health/2)/2;

            System.Random rand = new System.Random();
            //Loses fight
            if(rand.Next(0, 2) == 0)
            {
                this.photonView.RPC("RpcFightPlayer", GameManagement.PlayersList[opponentID], true, totalHealth);
                OnLostFight();
            }
            //Wins fight
            else
            {
                this.photonView.RPC("RpcFightPlayer", GameManagement.PlayersList[opponentID], false, totalHealth);
                OnWonFight(totalHealth);
            }
        }
    }

    public void OnClickShare()
    {
        if(!this.photonView.IsMine) return;

        (int opponentID, int opponentHealth) = CheckIfFirst();

        if(IsFirst)
        {
            this.photonView.RPC("RpcShareLife", GameManagement.PlayersList[opponentID], Health/2);
            AddHealth(opponentHealth/2);
            GameManagement.PlayerMovement.GoToRandomPosition();
            InEncounter = false;
            IsFirst = false;
        }
    }

    public void OnClickSteal()
    {
        if(!this.photonView.IsMine) return;

        (int opponentID, int opponentHealth) = CheckIfFirst();

        if(IsFirst)
        {
            System.Random rand = new System.Random();
            
            if(rand.Next(0, 4) == 0)
            {
                this.photonView.RPC("RpcStealLife", GameManagement.PlayersList[opponentID], Health/4);
            }
            else
            {
                AddHealth(-Health);
            }

            GameManagement.PlayerMovement.GoToRandomPosition();
        }
    }

    public void OnClickFlee()
    {
        if(!this.photonView.IsMine) return;

        (int opponentID, int opponentHealth) = CheckIfFirst();

        if(IsFirst)
        {
            AddHealth(Health/4);

            System.Random rand = new System.Random();

            if(rand.Next(0, 5) == 0)
            {
                IsFirst = false;
            }
            else
            {
                GameManagement.PlayerMovement.GoToRandomPosition();
                InEncounter = false;
                IsFirst = false;
            }
        }
    }

    [PunRPC]
    private void RpcStealLife(int health)
    {
        AddHealth(-health);
        InEncounter = false;
        IsFirst = false;
    }

    [PunRPC]
    private void RpcShareLife(int health)
    {
        AddHealth(health);
        InEncounter = false;
        IsFirst = false;
    }

    [PunRPC]
    private void RpcFightPlayer(bool win, int health)
    {
        if(win)
        {
            OnWonFight(health);
        }        
        else
        {
            OnLostFight();
        }
    }
}
