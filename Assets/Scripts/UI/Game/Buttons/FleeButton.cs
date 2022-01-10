using UnityEngine;
using UnityEngine.EventSystems;

public class FleeButton : MonoBehaviour, IPointerClickHandler
{
    private GameCanvas _gameCanvas;

    public void FirstInitialize(GameCanvas gameCanvas)
    {
        _gameCanvas = gameCanvas;
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        _gameCanvas.GameManagement.PlayerManager.OnClickFlee();
    }
}
