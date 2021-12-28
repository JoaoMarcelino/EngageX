using UnityEngine;
using UnityEngine.EventSystems;
public class UpButton : MonoBehaviour, IPointerClickHandler
{
    private GameCanvas _gameCanvas;
    public void FirstInitialize(GameCanvas gameCanvas)
    {
        _gameCanvas = gameCanvas;
    }
    public void OnPointerClick(PointerEventData pointerEventData)
    {
        _gameCanvas.Player.onClickLevelUp();
    }
}