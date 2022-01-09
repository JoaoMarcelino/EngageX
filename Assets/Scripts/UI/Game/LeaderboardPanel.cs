using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LeaderboardPanel : MonoBehaviour
{
    private GameCanvas _gameCanvas;
    [SerializeField] private NicknamesPanel _nicknamePanel;

    [SerializeField] private ExpPanel _expPanel;

    public void FirstInitialize(GameCanvas gameCanvas)
    {
        _gameCanvas = gameCanvas;
        _nicknamePanel.FirstInitialize(gameCanvas);
        _expPanel.FirstInitialize(gameCanvas);
    }

    private void ResetPanels()
    {
        _nicknamePanel.Clear();
        _expPanel.Clear();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void RenderLeaderboard(List<PlayerStatusInfo> playerInfoList)
    {
        ResetPanels();

        playerInfoList = playerInfoList.OrderByDescending(x => x.Exp).Take(MasterManager.GameSettings.LeaderboardSize).ToList();

        foreach(PlayerStatusInfo playerStatusInfo in playerInfoList)
        {
            _nicknamePanel.Add(playerStatusInfo.NickName);
            _expPanel.Add(playerStatusInfo.Exp);
        }
    }
}
