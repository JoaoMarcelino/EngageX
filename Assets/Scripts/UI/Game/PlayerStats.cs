using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    [SerializeField]
    private Text _healthText;
    [SerializeField]
    private Text _expText;
    public void SetPlayerStats(int health, int exp)
    {
        _healthText.text = "Health: " + health.ToString();
        _expText.text = "Exp: " + exp.ToString();
    }
    public void SetHealthText(string text)
    {
        _healthText.text = text;
    }

    public void SetExpText(string text)
    {
        _expText.text = text;
    }
}
