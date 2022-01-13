using System;
using System.Text;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerManager : MonoBehaviourPun, IPunObservable
{
    [SerializeField] private GameObject _healthPrefab;
    [SerializeField] private List<GameObject> _currentCollisions = new List<GameObject>();
    
    private bool healthUpdated;
    private bool expUpdated;
    
    private int scaleMap = 3;
    private float halfMovement;
    private float fullMovement;

    private double percentageSow = 0.33;
    private double percentageUp = 0.5;
    
    private float tilemapOffsetX = 0.25f;
    private float tilemapOffsetY = 2.75f;

    public GameManagement GameManagement {get; private set;}
    
    public int Health {get; private set;}
    public int Exp {get; private set;}
    public bool IsFirst {get; private set;}
    public bool InEncounter{get; set;}

    private void Start()
    {
        if(!this.photonView.IsMine) return;
        
        halfMovement = 0.5f * scaleMap;
        fullMovement = 1f * scaleMap;
        IsFirst = false;
        InEncounter = false;

        AddHealth(MasterManager.GameSettings.InitialHealth);
        AddExp(MasterManager.GameSettings.InitialExp);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) 
    {
        if(stream.IsWriting)
        {
            stream.SendNext(Health);
            stream.SendNext(Exp);
            stream.SendNext(IsFirst);
            stream.SendNext(InEncounter);
        }
        else
        {
            Health = (int)stream.ReceiveNext();
            Exp = (int)stream.ReceiveNext();
            IsFirst = (bool)stream.ReceiveNext();
            InEncounter = (bool)stream.ReceiveNext();
        }
    }

    public void FirstInitialize(GameManagement gameManagement)
    {
        GameManagement = gameManagement;
    }

    void Update()
    {   
        if(!this.photonView.IsMine) return;
        
        UpdateStats();
    }

    public void UpdateStats()
    {
        if(!healthUpdated)
        {
            healthUpdated = true;
            GameManagement.GameCanvas.PlayerStats.SetHealth(Health);
        }
        
        if(!expUpdated)
        {
            expUpdated = true;
            GameManagement.GameCanvas.PlayerStats.SetExp(Exp);
        }
    }

    private List<HeartInfo> GetNeighbourCoordinates(float x, float y)
    {
        List<HeartInfo> heartInfoList = new List<HeartInfo>();
        
        heartInfoList.Add(new HeartInfo(x + fullMovement, y));
        heartInfoList.Add(new HeartInfo(x - fullMovement, y));
        heartInfoList.Add(new HeartInfo(x + halfMovement, y + halfMovement));
        heartInfoList.Add(new HeartInfo(x + halfMovement, y - halfMovement));
        heartInfoList.Add(new HeartInfo(x - halfMovement, y + halfMovement));
        heartInfoList.Add(new HeartInfo(x - halfMovement, y - halfMovement));

        return heartInfoList;
    }

    public void OnClickSow()
    {
        if(!this.photonView.IsMine) return;

        float posX = GetPlayerPositionX();
        float posY = GetPlayerPositionY();

        int healthGiven;
        
        healthGiven = (int) Math.Ceiling(Health * percentageSow);

        if(healthGiven == 0) return;

        List<HeartInfo> heartInfoList = GetNeighbourCoordinates(posX, posY);
        GameObject currentHeart = null;
        
        foreach(HeartInfo heart in heartInfoList)
        {
            currentHeart = GameObject.Find("Heart_" + heart.X + "_" + heart.Y);
            if(currentHeart == null) break;
        }

        if(currentHeart == null)
        {
            AddHealth(-healthGiven);
            this.photonView.RPC("RpcHeartInstantiate", RpcTarget.MasterClient, posX, posY, healthGiven);
        }
    }

    public void OnClickHarvest()
    {
        if(!this.photonView.IsMine) return;

        GameObject heart = _currentCollisions.SingleOrDefault(x => x.gameObject.tag == "HealthItem");

        if(heart != null)
        {
            AddHealth(heart.GetComponent<HeartManager>().Health);
            this.photonView.RPC("RpcHeartDestroy", RpcTarget.MasterClient, heart.name);
        }
    }

    public void OnClickLevelUp()
    {
        if(!this.photonView.IsMine) return;

        int healthToLevel = (int) Math.Ceiling(Health * percentageUp);

        if(healthToLevel == 0) return;

        AddHealth(-healthToLevel);
        AddExp(healthToLevel);
    }

    private void CreateHeart(float posX, float posY, int health)
    {  
        String name =  "Heart_" + posX  + "_" + posY;

        if(GameObject.Find(name) != null) return;

        Vector2 position = new Vector2(posX, posY);

        object[] content = {
            name,
            health
        };

        GameObject heart = MasterManager.NetworkInstantiate(_healthPrefab, position, Quaternion.identity, true, content);
    }

    private float GetPlayerPositionX()
    {
        return transform.position.x - tilemapOffsetX;
    }

    private float GetPlayerPositionY()
    {
        return transform.position.y - tilemapOffsetY;
    }

    [PunRPC]
    private void RpcHeartInstantiate(float x, float y, int health)
    {
        List<HeartInfo> heartInfoList = GetNeighbourCoordinates(x, y);

        heartInfoList.ForEach(delegate(HeartInfo heartInfo){
            CreateHeart(heartInfo.X, heartInfo.Y, health);
        });
    }

    [PunRPC]
    private void RpcHeartDestroy(string name)
    {
        GameObject heart = GameObject.Find(name);
        PhotonNetwork.Destroy(heart);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(!this.photonView.IsMine) return;

        _currentCollisions.Add(col.gameObject);

        int players = _currentCollisions.Where(x => x.tag == "Player").Count();

        if(players == 1)
        {
            GameManagement.GameCanvas.EnableEncounter(); 
            InEncounter = true;
        }
        else if(players > 1)
        {    
            transform.position -= GameManagement.PlayerMovement.Direction;
        }
    }
 
    void OnTriggerExit2D(Collider2D col)
    {
        if(!this.photonView.IsMine) return;

        if(col.gameObject.tag == "Player")
        {
            GameManagement.GameCanvas.DisableEncounter();
            InEncounter = false;
            IsFirst = false;
        }

        _currentCollisions.Remove(col.gameObject);
    }

    private (int, int) CheckIfFirst()
    {
        GameObject opponentObject = _currentCollisions.SingleOrDefault(x => x.tag == "Player"); 
        PlayerManager opponent = opponentObject.GetComponent<PlayerManager>();

        IsFirst = !opponent.IsFirst;

        return (opponentObject.GetComponent<PhotonView>().ViewID, opponent.Health);
    }

    public void AddHealth(int health)
    {
        Health = Math.Max(Health+health, 0);
        healthUpdated = false;
    }

    private void AddExp(int exp)
    {
        Exp += exp;
        expUpdated = false;
    }

    private void OnWonFight(PlayerManager currentPlayer, int health)
    {
        currentPlayer.AddHealth(health - Health/2);
        currentPlayer.InEncounter = false;
        currentPlayer.IsFirst = false;
    }

    private void OnLostFight(PlayerManager currentPlayer, PlayerMovement movement)
    {
        currentPlayer.AddHealth(-Health/2);
        movement.GoToRandomPosition();
        currentPlayer.InEncounter = false;
        currentPlayer.IsFirst = false;
    }

    public void OnClickFight()
    {
        if(!this.photonView.IsMine) return;

        (int opponentID, int opponentHealth) = CheckIfFirst();

        if(IsFirst)
        {
            int totalHealth = (opponentHealth/2+Health/2)/2;

            System.Random rand = new System.Random();
            //Loses fight
            if(rand.Next(0, 2) == 0)
            {
                this.photonView.RPC("RpcFightPlayer", GameManagement.PlayersList[opponentID], opponentID, true, totalHealth);
                OnLostFight(GameManagement.PlayerManager, GameManagement.PlayerMovement);
            }
            //Wins fight
            else
            {
                this.photonView.RPC("RpcFightPlayer", GameManagement.PlayersList[opponentID], opponentID, false, totalHealth);
                OnWonFight(GameManagement.PlayerManager, totalHealth);
            }
        }
    }

    public void OnClickShare()
    {
        if(!this.photonView.IsMine) return;

        (int opponentID, int opponentHealth) = CheckIfFirst();

        if(IsFirst)
        {
            this.photonView.RPC("RpcShareLife", GameManagement.PlayersList[opponentID], opponentID, Health/2);
            AddHealth(opponentHealth/2);
            GameManagement.PlayerMovement.GoToRandomPosition();
            InEncounter = false;
            IsFirst = false;
        }
    }

    public void OnClickSteal()
    {
        if(!this.photonView.IsMine) return;

        (int opponentID, int opponentHealth) = CheckIfFirst();

        if(IsFirst)
        {
            System.Random rand = new System.Random();
            
            if(rand.Next(0, 4) == 0)
            {
                this.photonView.RPC("RpcStealLife", GameManagement.PlayersList[opponentID], opponentID, Health/4);
            }
            else
            {
                AddHealth(-Health);
            }

            GameManagement.PlayerMovement.GoToRandomPosition();
        }
    }

    public void OnClickFlee()
    {
        if(!this.photonView.IsMine) return;

        (int opponentID, int opponentHealth) = CheckIfFirst();

        if(IsFirst)
        {
            AddHealth(-Health/4);

            System.Random rand = new System.Random();

            if(rand.Next(0, 5) == 0)
            {
                IsFirst = false;
            }
            else
            {
                GameManagement.PlayerMovement.GoToRandomPosition();
                InEncounter = false;
                IsFirst = false;
            }
        }
    }

    [PunRPC]
    private void RpcStealLife(int viewID, int health)
    {
        PlayerManager currentPlayer = PhotonView.Find(viewID).gameObject.GetComponent<PlayerManager>();
        currentPlayer.AddHealth(-health);
        currentPlayer.InEncounter = false;
        currentPlayer.IsFirst = false;
    }

    [PunRPC]
    private void RpcShareLife(int viewID, int health)
    {
        PlayerManager currentPlayer = PhotonView.Find(viewID).gameObject.GetComponent<PlayerManager>();
        currentPlayer.AddHealth(health - currentPlayer.Health/2);
        currentPlayer.InEncounter = false;
        currentPlayer.IsFirst = false;
    }

    [PunRPC]
    private void RpcFightPlayer(int viewID, bool win, int health)
    {
        GameObject currentPlayerObject = PhotonView.Find(viewID).gameObject; 
        PlayerManager currentPlayer = currentPlayerObject.GetComponent<PlayerManager>();
        
        if(win)
        {
            OnWonFight(currentPlayer, health);
        }        
        else
        {
            PlayerMovement playerMovement = currentPlayerObject.GetComponent<PlayerMovement>();
            OnLostFight(currentPlayer, playerMovement);
        }
    }
}
