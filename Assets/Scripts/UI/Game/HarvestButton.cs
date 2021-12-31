using UnityEngine;
using UnityEngine.EventSystems;

public class HarvestButton : MonoBehaviour, IPointerClickHandler
{
    private GameManagement _gameManagement;

    public void FirstInitialize(GameManagement gameManagement)
    {
        _gameManagement = gameManagement;
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        _gameManagement.PlayerManager.OnClickHarvest();
    }
}
