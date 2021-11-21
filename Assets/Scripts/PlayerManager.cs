﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PlayerManager : MonoBehaviour
{
    //Stores input from the PlayerInput
    private Vector2 movementInput;

    private Vector3 direction;

    public int Health;
    public int Exp;

    public Sprite SowSprite;


    private int scaleMap = 3;

    private float halfMovement;
    private float fullMovement;


    private bool hasMoved;

    void Start(){
        this.halfMovement = 0.5f * scaleMap;
        this.fullMovement = 1f * scaleMap;
    }


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

        float maxValue = 10 * scaleMap;
        int offset = scaleMap;
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


    public void OnClickSow(){

        float posX= transform.position.x;
        float posY = transform.position.y;

        //ArrayList Hearts = new ArrayList();



        //Loop Around Player

        int index = 0;

        createHeart(posX + fullMovement, posY, index++);
        createHeart(posX - fullMovement, posY, index++);

        createHeart(posX + halfMovement, posY + halfMovement, index++);
        createHeart(posX + halfMovement, posY - halfMovement, index++);

        createHeart(posX - halfMovement, posY + halfMovement, index++);
        createHeart(posX - halfMovement, posY - halfMovement, index++);
    }


    private void createHeart(float posX, float posY, int i){

        GameObject heart = new GameObject();

        heart.tag = "HealthItem";
        heart.name = "Heart" + "1" + i;
        heart.transform.position = new Vector3(posX, posY);
        heart.transform.localScale = new Vector3(5, 5);

        heart.AddComponent<Rigidbody2D>();
        heart.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        heart.AddComponent<HeartManager>();
        heart.AddComponent<SpriteRenderer>();
        heart.GetComponent<SpriteRenderer>().sprite = SowSprite;
        heart.GetComponent<SpriteRenderer>().sortingOrder = 3;

        //Hearts.Add(heart);
    }
}