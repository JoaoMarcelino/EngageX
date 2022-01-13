using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class HeartManager : MonoBehaviourPunCallbacks, IPunObservable, IPunInstantiateMagicCallback
{
    [SerializeField] private int _health;
    private bool _initialized = false;
    private int _ticks;
    private long _heartCreationTime;


    public int Health 
    {
        get {return _health;}
        set {_health = value;}
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) 
    {
        if(stream.IsWriting)
        {
            stream.SendNext(Health);
        }
        else
        {
            Health = (int)stream.ReceiveNext();
        }
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        if(info.photonView.ViewID == base.photonView.ViewID)
        {
            gameObject.name = (string) info.photonView.InstantiationData[0];
            FirstInitialize((int) info.photonView.InstantiationData[1]);
        }
    }

    private int GetCurrentModuledTicks()
    {
        return GetCurrentTicks()/MasterManager.GameSettings.HeartUpgradeTicks;
    }

    private int GetCurrentTicks()
    {
        return (int) (MasterManager.GetCurrentTimestamp() - _heartCreationTime)/MasterManager.GameSettings.EachTickTime;
    }

    public void FirstInitialize(int health)
    {
        Health = health;
        _heartCreationTime = MasterManager.GetCurrentTimestamp();
        _ticks = GetCurrentModuledTicks();
        _initialized = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(!base.photonView.IsMine | !_initialized) return;

        int currentTicks = GetCurrentModuledTicks();

        if(currentTicks > _ticks & transform.localScale.x <14)
        { 
            _ticks = currentTicks;
            Debug.Log(currentTicks);
            Vector3 scale = new Vector3(3, 3);
            this.transform.localScale += scale; 
            _health += (int) Math.Round(_health * 0.10);
        }
    }
}