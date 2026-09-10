using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MonsterPoint : MonoBehaviour
{
    // Start is called before the first frame update
    //最大波数
    public int maxWave;
    //当前波次的怪物个数
    public int monsterNumOneWave;
    //当前有多少只
    private int nowNum;


    public List<int> MonsterId;
    //怪物id
    private int NowId;

    public float createOffsetTime;

    public float delayTime;

    public float firstDelayTime;

    private void CreatWave()
    {
        NowId= MonsterId[Random.Range(0, MonsterId.Count)];

        nowNum=monsterNumOneWave;

        CreatMonster();

        LevelDataMgr.Instance.ChangeWave(1);

        maxWave--;
    }

    private void CreatMonster()
    {
        MonsterData info=DataManager.Instance.monsterDataList[NowId-1];

        GameObject obj = Instantiate(Resources.Load<GameObject>(info.res),this.transform.position,Quaternion.identity);

        MonsterObject monsterobj = obj.AddComponent<MonsterObject>();

        monsterobj.InitInfo(info);

        LevelDataMgr.Instance.AddMonster(monsterobj);

        nowNum--;
        if(nowNum == 0)
        {
            if(maxWave>0)
            Invoke("CreatWave", delayTime);
        }
        else
        {

            Invoke("CreatMonster", createOffsetTime);
        }
    }

    void Start()
    {
        Invoke("CreatWave", firstDelayTime);
        LevelDataMgr.Instance.AddPoint(this);
     
       
        LevelDataMgr.Instance.UpDataWave(maxWave);
    }

    public bool CheckOver()
    {
        return nowNum == 0 && maxWave == 0;
    }

}
