using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HeartManager : MonoBehaviour
{

    private int Health = 0;
    private long tickTime; 
    private int Tick = 0;
    private bool hasUpdated;

    public static long GetTimestamp(DateTime value)
    {
        return Int64.Parse(value.ToString("yyyyMMddHHmmssffff"));
    }
    
    void Start(){
        tickTime = GetTimestamp(DateTime.Now);
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
            Vector3 scale = new Vector3(3, 3);
            this.transform.localScale += scale ; 
                Health += (int) Math.Round(Health * 0.10);
            
            hasUpdated = true;
        }
        
    }

    public void addHealth(int value){
        Health += value;
    }

    public int getHealth(){
        return Health;
    }
    
}
