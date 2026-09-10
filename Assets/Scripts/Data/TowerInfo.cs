using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerInfo : MonoBehaviour
{

    public Text Keyname;
    public Text MoneyTxt;
    public Image Towerimg;
    // Start is called before the first frame update
    public void InitInfo(int id,string InputString)
    {
         TowerData info = DataManager.Instance.TowerDataList[id-1];
        MoneyTxt.text = "￥"+info.money.ToString();
        Towerimg.sprite = Resources.Load<Sprite>(info.imgRes);

        Keyname.text = InputString;
        if (info.money > LevelDataMgr.Instance.playerObject.HasMoney)
        {
            MoneyTxt.text = "金钱不足";
        }

    }
}
