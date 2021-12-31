using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameCanvas : MonoBehaviour
{
    [SerializeField] private GameManagement _gameManager;
    public GameManagement GameManagement { get { return _gameManager;}}
    [SerializeField] private PlayerStats _playerStats;
    public PlayerStats PlayerStats { get { return _playerStats;}}
    [SerializeField] private SowButton _sowButton;
    [SerializeField] private HarvestButton _harvestButton;
    [SerializeField] private UpButton _upButton;
    [SerializeField] private Text _ticksText;

    public void FirstInitialize(GameManagement gameManager)
    {
        _gameManager = gameManager;
        _sowButton.FirstInitialize(gameManager);
        _harvestButton.FirstInitialize(gameManager);
        _upButton.FirstInitialize(gameManager);
    }

    public void SetTicksText(string text)
    {
        _ticksText.text = text;
    }
}
