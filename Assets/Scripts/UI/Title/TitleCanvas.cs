using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class TitleCanvas : MonoBehaviourPunCallbacks
{
    [SerializeField] private LoadingScreen _loadingScreen;

    public void Start() 
    {
        FirstInitialize();    
    }

    public void FirstInitialize()
    {
        _loadingScreen.FirstInitialize(this);
    }

    public void OnClick_Play()
    {
        _loadingScreen.Show();

        PhotonNetwork.SendRate = 20;
        PhotonNetwork.SerializationRate = 10;
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.NickName = MasterManager.GameSettings.NickName;
        PhotonNetwork.GameVersion = MasterManager.GameSettings.GameVersion;
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log(PhotonNetwork.LocalPlayer.NickName);
        
        if(!PhotonNetwork.InLobby)
            PhotonNetwork.JoinLobby();
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        _loadingScreen.Hide();
    }

    public override void OnJoinedLobby()
    {
        PhotonNetwork.LoadLevel(1);
    }
}
