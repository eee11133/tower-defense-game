using Assets.Scripts.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class GameOverPanel : BasePanel
{

    public Text tiptext;
    public Text WinInfo;
    public Text MoneyInfo;
    public Button BtnCheck;
    public override void Init()
    {
        // 结算面板需要点击"确认"按钮，解锁并显示鼠标（战斗中为 Locked 锁定状态）
        Cursor.lockState = CursorLockMode.None;

        BtnCheck.onClick.AddListener(() =>
        {
            DataManager.Instance.SavePlayerData();
            UIManager.Instance.HidePanel<GameOverPanel>();
            UIManager.Instance.HidePanel<GamePanel>();

            LevelDataMgr.Instance.Clear();
           
            SceneManager.LoadScene("BeginScene");
            

        });
    }

    public void InitInfo(int money, bool isWin)
    {


        tiptext.text = isWin ? "胜利" : "失败";
        WinInfo.text = isWin ? "获得胜利奖励" : "获得失败奖励";
        MoneyInfo.text = "金币" + money;


        DataManager.Instance.playerData.HasMoney += LevelDataMgr.Instance.playerObject.HasMoney;

    
    }
}
