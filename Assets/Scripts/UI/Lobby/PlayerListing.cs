using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;

public class PlayerListing : MonoBehaviourPunCallbacks
{
    [SerializeField] private Text _text;
    public Player Player{get; private set;}
    public bool Ready = false;
    public void SetPlayerInfo(Player player)
    {
        SetPlayerText(player);
    }
    public override void OnPlayerPropertiesUpdate(Player target, ExitGames.Client.Photon.Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(target, changedProps);

        if(target != null && target == Player)
        {
            if(changedProps.ContainsKey("RandomNumber"))
                SetPlayerText(target);
        }
    }
    private void SetPlayerText(Player player) 
    {
        Player = player;
        _text.text = player.NickName;
    }
}
