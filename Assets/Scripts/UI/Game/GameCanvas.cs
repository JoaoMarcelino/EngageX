using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameCanvas : MonoBehaviour
{
    [SerializeField] private PlayerStats _playerStats;
    public PlayerStats PlayerStats { get { return _playerStats;}}
    private PlayerManager _player;
    public PlayerManager Player { get { return _player;}}
    [SerializeField] private SowButton _sowButton;
    [SerializeField] private HarvestButton _harvestButton;
    [SerializeField] private UpButton _upButton;
    [SerializeField] private Text _ticksText;
    public void FirstInitialize(PlayerManager player)
    {
        _player = player;
        _sowButton.FirstInitialize(this);
        _harvestButton.FirstInitialize(this);
        _upButton.FirstInitialize(this);
    }
    public void SetTicksText(string text)
    {
        _ticksText.text = text;
    }
}
