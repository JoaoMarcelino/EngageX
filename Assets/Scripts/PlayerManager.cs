using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class PlayerManager : MonoBehaviour
{
    //Stores input from the PlayerInput
    public FixedJoystick moveJoystick;
    public GameObject FoW;
    public int health;
    public int Exp;
    public Text HealthText;
    public Text ExpText;
    public double percentageSow = 0.33;
    public double percentageUp = 0.5;
    
    //Player movement variables
    public float movementSpeed = 5f;
    public Transform destination;

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

    void Start(){
        destination.parent = null;
        this.halfMovement = 0.5f * scaleMap ;
        this.fullMovement = 1f * scaleMap;
        this.FoW = GameObject.FindGameObjectWithTag("FogOfWar");
        this.EventSystem = GameObject.FindGameObjectWithTag("EventSystem");
    }

    public static long GetTimestamp(DateTime value){
        return Int64.Parse(value.ToString("yyyyMMddHHmmssffff"));
    }


    void Update(){

        //this.FoW.GetComponent<SpriteRenderer>().sprite = new SPRITE
        moveX = moveJoystick.Horizontal;
        moveY = moveJoystick.Vertical;
        minVal = 0.4f;

        transform.position = Vector3.MoveTowards(transform.position, destination.position, movementSpeed*Time.deltaTime) ;
        
        checkBorders(direction);
        //Cursed if else if pq nao dava doutra maneira nao sei pq
        if(moveX == 0 && !hasMoved){
            hasMoved = false;
        }else if(moveX > minVal && !hasMoved){
            hasMoved = true;
            flagTimeStamp = GetTimestamp(DateTime.Now);
            GetMovementDirection();
        }else if(moveX < -minVal && !hasMoved){
            hasMoved = true;
            flagTimeStamp = GetTimestamp(DateTime.Now);
            GetMovementDirection();
        }
        if(GetTimestamp(DateTime.Now) - flagTimeStamp >= 1000){
            hasMoved = false;
        }

        //Update Text:
        HealthText.text = "health: " + health;
        ExpText.text = "XP: " + Exp;
    }

    public void GetMovementDirection(){
        float minVal = 0.4f;
        if (moveX < -minVal){
            if (moveY > minVal){            // diagonal esquerda cima
                if(Mathf.Ceil(Vector3.Distance(transform.position, destination.position)) <= Math.Sqrt(Math.Pow(halfMovement, 2) * 2)) {
                    direction = new Vector3(-halfMovement, halfMovement);
                    destination.position += direction;
                }
            }else if (moveY < -minVal){     // diagonal esquerda baixo 
                if(Mathf.Ceil(Vector3.Distance(transform.position, destination.position)) <= Math.Sqrt(Math.Pow(halfMovement, 2) * 2)) {
                    direction = new Vector3(-halfMovement, -halfMovement);
                    destination.position += direction;
                }
            }else{                          // esquerda
                if(Mathf.Ceil(Vector3.Distance(transform.position, destination.position)) <= fullMovement) {
                    direction = new Vector3(-fullMovement, 0);
                    destination.position += direction;
                }
            }
        }else if (moveX> minVal){
            if (moveY > minVal){            // diagonal direita cima
                if(Mathf.Ceil(Vector3.Distance(transform.position, destination.position)) <= Math.Sqrt(Math.Pow(halfMovement, 2) * 2)) {
                    direction = new Vector3(halfMovement, halfMovement);
                    destination.position += direction;
                }
            } else if (moveY < -minVal){     // diagonal direita baixo
                if(Mathf.Ceil(Vector3.Distance(transform.position, destination.position)) <= Math.Sqrt(Math.Pow(halfMovement, 2) * 2)) {
                    direction = new Vector3(halfMovement, -halfMovement);
                    destination.position += direction;
                }
            } else{                          // direita
                if(Mathf.Ceil(Vector3.Distance(transform.position, destination.position)) <= fullMovement) {
                    direction = new Vector3(fullMovement, 0);
                    destination.position += direction;
                }
            }
        }
        //String strings = String.Format("Log: {0}  {1} {2}", System.DateTime.Now, -10 * scaleMap - direction.x, 10 * scaleMap + direction.x);
        //Debug.Log(strings);
        
        //UpdateFogOfWar();
    }

    private void checkBorders(Vector3 direction){

        float maxValue = 10 * scaleMap;
        int offset = scaleMap;
        float posY = transform.position.y;
        float posX= transform.position.x;


        //X LIMITS FOR NORMAL VALUES
        if(posX < -maxValue && posX == -maxValue - fullMovement) {
            if(posY != 0 && posY % 3 != 0) {                                            //impares           
                transform.position = new Vector3(maxValue - 1.5f, posY);                
                destination.position = new Vector3(maxValue - 1.5f, posY);
            } else {                                                                    //pares
                transform.position = new Vector3(maxValue, posY);
                destination.position = new Vector3(maxValue, posY);
            }
        }else if(posX > maxValue && posX == maxValue + fullMovement) {
            if(posY != 0 && posY % 3 != 0) {                                            //impares
                transform.position = new Vector3(-maxValue, posY);
                destination.position = new Vector3(-maxValue, posY);
            } else {                                                                    //pares
                transform.position = new Vector3(-maxValue+1.5f, posY);
                destination.position = new Vector3(-maxValue+1.5f, posY);
            }
        }

        //X LIMITS FOR .5 VALUES
        else if(posX < -maxValue - fullMovement) {
            if(posY != 0 && posY % 3 != 0) {                                            //impares
                transform.position = new Vector3(maxValue - halfMovement, posY);
                destination.position = new Vector3(maxValue - halfMovement, posY);
            }else{                                                                    //pares
                transform.position = new Vector3(maxValue - halfMovement + 1.5f, posY);
                destination.position = new Vector3(maxValue - halfMovement + 1.5f, posY);
            }
        }else if(posX > maxValue) {
            if(posY != 0 && posY % 3 != 0) {                                            //impares
                transform.position = new Vector3(-maxValue - halfMovement, posY);
                destination.position = new Vector3(-maxValue - halfMovement, posY);
            }else{                                                                    //pares
                transform.position = new Vector3(-maxValue - halfMovement + 1.5f, posY);
                destination.position = new Vector3(-maxValue - halfMovement + 1.5f, posY);
            }
        }
        //Y LIMITS FOR .5 VALUES
        else if(posY < -maxValue - halfMovement) {
            transform.position = new Vector3(posX, maxValue);
            destination.position = new Vector3(posX, maxValue);
        }else if(posY > maxValue) {
            transform.position = new Vector3(posX, -maxValue - halfMovement);
            destination.position = new Vector3(posX, -maxValue - halfMovement);
        }
        String strings = String.Format("Log: {0}  {1} {2}", System.DateTime.Now, transform.position, transform.position);
        //Debug.Log(strings);
    }

    public void OnMove(InputValue value){
        movementInput = value.Get<Vector2>();
    }


    private void OnCollisionEnter2D(Collision2D collision){
        transform.position -= direction;
    }


    public void addHealth(int value){
        health = value;
    }




    public void OnClickSow(){

        float posX= transform.position.x - tilemapOffsetX;
        float posY = transform.position.y - tilemapOffsetY;
        int healthGiven;

        int aux = 10; 

        //TODO LOSE HEALTH, CHOOSE PERCENTAGE OF HEALTH GIVEN
        if (health <= aux){
            healthGiven = (int) Math.Ceiling(aux * percentageSow);
        }else{
            healthGiven = (int) Math.Ceiling(health * percentageSow);
            health -= healthGiven;
        }


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
            health += healthChange;
            //Debug.Log(health);
        }
    }


    public void onClickLevelUp(){

        int healthToLevel = (int) Math.Ceiling(health * percentageUp);
        health -= healthToLevel;
        Exp += healthToLevel;
    }

}
