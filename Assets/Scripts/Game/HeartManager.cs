using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class HeartManager : MonoBehaviourPunCallbacks
{
    private int _health;
    private long _tickTime; 
    private int _tick;
    private bool _hasUpdated;

    public static long GetTimestamp(DateTime value)
    {
        return Int64.Parse(value.ToString("yyyyMMddHHmmssffff"));
    }
    
    void Start(){
        _tickTime = GetTimestamp(DateTime.Now);
        _health = 0;
        _tick = 0;
    }

    // Update is called once per frame
    void Update()
    {
        _hasUpdated = true;
        
        if ( GetTimestamp(DateTime.Now) - _tickTime >= 10000){
            _tick += 1;
            _tickTime = GetTimestamp(DateTime.Now);
            _hasUpdated = false;
        }

        if(_tick % 10 == 0 & this.transform.localScale.x < 14 & !_hasUpdated){
            Vector3 scale = new Vector3(3, 3);
            this.transform.localScale += scale ; 
                _health += (int) Math.Round(_health * 0.10);
            
            _hasUpdated = true;
        }
    }

    public void addHealth(int value){
        _health += value;
    }

    public int getHealth(){
        return _health;
    }
}
