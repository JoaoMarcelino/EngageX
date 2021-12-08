using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class GameManagement : MonoBehaviour
{
    
    public int Ticks;
    public Text TickText;

    public Sprite SowSprite;

    private long flagTimeStamp =  GetTimestamp(DateTime.Now);

    //Cursed
    private float tilemapOffsetX = 0.25f;
    private float tilemapOffsetY = 2.75f;

    ArrayList heartsList = new ArrayList();

    public static long GetTimestamp(DateTime value)
    {
        return Int64.Parse(value.ToString("yyyyMMddHHmmssffff"));
    }

    void Start(){

        

    }

    void Update()
    {
        

        if(GetTimestamp(DateTime.Now) - flagTimeStamp >= 10000){
            Ticks += 1;
            flagTimeStamp = GetTimestamp(DateTime.Now);

        }

        TickText.text = Ticks.ToString();
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

}
