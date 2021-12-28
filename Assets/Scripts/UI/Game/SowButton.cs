using UnityEngine;
using UnityEngine.EventSystems;

public class SowButton : MonoBehaviour, IPointerClickHandler
{
    private GameCanvas _gameCanvas;
    public void FirstInitialize(GameCanvas gameCanvas)
    {
        _gameCanvas = gameCanvas;
    }
    public void OnPointerClick(PointerEventData pointerEventData)
    {
        _gameCanvas.Player.OnClickSow();
    }
}