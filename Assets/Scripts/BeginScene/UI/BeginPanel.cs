using Assets.Scripts.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class BeginPanel : BasePanel
{
    public Button btnStart;
    public Button btnQuit;
    public Button btnSetting;
    public Button btnAbout;

    public override void Init()
    {
        btnStart.onClick.AddListener(() =>
        {
            //显示选角面板的逻辑
            Camera.main.GetComponent<CarmraAnimator>().TurnLeft(() =>
            {
                //显示选角界面
                UIManager.Instance.ShowPanel<RolePanel>();
            });

            UIManager.Instance.HidePanel<BeginPanel>();
        });
        btnAbout.onClick.AddListener(() =>
        {
            //显示制作人信息的逻辑
        });
        btnSetting.onClick.AddListener(() =>
        {
            //设置面板
            UIManager.Instance.ShowPanel<SettingPanel>();
        });
        btnQuit.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }


}
