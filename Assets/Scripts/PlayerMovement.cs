using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

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
        
        Debug.Log(transform.position.x);
        
        checkBorders();
        //UpdateFogOfWar();
    }

    private void checkBorders(){

        int maxValue = 3 * 10;

        if (transform.position.x < -maxValue){
            transform.position += new Vector3(maxValue*2, 0);
        }
        else if (transform.position.x > maxValue ){
            transform.position += new Vector3(-maxValue*2, 0);
        }
        else if (transform.position.y < -maxValue ){
            transform.position += new Vector3(0, maxValue*2);
        }
        else if (transform.position.y > maxValue ){
            transform.position += new Vector3(0, -maxValue*2);
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
