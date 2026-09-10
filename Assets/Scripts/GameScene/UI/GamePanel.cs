using Assets.Scripts.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GamePanel : BasePanel
{
    public Button BtnBack;
    public Text EndTurn;
    public Text Money;
    public Text HpTxt;
    public int Hpwide = 500;
    public Image Hpimg;
    public Transform Bottransform;
    public List<TowerInfo> TowerInfo = new List<TowerInfo>();
    private TowerPoint nowtowerpoint;

    private   bool Checkinfo = false;
    public override void Init()
    {
        BtnBack.onClick.AddListener(BackToBegin);

        Bottransform.gameObject.SetActive(false);

        // 锁定鼠标到屏幕中心并隐藏指针，鼠标水平移动作为角色转向输入
        Cursor.lockState = CursorLockMode.Locked;

    }

    /// <summary>返回主菜单：由返回按钮或 Esc 键触发（战斗中鼠标锁定，按钮无法点击）。
    /// 必须先清理关卡数据：LevelDataMgr 是不随场景销毁的纯 C# 单例，
    /// 不清理的话切场景后怪物 GameObject 被销毁但仍残留在列表中（假空引用），
    /// 再次进入关卡时 findMonster 遍历会抛 MissingReferenceException</summary>
    private void BackToBegin()
    {
        UIManager.Instance.HidePanel<GamePanel>();
        LevelDataMgr.Instance.Clear();
        SceneManager.LoadScene("BeginScene");
    }

    public void UpdateMoney(int money)
    {
        Money.text = "金币:" + money;
    }
    public void UpdateHp(int hp,int hpMax)
    {
        HpTxt.text = hp + "/" + hpMax;
        (Hpimg.transform as RectTransform).sizeDelta = new Vector2((float)hp / hpMax * Hpwide, 40);

    }

    public void UpdateTurn(int nowturn,int maxturn)
    {
        EndTurn.text="总轮次:"+nowturn+"/"+maxturn;
    }
    public override void HideMe(UnityAction callback)
    {
        base.HideMe(callback);

        Cursor.lockState = CursorLockMode.None;
    }
    public void UpDateTower(TowerPoint point)
    {
        nowtowerpoint = point;

        if (nowtowerpoint == null)
        {
            Bottransform.gameObject.SetActive(false);

            Checkinfo = false;
        }
        else
        {
            Checkinfo = true;
            Bottransform.gameObject.SetActive(true);
            if (nowtowerpoint.nowtowerData == null)
            {
                for (int i = 0; i < TowerInfo.Count; i++)
                {
                    TowerInfo[i].gameObject.SetActive(true);
                    TowerInfo[i].InitInfo(nowtowerpoint.ChooseIds[i], "数字键" + (i + 1));
                }

            }
            else
            {
                for (int i = 0; i < TowerInfo.Count; i++)
                {
                    TowerInfo[i].gameObject.SetActive(false);

                }
                TowerInfo[1].gameObject.SetActive(true);
                TowerInfo[1].InitInfo(nowtowerpoint.nowtowerData.next, "空格键");
            }

        }

    }
    protected override void Update()
    {
        base.Update();
        // 游戏结束后禁止按键建塔/升级，防止结算面板弹出后仍能消耗金钱
        if (LevelDataMgr.Instance.isGameOver)
        {
            return;
        }

        // 鼠标锁定在屏幕中心时无法点击返回按钮，按 Esc 返回主菜单
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            BackToBegin();
            return;
        }

        if (Checkinfo == false)
        {
            return;
        }

        if (nowtowerpoint.nowtowerData == null)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                nowtowerpoint.CreatTower(nowtowerpoint.ChooseIds[0]);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {

                nowtowerpoint.CreatTower(nowtowerpoint.ChooseIds[1]);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {

                nowtowerpoint.CreatTower(nowtowerpoint.ChooseIds[2]);
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                nowtowerpoint.CreatTower(nowtowerpoint.nowtowerData.next);
            }
        }


    }

}
