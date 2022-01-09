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

public class PlayerManager : MonoBehaviourPun
{
    [SerializeField] private GameObject _healthPrefab;
    [SerializeField] private List<HeartInfo> _heartList = new List<HeartInfo>();
    [SerializeField] private List<GameObject> _currentCollisions = new List<GameObject>();
    
    private Pair _health = new Pair();
    private Pair _exp = new Pair();
    
    private int scaleMap = 3;
    private float halfMovement;
    private float fullMovement;

    private double percentageSow = 0.33;
    private double percentageUp = 0.5;
    
    private float tilemapOffsetX = 0.25f;
    private float tilemapOffsetY = 2.75f;

    public GameManagement GameManagement {get; private set;}
    public List<HeartInfo> HeartList {get{ return _heartList;} set{_heartList = value;}}
    
    public int Health {get {return _health.GetNew();}}

    public int Exp {get{return _exp.GetNew();}}

    private void Start()
    {
        if(!base.photonView.IsMine) return;
        
        halfMovement = 0.5f * scaleMap;
        fullMovement = 1f * scaleMap;

        _health.Add(MasterManager.GameSettings.InitialHealth);
        _exp.Add(MasterManager.GameSettings.InitialExp);
    }

    public void FirstInitialize(GameManagement gameManagement)
    {
        GameManagement = gameManagement;
    }

    void Update()
    {   
        if(!base.photonView.IsMine) return;
        
        UpdateStats();
    }

    public void UpdateStats()
    {
        if(_health.WasUpdated())
        {
            _health.Update();
            GameManagement.GameCanvas.PlayerStats.SetHealth(_health.GetNew());
        }
        
        if(_exp.WasUpdated())
        {
            _exp.Update();
            GameManagement.GameCanvas.PlayerStats.SetExp(_exp.GetNew());
        }
    }

    public void OnClickSow()
    {
        if(!base.photonView.IsMine) return;

        float posX = GetPlayerPositionX();
        float posY = GetPlayerPositionY();

        int healthGiven;
        int aux = 10; 

        //TODO LOSE HEALTH, CHOOSE PERCENTAGE OF HEALTH GIVEN
        if (_health.GetNew() <= aux)
        {
            healthGiven = (int) Math.Ceiling(aux * percentageSow);
        }
        else
        {
            healthGiven = (int) Math.Ceiling(_health.GetNew() * percentageSow);
            _health.Add(-healthGiven);
        }

        List<HeartInfo> heartInfoList = new List<HeartInfo>();
        
        heartInfoList.Add(new HeartInfo(posX + fullMovement, posY));
        heartInfoList.Add(new HeartInfo(posX - fullMovement, posY));
        heartInfoList.Add(new HeartInfo(posX + halfMovement, posY + halfMovement));
        heartInfoList.Add(new HeartInfo(posX + halfMovement, posY - halfMovement));
        heartInfoList.Add(new HeartInfo(posX - halfMovement, posY + halfMovement));
        heartInfoList.Add(new HeartInfo(posX - halfMovement, posY - halfMovement));

        heartInfoList = FilterDuplicateHearts(heartInfoList);

        if(heartInfoList.Count != 0)
        {
            byte[] byteArrayHeartList = MasterManager.ToByteArray<List<HeartInfo>>(heartInfoList);
            base.photonView.RPC("RPC_HeartInstantiate", RpcTarget.All, byteArrayHeartList, healthGiven);
        }
    }

    public void OnClickHarvest()
    {
        if(!base.photonView.IsMine) return;

        float posX = GetPlayerPositionX();
        float posY = GetPlayerPositionY();

        GameObject heart = _currentCollisions.SingleOrDefault(x => x.gameObject.tag == "HealthItem");

        if(heart != null)
        {
            int playerId = base.photonView.ViewID;
            
            if(PhotonNetwork.IsMasterClient) 
                HeartDestroy(posX, posY, playerId);
            else
                base.photonView.RPC("RPC_HeartDestroy", RpcTarget.MasterClient, posX, posY, playerId);
        }
    }

    public void OnClickLevelUp()
    {
        if(!base.photonView.IsMine) return;

        int healthToLevel = (int) Math.Ceiling(_health.GetNew() * percentageUp);

        Debug.Log("Health to level: " + healthToLevel);

        _health.Add(-healthToLevel);
        _exp.Add(healthToLevel);
    }

