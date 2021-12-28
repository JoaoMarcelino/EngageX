using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameCanvas : MonoBehaviour
{
    [SerializeField] private PlayerStats _playerStats;
    [SerializeField] private Text _ticksText;

    public PlayerStats PlayerStats { get { return PlayerStats;}}
    public void SetTicksText(string text)
    {
        _ticksText.text = text;
    }
}
