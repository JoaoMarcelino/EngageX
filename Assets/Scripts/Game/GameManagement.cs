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
    [SerializeField]
    private int _ticks;
    [SerializeField]
    private GameObject _prefab;
    public Text TickText;
    private long flagTimeStamp =  GetTimestamp(DateTime.Now);

    public static long GetTimestamp(DateTime value)
    {
        return Int64.Parse(value.ToString("yyyyMMddHHmmssffff"));
    }
    private void Start() {
        Vector3 spawnPoint = new Vector3(0, 0, 0);
        GameObject player = MasterManager.NetworkInstantiate(_prefab, spawnPoint, Quaternion.identity);

        if(!player.GetPhotonView().IsMine) return;

        player.transform.Find("Camera").gameObject.GetComponent<CameraMovement>().enabled = true;
        player.transform.Find("Camera").gameObject.GetComponent<CameraMovement>().SetTarget(player.transform);
        player.transform.Find("Camera").gameObject.SetActive(true);
    }
    void Update()
    {
        if(GetTimestamp(DateTime.Now) - flagTimeStamp >= 10000){
            _ticks += 1;
            flagTimeStamp = GetTimestamp(DateTime.Now);

        }

        TickText.text = _ticks.ToString();
    }
}
