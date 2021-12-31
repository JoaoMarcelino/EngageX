using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [SerializeField] private Text _healthText;
    [SerializeField] private Text _expText;

    public void SetPlayerStats(int health, int exp)
    {
        _healthText.text = "Health: " + health.ToString();
        _expText.text = "Exp: " + exp.ToString();
    }
    
    public void SetHealth(int health)
    {
        _healthText.text = "Health: " + health.ToString();
    }

    public void SetExp(int exp)
    {
        _expText.text = "Exp: " + exp.ToString();
    }
}
