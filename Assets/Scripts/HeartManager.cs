using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HeartManager : MonoBehaviour
{

    public int health = 0;
    private long tickTime; 
    private int Tick = 0;
    private bool hasUpdated;

    public double healthLimit = 10.0;
    public int scaleSize = 1;
    public int initialSize;

    public static long GetTimestamp(DateTime value)
    {
        return Int64.Parse(value.ToString("yyyyMMddHHmmssffff"));
    }
    
    void Start(){
        tickTime = GetTimestamp(DateTime.Now);

        changeScale();
    }

    // Update is called once per frame
    void Update()
    {
        hasUpdated = true;
        
        if ( GetTimestamp(DateTime.Now) - tickTime >= 10000){
            Tick += 1;
            tickTime = GetTimestamp(DateTime.Now);
            hasUpdated = false;
        }

        if(Tick % 10 == 0 & this.transform.localScale.x < 14 & !hasUpdated){
            
            health += (int) Math.Ceiling(health * 0.25);
            
            hasUpdated = true;
            
            changeScale();
        }

        
    }

    public void changeScale(){
        int size = (int) Math.Floor(health / healthLimit);

        Vector3 scale = new Vector3(initialSize +  scaleSize * size, initialSize +  scaleSize * size);
        this.transform.localScale = scale; 
    }

    public void addHealth(int value){
        health += value;
    }

    public int getHealth(){
        return health;
    }
    
}
