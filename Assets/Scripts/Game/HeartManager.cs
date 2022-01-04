using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class HeartManager : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private int _health;

    public int Health {
        get {return _health;}
        set {_health = value;}
    }

    private long _tickTime; 
    private int _tick;
    private bool _hasUpdated;

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

    void Start()
    {
        FirstInitialize(GameManagement.GetTimestamp(DateTime.Now), 0);
    }

    public void FirstInitialize(long tickTime, int health)
    {
        _tickTime = tickTime;
        _health = health;
        _tick = 0;
    }

    // Update is called once per frame
    void Update()
    {
        _hasUpdated = true;
        
        if (GameManagement.GetTimestamp(DateTime.Now) - _tickTime >= 10000)
        {
            _tick += 1;
            _tickTime = GameManagement.GetTimestamp(DateTime.Now);
            _hasUpdated = false;
        }

        if(_tick % 10 == 0 & this.transform.localScale.x < 14 & !_hasUpdated)
        {
            Vector3 scale = new Vector3(3, 3);
            this.transform.localScale += scale; 
            _health += (int) Math.Round(_health * 0.10);
            
            _hasUpdated = true;
        }
    }
}
