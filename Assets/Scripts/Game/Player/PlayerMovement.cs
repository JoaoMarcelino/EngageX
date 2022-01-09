using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class PlayerMovement : MonoBehaviourPun
{
    private int scaleMap = 3;
    private float halfMovement;
    private float fullMovement;
    private Vector2 movementInput;
    private Vector3 direction;
    private float minVal;
    private bool hasMoved;
    private long flagTimeStamp;
    
    public Vector3 Direction {get{ return direction;}}
    public GameManagement GameManagement {get; private set;}

    public void FirstInitialize(GameManagement gameManagement)
    {
        GameManagement = gameManagement;
    }

    void Start()
    {
        if(!base.photonView.IsMine) return;

        halfMovement = 0.5f * scaleMap;
        fullMovement = 1f * scaleMap;
        flagTimeStamp = MasterManager.GetCurrentTimestamp();
    }

    void Update()
    {
        if(!base.photonView.IsMine) return;

        ProcessMovement();
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
            flagTimeStamp = MasterManager.GetCurrentTimestamp();

            GetMovementDirection(moveX, moveY);
        }
        else if(moveX < -minVal && !hasMoved)
        {
            hasMoved = true;
            flagTimeStamp = MasterManager.GetCurrentTimestamp();

            GetMovementDirection(moveX, moveY);
        }

        if(MasterManager.GetCurrentTimestamp() - flagTimeStamp >= 5000){
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
        if (posX < -maxValue && posX == -maxValue - fullMovement)
        {
            transform.position = new Vector3(maxValue, posY);
        }
        else if (posX > maxValue && posX == maxValue + fullMovement)
        {
            transform.position = new Vector3(-maxValue , posY);
        }
        
        //X LIMITS FOR .5 VALUES
        else if(posX < -maxValue - fullMovement)
        {
            transform.position = new Vector3(maxValue - halfMovement, posY);
        }
        else if(posX > maxValue)
        {
            transform.position = new Vector3(-maxValue - halfMovement, posY);
        }
        //Y LIMITS FOR .5 VALUES
        else if (posY < -maxValue - halfMovement)
        {
            transform.position = new Vector3(posX, maxValue);
        }
        else if (posY > maxValue)
        {
            transform.position = new Vector3(posX, - maxValue - halfMovement);
        }
        //String strings = String.Format("Log: {0}  {1} {2}", System.DateTime.Now, transform.position, transform.position);
    }

    public void OnMove(InputValue value)
    {
        if(!base.photonView.IsMine) return;

        movementInput = value.Get<Vector2>();
    }
}
