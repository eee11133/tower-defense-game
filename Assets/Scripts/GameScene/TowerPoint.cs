using Assets.Scripts.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerPoint : MonoBehaviour
{

    private GameObject towerObj = null;

    public TowerData nowtowerData=null;

    public List<int> ChooseIds = new List<int>();
    // Start is called before the first frame update

    public void CreatTower(int id)
    {
        TowerData info =DataManager.Instance.TowerDataList[id-1];
        if (info.money > LevelDataMgr.Instance.playerObject.HasMoney)
        {
            return;
        }
        LevelDataMgr.Instance.playerObject.AddMoney(-info.money);
        if(towerObj != null)
        {
            Destroy(towerObj);
            towerObj = null;
        }
        towerObj = Instantiate(Resources.Load<GameObject>(info.res),this.transform.position,Quaternion.identity);
        towerObj.GetComponent<TowerObject>().Initinfo(info);

        nowtowerData = info;
        if (nowtowerData.next != 0)
        {
            UIManager.Instance.GetPanel<GamePanel>().UpDateTower(this);
        }
        else
        {

            UIManager.Instance.GetPanel<GamePanel>().UpDateTower(null);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (nowtowerData != null && nowtowerData.next == 0)
        {
            return;
        }
        
        UIManager.Instance.GetPanel<GamePanel>().UpDateTower(this);
    }
    private void OnTriggerExit(Collider other)
    {

        UIManager.Instance.GetPanel<GamePanel>().UpDateTower(null);
    }
}
