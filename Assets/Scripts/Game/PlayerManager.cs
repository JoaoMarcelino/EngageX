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
    private GameObject FoW;
    public GameManagement GameManagement {get; private set;}
    private Pair _health = new Pair();
    public int Health {get{return _health.GetNew();}}
    private Pair _exp = new Pair();
    public int Exp {get{return _exp.GetNew();}}
    private Vector2 movementInput;
    private Vector3 direction;
    private bool _inEncounter;

    public double percentageSow = 0.33;
    public double percentageUp = 0.5;

    //Cursed
    private float tilemapOffsetX = 0.25f;
    private float tilemapOffsetY = 2.75f;

    private int scaleMap = 3;

    private float halfMovement;
    private float fullMovement;
    private float minVal;

    private bool hasMoved;
    private long flagTimeStamp = GameManagement.GetTimestamp(DateTime.Now);

    [SerializeField] private List<GameObject> _currentCollisions = new List<GameObject>();

    private void Start()
    {
        this.halfMovement = 0.5f * scaleMap;
        this.fullMovement = 1f * scaleMap;

        _health.Add(10);
    }
    public void FirstInitialize(GameManagement gameManagement)
    {
        GameManagement = gameManagement;
    }
    void Update()
    {   
        if(!base.photonView.IsMine || _inEncounter) return;
        
        ProcessMovement();
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

    public void ProcessMovement()
    {
        float moveX = GameManagement.GameCanvas.NavigationControls.FixedJoystick.Horizontal;
        float moveY = GameManagement.GameCanvas.NavigationControls.FixedJoystick.Vertical;

        minVal = 0.4f;

        if(moveX == 0)
        {
            hasMoved = false;
        }
        else if(moveX > minVal && !hasMoved)
        {
            hasMoved = true;
            flagTimeStamp = GameManagement.GetTimestamp(DateTime.Now);

            GetMovementDirection(moveX, moveY);
        }
        else if(moveX < -minVal && !hasMoved)
        {
            hasMoved = true;
            flagTimeStamp = GameManagement.GetTimestamp(DateTime.Now);

            GetMovementDirection(moveX, moveY);
        }

        if(GameManagement.GetTimestamp(DateTime.Now) - flagTimeStamp >= 5000){
            hasMoved = false;
        }
    }

    public void GetMovementDirection(float moveX, float moveY)
    {
        float minVal = 0.4f;
        if (moveX < -minVal)
        {
            if (moveY > minVal)
            {
                direction = new Vector3(-halfMovement, halfMovement);
            }
            else if (moveY < -minVal)
            {
                direction = new Vector3(-halfMovement, -halfMovement);
            }
            else
            {
                direction = new Vector3(-fullMovement, 0);
            }
            transform.position += direction;
        }
        else if (moveX> minVal)
        {
            if (moveY > minVal)
            {
                direction = new Vector3(halfMovement, halfMovement);
            }
            else if (moveY < -minVal)
            {
                direction = new Vector3(halfMovement, -halfMovement);
            }
            else
            {
                direction = new Vector3(fullMovement, 0);
            }

            transform.position += direction;
        }
        
        checkBorders(direction);
    }

    private void checkBorders(Vector3 direction)
    {
        float maxValue = 10 * scaleMap;
        int offset = scaleMap;
        float posY = transform.position.y;
        float posX= transform.position.x;

        //X LIMITS FOR NORMAL VALUES
        if (posX < -maxValue && posX == -maxValue - fullMovement){
            transform.position = new Vector3(maxValue, posY);
        }
        else if (posX > maxValue && posX == maxValue + fullMovement){
            transform.position = new Vector3(-maxValue , posY);
        }
        
        //X LIMITS FOR .5 VALUES
        else if(posX < -maxValue - fullMovement){
            transform.position = new Vector3(maxValue - halfMovement, posY);
        }
        else if(posX > maxValue ){
            transform.position = new Vector3(-maxValue - halfMovement, posY);
        }
        //Y LIMITS FOR .5 VALUES
        else if (posY < -maxValue - halfMovement){
            transform.position = new Vector3(posX, maxValue);
        }
        else if (posY > maxValue){
            transform.position = new Vector3(posX, - maxValue - halfMovement);
        }
        String strings = String.Format("Log: {0}  {1} {2}", System.DateTime.Now, transform.position, transform.position);
    }

    public void OnMove(InputValue value)
    {
        if(!base.photonView.IsMine) return;

        movementInput = value.Get<Vector2>();
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
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(stream, heartInfoList);

            byte[] byteArrayHeartList = stream.GetBuffer();
            
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
            int heartId = heart.GetComponent<PhotonView>().ViewID;
            base.photonView.RPC("RPC_HeartDestroy", RpcTarget.All, posX, posY, playerId);
        }
    }

    public void OnClickLevelUp()
    {
        if(!base.photonView.IsMine) return;

        int healthToLevel = (int) Math.Ceiling(_health.GetNew() * percentageUp);

        _health.Add(-healthToLevel);
        _exp.Add(healthToLevel);
    }

    private void CreateHeart(float posX, float posY, int health)
    {  
        String name =  "Heart_" + posX  + "_" + posY;

        if (GameObject.Find(name)){
            return;
        }

        Vector2 position = new Vector2(posX, posY);

        GameObject heart = MasterManager.NetworkInstantiate(_healthPrefab, position, Quaternion.identity, true);

        heart.name = name;

        heart.GetComponent<HeartManager>().Health = health;
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
        MemoryStream stream = new MemoryStream(byteArrayHeartList);
        BinaryFormatter formatter = new BinaryFormatter();

        object heartInfoObject = formatter.Deserialize(stream);
        List<HeartInfo> heartInfoList = heartInfoObject as List<HeartInfo>;

        heartInfoList = FilterDuplicateHearts(heartInfoList);

        if(PhotonNetwork.IsMasterClient)
        {
            heartInfoList.ForEach(delegate(HeartInfo heartInfo){
                CreateHeart(heartInfo.X, heartInfo.Y, health);
            });
        }

        GameManagement.HeartList.AddRange(heartInfoList);
    }

    [PunRPC]
    private void RPC_HeartDestroy(float x, float y, int playerId)
    {
        if(PhotonNetwork.IsMasterClient)
        {
            String name =  "Heart_" + x  + "_" + y;

            GameObject heart = GameObject.Find(name);

            if (!heart)
            {
                Debug.Log("RPC method called for non existing heart!");
                return;
            }

            int health = heart.GetComponent<HeartManager>().Health;
            Debug.Log(health);
            GameObject player = PhotonView.Find(playerId).gameObject;
            player.GetComponent<PlayerManager>().AddHealth(health);
            PhotonNetwork.Destroy(heart);
        }

        HeartInfo coordinate = GameManagement.HeartList.SingleOrDefault(r => r.X == x && r.Y == y);
        
        if(coordinate == null)
        {
            Debug.Log("RPC method called for non existing heart!");
            return;
        }

        GameManagement.HeartList.Remove(coordinate);
    }

    private List<HeartInfo> FilterDuplicateHearts(List<HeartInfo> heartInfoList)
    {
        return heartInfoList.Where(
            localHeartInfo => GameManagement.HeartList.All(
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

        int players = _currentCollisions.Where(x => x.tag == "Player").Count();

        if(players == 1)
        {
            _inEncounter = true;
            GameManagement.GameCanvas.EnableEncounter(); 
        }
        else if(players > 1)
            transform.position -= direction;
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
