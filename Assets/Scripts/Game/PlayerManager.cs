using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Photon.Pun;

public class PlayerManager : MonoBehaviourPun
{
    //Stores input from the PlayerInput
    [SerializeField] private FixedJoystick _moveJoystick;
    [SerializeField] private GameObject FoW;
    [SerializeField] private Pair _health = new Pair();
    [SerializeField] private Pair _exp = new Pair();
    [SerializeField] private GameCanvas _gameCanvas;
    private GameObject EventSystem;

    private Vector2 movementInput;

    private Vector3 direction;

    public double percentageSow = 0.33;
    public double percentageUp = 0.5;

    //Cursed
    private float tilemapOffsetX = 0.25f;
    private float tilemapOffsetY = 2.75f;

    private int scaleMap = 3;

    private float halfMovement;
    private float fullMovement;

    private float moveX, moveY, minVal;
    
    public Sprite SowSprite;
    ArrayList heartsList = new ArrayList();

    private bool hasMoved;

    private long flagTimeStamp =  GetTimestamp(DateTime.Now);

    private void Start() {
        this.halfMovement = 0.5f * scaleMap;
        this.fullMovement = 1f * scaleMap;

        //this.FoW = GameObject.FindGameObjectWithTag("FogOfWar");
        this.EventSystem = GameObject.FindGameObjectWithTag("EventSystem");
        GameObject moveJoystickObject = GameObject.FindGameObjectWithTag("Joystick");

        if(moveJoystickObject != null)
        {
            this._moveJoystick = moveJoystickObject.GetComponent<FixedJoystick>();
        }
    }
    public void FirstInitialize(GameCanvas gameCanvas)
    {
        _gameCanvas = gameCanvas;
    }

    public static long GetTimestamp(DateTime value)
    {
        return Int64.Parse(value.ToString("yyyyMMddHHmmssffff"));
    }


    void Update()
    {   
        if(!base.photonView.IsMine) return;

        //this.FoW.GetComponent<SpriteRenderer>().sprite = new SPRITE
        moveX = _moveJoystick.Horizontal;
        moveY = _moveJoystick.Vertical;
        minVal = 0.4f;


        //Cursed if else if pq nao dava doutra maneira nao sei pq
        if(moveX == 0 )
        {
            hasMoved = false;
        }
        else if(moveX >minVal && !hasMoved)
        {

            hasMoved = true;
            flagTimeStamp = GetTimestamp(DateTime.Now);

            GetMovementDirection();
        }
        else if(moveX < -minVal && !hasMoved)
        {

            hasMoved = true;
            flagTimeStamp = GetTimestamp(DateTime.Now);

            GetMovementDirection();
        }

        if(GetTimestamp(DateTime.Now) - flagTimeStamp >= 5000){
            hasMoved = false;
        }

        if(_health.WasUpdated())
        {
            _health.Update();
            _gameCanvas.PlayerStats.SetHealthText("Health: " + _health.GetNew().ToString());
        }
        
        if(_exp.WasUpdated())
        {
            _exp.Update();
            _gameCanvas.PlayerStats.SetExpText("Exp: " + _exp.GetNew().ToString());
        }
    }

    public void GetMovementDirection()
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
            //UpdateFogOfWar();
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
        
        //String strings = String.Format("Log: {0}  {1} {2}", System.DateTime.Now, -10 * scaleMap - direction.x, 10 * scaleMap + direction.x);
        //Debug.Log(strings);
        
        checkBorders(direction);
        //UpdateFogOfWar();
    }

    private void checkBorders(Vector3 direction){

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
        Debug.Log(strings);
    }

    public void OnMove(InputValue value)
    {
        movementInput = value.Get<Vector2>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        transform.position -= direction;
    }
    public void OnClickSow(){

        float posX= transform.position.x - tilemapOffsetX;
        float posY = transform.position.y - tilemapOffsetY;

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
        
        createHeart(posX + fullMovement, posY, healthGiven);
        createHeart(posX - fullMovement, posY, healthGiven);

        createHeart(posX + halfMovement, posY + halfMovement, healthGiven);
        createHeart(posX + halfMovement, posY - halfMovement, healthGiven);

        createHeart(posX - halfMovement, posY + halfMovement, healthGiven);
        createHeart(posX - halfMovement, posY - halfMovement, healthGiven);
    }


    public void OnClickHarvest(){
        
        float posX= transform.position.x - tilemapOffsetX;
        float posY = transform.position.y - tilemapOffsetY;


        int healthChange = removeHeart(posX, posY);

        if (healthChange != -1){
            _health.Add(healthChange);
            Debug.Log(_health.GetNew());
        }
    }


    public void onClickLevelUp(){
        int healthToLevel = (int) Math.Ceiling(_health.GetNew() * percentageUp);
        _health.Add(-healthToLevel);
        _exp.Add(healthToLevel);
    }

    public void createHeart(float posX, float posY, int health){    
    
        String name =  "Heart_" + (posX + tilemapOffsetX)  + "_" + (posY + tilemapOffsetY);

        if ( GameObject.Find(name)){
            return ;
        }

        GameObject heart = new GameObject();

        heartsList.Add(heart);

        heart.tag = "HealthItem";
        heart.name = name;


        heart.transform.position = new Vector3(posX, posY);
        heart.transform.localScale = new Vector3(5, 5);

        heart.AddComponent<Rigidbody2D>();
        heart.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        heart.AddComponent<SpriteRenderer>();
        heart.GetComponent<SpriteRenderer>().sprite = SowSprite;
        heart.GetComponent<SpriteRenderer>().sortingOrder = 3;

        heart.AddComponent<HeartManager>();
        heart.GetComponent<HeartManager>().addHealth(health);
    }

    public int removeHeart(float posX, float posY){
        String name =  "Heart_" + (posX + tilemapOffsetX)  + "_" + (posY + tilemapOffsetY);

        GameObject heart = GameObject.Find(name);

        if (!heart){
            return -1;
        }
        
        int heartValue = heart.GetComponent<HeartManager>().getHealth();

        heartsList.Remove(heart);

        Destroy(heart);

        return heartValue;
    }

    /*
    public void onClickFight(){

        //TODO CALL FUNCTION TO CHECK IF FIRST;
        bool isFirst = false;


        if (isFirst){

            int sum = this.health + opponent.getHealth * 0.5;

            if (winner){
                this.addHealth(sum * 0.5);
            }else{
                opponent.addHealth(sum * 0.5);
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
                    opponent.addHealth(aux);
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

            opponent.addHealth(health * 0.5);

            this.addHealth(auxHealth);
        }

    }

    public void onClickSteal(){

        //TODO CALL FUNCTION TO CHECK IF FIRST;
        bool isFirst = false;

        if (isFirst){
            if (Random.Range(0, 100) < 25){
                this.addHealth(opponent.getHealth() * 0.25);
            }
            else{ 
                this.Health = 0;
            }
        }


    }*/

}
