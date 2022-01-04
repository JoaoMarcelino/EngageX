using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EncounterControls : MonoBehaviour
{
    [SerializeField] private FightButton _fightButton;
    [SerializeField] private FleeButton _fleeButton;
    [SerializeField] private ShareButton _shareButton;
    [SerializeField] private StealButton _stealButton;

    private GameCanvas _gameCanvas;

    public void FirstInitialize(GameCanvas gameCanvas)
    {
        _gameCanvas = gameCanvas;

        _fightButton.FirstInitialize(gameCanvas);
        _fleeButton.FirstInitialize(gameCanvas);
        _shareButton.FirstInitialize(gameCanvas);
        _stealButton.FirstInitialize(gameCanvas);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
