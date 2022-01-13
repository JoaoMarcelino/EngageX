using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavigationControls : MonoBehaviour
{
    [SerializeField] private SowButton _sowButton;
    [SerializeField] private HarvestButton _harvestButton;
    [SerializeField] private UpButton _upButton;
    [SerializeField] private FixedJoystick _fixedJoystick;
    
    public FixedJoystick FixedJoystick { get { return _fixedJoystick;}}

    private GameCanvas _gameCanvas;

    public void FirstInitialize(GameCanvas gameCanvas)
    {
        _gameCanvas = gameCanvas;
        _sowButton.FirstInitialize(gameCanvas);
        _harvestButton.FirstInitialize(gameCanvas);
        _upButton.FirstInitialize(gameCanvas);
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        FixedJoystick.ReleaseJoystick();
        gameObject.SetActive(false);
    }
}
