using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerListingsMenu : MonoBehaviourPunCallbacks
{
    [SerializeField] private Transform _content;
    [SerializeField] private PlayerListing _playerListing;

    private List<PlayerListing> _listings = new List <PlayerListing>();
    private LobbyCanvases _lobbyCanvases;
    
    public override void OnEnable() 
    {
        base.OnEnable();

        GetCurrentRoomPlayers();
    }
    
    public override void OnDisable()
    {
        base.OnDisable();

        for(int i = 0; i < _listings.Count; i++)
            Destroy(_listings[i].gameObject);
        
        _listings.Clear();    
    }

    public void FirstInitialize(LobbyCanvases canvases)
    {
        _lobbyCanvases = canvases;
    }

    private void GetCurrentRoomPlayers()
    {
        if(!PhotonNetwork.IsConnected || PhotonNetwork.CurrentRoom == null || PhotonNetwork.CurrentRoom.Players == null)
            return;

        foreach (KeyValuePair<int, Player> playerInfo in PhotonNetwork.CurrentRoom.Players)
        {
            AddPlayerListing(playerInfo.Value);
        }
    }

    private void AddPlayerListing(Player player)
    {
        int index = _listings.FindIndex(x => x.Player == player);
        if(index != -1)
        {
            _listings[index].SetPlayerInfo(player);
        }   
        else
        {
            PlayerListing listing = (PlayerListing) Instantiate(_playerListing, _content);
            if(listing != null)
            {
                listing.SetPlayerInfo(player);
                _listings.Add(listing);
            }
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        _lobbyCanvases.CurrentRoomCanvas.LeaveRoomMenu.OnClick_LeaveRoom();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) 
    {
        AddPlayerListing(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer) {
        int index = _listings.FindIndex( x => x.Player == otherPlayer);
        if(index != -1)
        {
            Destroy(_listings[index].gameObject);
            _listings.RemoveAt(index);
        }
    }

    private static T GetProperty<T>(string key, T deafult)
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(key, out object value))
        {
            return (T)value;
        }
        else return deafult;
    }

    private static void SetProperty(string key, object value)
    {
        ExitGames.Client.Photon.Hashtable table = PhotonNetwork.CurrentRoom.CustomProperties;
        if (table.ContainsKey(key))
        {
            table[key] = value;
        }
        else
        {
            table.Add(key, value);
        }
        PhotonNetwork.CurrentRoom.SetCustomProperties(table);
    }

    public void OnClick_StartGame()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel(2);
        }
    }
}