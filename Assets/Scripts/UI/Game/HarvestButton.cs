using UnityEngine;
using UnityEngine.EventSystems;

public class HarvestButton : MonoBehaviour, IPointerClickHandler
{
    private GameCanvas _gameCanvas;
    public void FirstInitialize(GameCanvas gameCanvas)
    {
        _gameCanvas = gameCanvas;
    }
    public void OnPointerClick(PointerEventData pointerEventData)
    {
        _gameCanvas.Player.OnClickHarvest();
    }
}
