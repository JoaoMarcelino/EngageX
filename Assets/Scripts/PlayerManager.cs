using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    //Stores input from the PlayerInput
    public FixedJoystick moveJoystick;
    public GameObject FoW;
    public int Health;
    public int Exp;
    public Text HealthText;
    public Text ExpText;

    private GameObject EventSystem;

    private Vector2 movementInput;

    private Vector3 direction;

    //Cursed
    private float tilemapOffsetX = 0.25f;
    private float tilemapOffsetY = 2.75f;

    private int scaleMap = 3;

    private float halfMovement;
    private float fullMovement;

    private float moveX, moveY, minVal;


    private bool hasMoved;

    private long flagTimeStamp =  GetTimestamp(DateTime.Now);

    void Start()
    {
        Debug.Log("Start Here!");
        this.halfMovement = 0.5f * scaleMap;
        this.fullMovement = 1f * scaleMap;
        this.FoW = GameObject.FindGameObjectWithTag("FogOfWar");
        this.EventSystem = GameObject.FindGameObjectWithTag("EventSystem");

        GameObject moveJoystickObject = GameObject.FindGameObjectWithTag("Joystick"); 
        this.moveJoystick = moveJoystickObject.GetComponent<FixedJoystick>();
    }

    public static long GetTimestamp(DateTime value)
    {
        return Int64.Parse(value.ToString("yyyyMMddHHmmssffff"));
    }


    void Update()
    {   
        Debug.Log(transform.position);

        //this.FoW.GetComponent<SpriteRenderer>().sprite = new SPRITE
        moveX = moveJoystick.Horizontal;
        moveY = moveJoystick.Vertical;
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


        //Update Text:
        HealthText.text = "Health: " + Health;
        ExpText.text = "XP: " + Exp;

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


    public void addHealth(int value){
        Health = value;
    }




    public void OnClickSow(){

        float posX= transform.position.x - tilemapOffsetX;
        float posY = transform.position.y - tilemapOffsetY;


        //TODO LOSE HEALTH, CHOOSE PERCENTAGE OF HEALTH GIVEN
        int healthGiven = 10;


        //Loop Around Player
        EventSystem.GetComponent<GameManagement>().createHeart(posX + fullMovement, posY, healthGiven);
        EventSystem.GetComponent<GameManagement>().createHeart(posX - fullMovement, posY, healthGiven);

        EventSystem.GetComponent<GameManagement>().createHeart(posX + halfMovement, posY + halfMovement, healthGiven);
        EventSystem.GetComponent<GameManagement>().createHeart(posX + halfMovement, posY - halfMovement, healthGiven);

        EventSystem.GetComponent<GameManagement>().createHeart(posX - halfMovement, posY + halfMovement, healthGiven);
        EventSystem.GetComponent<GameManagement>().createHeart(posX - halfMovement, posY - halfMovement, healthGiven);
    }


    public void OnClickHarvest(){
        
        float posX= transform.position.x - tilemapOffsetX;
        float posY = transform.position.y - tilemapOffsetY;


        int healthChange = EventSystem.GetComponent<GameManagement>().removeHeart(posX, posY);

        if (healthChange != -1){
            Health += healthChange;
            Debug.Log(Health);
        }
    }


}
