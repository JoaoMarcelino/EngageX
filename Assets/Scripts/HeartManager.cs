using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HeartManager : MonoBehaviour
{

    private int Health = 0;
    private DateTime startTime; 
    
    void Start(){
        startTime = DateTime.Now;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void addHealth(int value){
        Health += value;
    }

    public int getHealth(){
        return Health;
    }
    
}
