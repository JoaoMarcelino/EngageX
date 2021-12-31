using UnityEngine;
using UnityEngine.EventSystems;

public class UpButton : MonoBehaviour
{
    private GameManagement _gameManagement;

    public void FirstInitialize(GameManagement gameManagement)
    {
        _gameManagement = gameManagement;
    }

    public void OnPointerClick(PointerEventData pointerEventData)
    {
        _gameManagement.PlayerManager.OnClickLevelUp();
    }
}