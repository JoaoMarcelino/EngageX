using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

using System;

public class PlayerMovement : MonoBehaviour
{
    //Stores input from the PlayerInput
    private Vector2 movementInput;

    private Vector3 direction;

    public Tilemap fogOfWar;

    private float halfMovement = 1.5f;
    private float fullMovement = 3f;


    bool hasMoved;
    void Update()
    {
        if(movementInput.x == 0)
        {
            hasMoved = false;
        }
        else if (movementInput.x != 0 && !hasMoved)
        {
            hasMoved = true;

            GetMovementDirection();
        }

    }

    public void GetMovementDirection()
    {
        if (movementInput.x < 0)
        {
            if (movementInput.y > 0)
            {
                direction = new Vector3(-halfMovement, halfMovement);
            }
            else if (movementInput.y < 0)
            {
                direction = new Vector3(-halfMovement, -halfMovement);
            }
            else
            {
                direction = new Vector3(-fullMovement, 0, 0);
            }
            transform.position += direction;
            //UpdateFogOfWar();
        }
        else if (movementInput.x > 0)
        {
            if (movementInput.y > 0)
            {
                direction = new Vector3(halfMovement, halfMovement);
            }
            else if (movementInput.y < 0)
            {
                direction = new Vector3(halfMovement, -halfMovement);
            }
            else
            {
                direction = new Vector3(fullMovement, 0, 0);
            }


            transform.position += direction;
        }
        String strings = String.Format("Log: {0}  {1}", System.DateTime.Now, transform.position.y);
        Debug.Log(strings);
        
        checkBorders();
        //UpdateFogOfWar();
    }

    private void checkBorders(){

        float maxValue = 30 ;
        int offset = 3;
        float posY = transform.position.y;
        float posX= transform.position.x;

        if (posX < -maxValue){
            transform.position += new Vector3(maxValue * 2, 0);
        }
        else if (posX >= maxValue ){
            transform.position += new Vector3(-maxValue * 2 , 0);
        }
        else if (posY < -maxValue ){
            transform.position += new Vector3(0, maxValue * 2);
        }
        else if (posY >= maxValue ){
            transform.position += new Vector3(0, -maxValue * 2);
        }
    }

    public void OnMove(InputValue value)
    {
        movementInput = value.Get<Vector2>();
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        transform.position -= direction;
    }


}
