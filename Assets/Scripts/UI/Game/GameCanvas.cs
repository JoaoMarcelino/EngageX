using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameCanvas : MonoBehaviour
{
    [SerializeField] private GameManagement _gameManagement;
    public GameManagement GameManagement { get { return _gameManagement;}}
    
    [SerializeField] private PlayerStats _playerStats;
    public PlayerStats PlayerStats { get { return _playerStats;}}  

    [SerializeField] private NavigationControls _navigationControls;
    public NavigationControls NavigationControls { get { return _navigationControls;}}

    [SerializeField] private EncounterControls _encounterControls;
    public EncounterControls EncounterControls { get { return _encounterControls;}}

    [SerializeField] private LeaderboardPanel _leaderboardPanel;
    public LeaderboardPanel LeaderboardPanel { get { return _leaderboardPanel;}}

    [SerializeField] private Text _ticksText;

    public void FirstInitialize(GameManagement gameManagement)
    {
        _gameManagement = gameManagement;

        _encounterControls.FirstInitialize(this);
        _navigationControls.FirstInitialize(this);
        _leaderboardPanel.FirstInitialize(this);
    }

    public void EnableEncounter()
    {
        _navigationControls.Hide();
        _encounterControls.Show();
    }

    public void DisableEncounter()
    {
        _encounterControls.Hide();
        _navigationControls.Show();
    }

    public void SetTicksText(string text)
    {
        _ticksText.text = text;
    }
}