    private void CreateHeart(float posX, float posY, int health)
    {  
        String name =  "Heart_" + posX  + "_" + posY;

        Vector2 position = new Vector2(posX, posY);

        GameObject heart = MasterManager.NetworkInstantiate(_healthPrefab, position, Quaternion.identity, true);
        heart.name = name;
        heart.GetComponent<HeartManager>().FirstInitialize(health);
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
    private void RPC_HeartInstantiate(byte[] byteArrayHeartList, int health)
    {
        List<HeartInfo> heartInfoList = MasterManager.FromByteArray<List<HeartInfo>>(byteArrayHeartList);

        heartInfoList = FilterDuplicateHearts(heartInfoList);

        if(PhotonNetwork.IsMasterClient)
        {
            heartInfoList.ForEach(delegate(HeartInfo heartInfo){
                CreateHeart(heartInfo.X, heartInfo.Y, health);
            });
        }

        HeartList.AddRange(heartInfoList);
    }

    private void HeartDestroy(float x, float y, int playerId)
    {
        String name =  "Heart_" + x  + "_" + y;
        GameObject heart = GameObject.Find(name);
        int health = heart.GetComponent<HeartManager>().Health;
        _health.Add(health);
        PhotonNetwork.Destroy(heart);
        HeartList.Remove(HeartList.SingleOrDefault(r => r.X == x && r.Y == y));
        Debug.Log("Adding " + health + " to player" + playerId);
        base.photonView.RPC("RPC_UpdatePlayers", RpcTarget.Others, x, y, playerId, health);
    }

    [PunRPC]
    private void RPC_HeartDestroy(float x, float y, int playerId)
    {
        HeartDestroy(x, y, playerId);
    }

    [PunRPC]
    private void RPC_UpdatePlayers(float x, float y, int playerID, int health)
    {
        if(base.photonView.ViewID == playerID)
        {
            _health.Add(health);
        }

        HeartList.Remove(HeartList.SingleOrDefault(r => r.X == x && r.Y == y));
    }

    private List<HeartInfo> FilterDuplicateHearts(List<HeartInfo> heartInfoList)
    {
        return heartInfoList.Where(
            localHeartInfo => HeartList.All(
                globalHeartInfo =>
                    localHeartInfo.X != globalHeartInfo.X || 
                    localHeartInfo.Y != globalHeartInfo.Y)
        ).ToList();
    }

    public void AddHealth(int health)
    {
        _health.Add(health);
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(!base.photonView.IsMine) return;

        _currentCollisions.Add(col.gameObject);

        if(col.gameObject.tag == "HealthItem")
            Debug.Log("Heart health: " + col.gameObject.GetComponent<HeartManager>().Health);

        int players = _currentCollisions.Where(x => x.tag == "Player").Count();

        if(players == 1)
        {
            GameManagement.GameCanvas.EnableEncounter(); 
        }
        else if(players > 1)
            transform.position -= GameManagement.PlayerMovement.Direction;
    }
 
    void OnTriggerExit2D(Collider2D col)
    {
        if(!base.photonView.IsMine) return;

        _currentCollisions.Remove(col.gameObject);
    }

    /*
    public void onClickFight(){

        //TODO CALL FUNCTION TO CHECK IF FIRST;
        bool isFirst = false;


        if (isFirst){

            int sum = this.health + opponent.getHealth * 0.5;

            if (winner){
                this.AddHealth(sum * 0.5);
            }else{
                opponent.AddHealth(sum * 0.5);
            }

        }

    }

    public void onClickFlee(){
        
        //TODO CALL FUNCTION TO CHECK IF FIRST;
        bool isFirst = false;

        if (isFirst){
            if(Random.Range(0, 100) < 25){
                int aux = Health*0.25;
                
                this.Health -= aux;
                if (Random.Range(0, 100) < 20){
                    opponent.AddHealth(aux);
                }
            }
            else{ 
                pass;
            }
        }

    }

    public void onClickShare(){

        //TODO CALL FUNCTION TO CHECK IF FIRST;
        bool isFirst = false;
        if (isFirst){
            int auxHealth = opponent.getHealth() * 0.5;

            opponent.AddHealth(health * 0.5);

            this.AddHealth(auxHealth);
        }

    }

    public void onClickSteal(){

        //TODO CALL FUNCTION TO CHECK IF FIRST;
        bool isFirst = false;

        if (isFirst){
            if (Random.Range(0, 100) < 25){
                this.AddHealth(opponent.getHealth() * 0.25);
            }
            else{ 
                this.Health = 0;
            }
        }


    }*/

}
