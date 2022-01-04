using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class GameManagement : MonoBehaviourPun
{
    [SerializeField] private GameCanvas _gameCanvas;
    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private int _ticks;
    [SerializeField] private List<HeartInfo> _heartList = new List<HeartInfo>();
    [SerializeField] private List<PlayerStatusInfo> _leaderboardList = new List<PlayerStatusInfo>();
    private Text TickText;
    private long flagTimeStamp;
    private bool _initialized = false;

    public List<HeartInfo> HeartList {get{return _heartList;}}
    public GameCanvas GameCanvas {get{return _gameCanvas;}}
    public PlayerManager PlayerManager{get; private set;}

    public static long GetTimestamp(DateTime value)
    {
        return Int64.Parse(value.ToString("yyyyMMddHHmmssffff"));
    }

    private void Start()
    {
        Vector3 spawnPoint = new Vector3(0, 0, 0);
        GameObject player = MasterManager.NetworkInstantiate(_playerPrefab, spawnPoint, Quaternion.identity, false);

        if(!player.GetPhotonView().IsMine) return;

        PlayerManager = player.GetComponent<PlayerManager>();

        if(PlayerManager != null)
        {
            PlayerManager.FirstInitialize(this);
            _gameCanvas.FirstInitialize(this);
        }

        if(PhotonNetwork.IsMasterClient)
        {
            player.GetPhotonView().RPC("RPC_SyncTicks", RpcTarget.Others);
        }

        GameObject camera = GameObject.FindWithTag("MainCamera");

        if(camera != null)
        {
            CameraMovement cameraScript = camera.GetComponent<CameraMovement>();
            if(cameraScript != null)
            {
                cameraScript.SetTarget(player.transform);
            }
        }

        GameObject hearts = GameObject.FindWithTag("HealthItem");

        if(hearts != null)
        {   
            player.GetPhotonView().RPC("RPC_RequestCurrentHeartsList", RpcTarget.MasterClient);
        }

        _initialized = true;
    }

    void Update()
    {
        if(GetTimestamp(DateTime.Now) - flagTimeStamp >= 10000)
        {
            _ticks += 1;
            flagTimeStamp = GetTimestamp(DateTime.Now);
        }
        _gameCanvas.SetTicksText(_ticks.ToString());
    }

    [PunRPC]
    private void RPC_RequestCurrentHeartsList()
    {
        MemoryStream stream = new MemoryStream();
        BinaryFormatter formatter = new BinaryFormatter();

        formatter.Serialize(stream, _heartList);
        byte[] byteArrayHeartList = stream.GetBuffer();

        base.photonView.RPC("RPC_SetCurrentHeartsList", RpcTarget.Others, byteArrayHeartList);
    } 

    [PunRPC]
    private void RPC_SetCurrentHeartsList(byte[] byteArrayHeartList)
    {
        if(_initialized) return;

        MemoryStream stream = new MemoryStream(byteArrayHeartList);
        BinaryFormatter formatter = new BinaryFormatter();

        object heartInfoObject = formatter.Deserialize(stream);
        List<HeartInfo> heartInfoList = heartInfoObject as List<HeartInfo>;

        _heartList.AddRange(heartInfoList);
    }

    [PunRPC]
    private void RPC_SyncTicks(long timestamp)
    {
        if(_initialized) return;

        flagTimeStamp = timestamp;
    }
}
