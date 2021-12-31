using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class HeartManager : MonoBehaviourPunCallbacks, IPunObservable
{
    private int _health = 0;
    private long tickTime; 
    private int Tick = 0;
    private bool hasUpdated;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) 
    {
        if(stream.IsWriting)
        {
            stream.SendNext(_health);
        }
        else
        {
            _health = (int)stream.ReceiveNext();
        }
    }

    public static long GetTimestamp(DateTime value)
    {
        return Int64.Parse(value.ToString("yyyyMMddHHmmssffff"));
    }
    
    void Start(){
        tickTime = GetTimestamp(DateTime.Now);
    }

    // Update is called once per frame
    void Update()
    {
        hasUpdated = true;
        
        if ( GetTimestamp(DateTime.Now) - tickTime >= 10000){
            Tick += 1;
            tickTime = GetTimestamp(DateTime.Now);
            hasUpdated = false;
        }

        if(Tick % 10 == 0 & this.transform.localScale.x < 14 & !hasUpdated){
            Vector3 scale = new Vector3(3, 3);
            this.transform.localScale += scale ; 
                _health += (int) Math.Round(_health * 0.10);
            
            hasUpdated = true;
        }
    }

    public void addHealth(int value){
        _health += value;
    }

    public int getHealth(){
        return _health;
    }
    
}
