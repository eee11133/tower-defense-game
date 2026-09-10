using Assets.Scripts.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TipPanel : BasePanel
{

    public Text Tiptext;

    public Button BtnCheck;
    public override void Init()
    {
        BtnCheck.onClick.AddListener(() =>
        {
            UIManager.Instance.HidePanel<TipPanel>();

        });
    }

    public void ChangeInfo(string text)
    {
        Tiptext.text = text;
    }
}
