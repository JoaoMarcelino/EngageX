using UnityEngine;
using UnityEngine.EventSystems;

public class SowButton : MonoBehaviour, IPointerClickHandler
{
    private GameManagement _gameManagement;

    public void FirstInitialize(GameManagement gameManagement)
    {
        _gameManagement = gameManagement;
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        _gameManagement.PlayerManager.OnClickSow();
    }
}