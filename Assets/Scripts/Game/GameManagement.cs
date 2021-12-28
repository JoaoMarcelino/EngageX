using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Photon.Pun;

public class GameManagement : MonoBehaviourPun
{
    [SerializeField] private GameCanvas _gameCanvas;
    [SerializeField] private int _ticks;
    [SerializeField] private GameObject _playerPrefab;
    
    public Text TickText;
    private long flagTimeStamp =  GetTimestamp(DateTime.Now);

    public static long GetTimestamp(DateTime value)
    {
        return Int64.Parse(value.ToString("yyyyMMddHHmmssffff"));
    }
    private void Start() {
        Vector3 spawnPoint = new Vector3(0, 0, 0);
        GameObject player = MasterManager.NetworkInstantiate(_playerPrefab, spawnPoint, Quaternion.identity);

        if(!player.GetPhotonView().IsMine) return;
        
        PlayerManager playerManager = player.GetComponent<PlayerManager>();

        if(playerManager != null)
        {
            playerManager.FirstInitialize(_gameCanvas);
            _gameCanvas.FirstInitialize(playerManager);
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
    }
    void Update()
    {
        if(GetTimestamp(DateTime.Now) - flagTimeStamp >= 10000){
            _ticks += 1;
            flagTimeStamp = GetTimestamp(DateTime.Now);

        }
        _gameCanvas.SetTicksText(_ticks.ToString());
    }
}
