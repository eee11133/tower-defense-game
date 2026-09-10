using Assets.Scripts.UI;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class LevelDataMgr
{
    private static LevelDataMgr instance=new LevelDataMgr();

    public static LevelDataMgr Instance=>instance;

    public Transform HeroPos;
    public PlayerObject playerObject;



    public List<MonsterPoint> points=new List<MonsterPoint>();

    public List<MonsterObject> monsterObjects=new List<MonsterObject>();



    public int MaxWave;
    public int NowWave;

    /// <summary>游戏结束总开关：胜利或失败后置为 true，怪物/塔/玩家据此冻结，
    /// 由两个结束点（主塔血量归零、最后一只怪死亡）通过 GameOver() 统一触发</summary>
    public bool isGameOver = false;

    /// <summary>
    /// 游戏结束（胜利或失败）时调用：
    /// 置结束开关，怪物停止活动进入待机、玩家与炮塔停止响应、波次停止刷新。
    /// </summary>
    public void GameOver()
    {
        if (isGameOver)
        {
            return;
        }
        isGameOver = true;

        // 取消所有出生点的 Invoke，停止后续波次与怪物的生成
        for (int i = 0; i < points.Count; i++)
        {
            points[i].CancelInvoke();
        }
    }

    public void InitInfo(MapData mapinfo)
    {//��ʾ��Ϸ����
        UIManager.Instance.ShowPanel<GamePanel>();

        RoleData roleData = DataManager.Instance.nowrole;

        

        HeroPos = GameObject.Find("HeroPos").transform;


        //��ȡѡ��Ľ�ɫ��Ϣ
        GameObject player = GameObject.Instantiate(Resources.Load<GameObject>(roleData.res), HeroPos.position,HeroPos.rotation);
         playerObject =player.GetComponent<PlayerObject>();

        playerObject.InitPlayer(mapinfo.money, roleData.defaultWeapon);

        Camera.main.GetComponent<CarmeraMove>().target=player.transform;


        MainTowerObject.Instance.UpDateHp(mapinfo.towerHp, mapinfo.towerHp);
        
    }
    public void AddPoint(MonsterPoint point)
    {
        points.Add(point);
    }
    public bool CheckOver()
    {
        for (int i = 0; i < points.Count; i++)
        {
            if (!points[i].CheckOver())
            {
                return false;
            }
        }
        if (monsterObjects.Count > 0)
        {
            return false;
        }
        return true;
    }

    public void UpDataWave(int maxwave)
    {
        MaxWave+=maxwave;

        NowWave = MaxWave;

        UIManager.Instance.GetPanel<GamePanel>().UpdateTurn(NowWave,MaxWave);

    }

    public void ChangeWave(int wave)
    {
        NowWave-=wave;
        UIManager.Instance.GetPanel<GamePanel>().UpdateTurn(NowWave, MaxWave);
    }
  

    public void AddMonster(MonsterObject obj)
    {
        monsterObjects.Add(obj);
    }

    public void Remove(MonsterObject obj)
    {
        monsterObjects.Remove(obj);
    }
    public void Clear()
    {
        monsterObjects.Clear();
        MaxWave = 0;
        NowWave = 0;
        // 重置游戏结束开关，供下一次进入关卡使用
        isGameOver = false;
        playerObject=null;

    points.Clear();

    }
    public MonsterObject findMonster(Vector3 pos, int range)
    {

        for (int i = 0; i < monsterObjects.Count; i++)
        {
            MonsterObject m = monsterObjects[i];
            // Unity 假空引用防御：对象已被销毁（如切场景未清理列表）时
            // m == null 成立，将其移出列表避免访问已销毁对象抛 MissingReferenceException
            if (m == null)
            {
                monsterObjects.RemoveAt(i);
                i--;
                continue;
            }
            if (!m.isDead && Vector3.Distance(pos, m.transform.position) <= range)
            {
                return m;
            }
        }
        return null;
    }
    public List<MonsterObject> findMonsters(Vector3 pos, int range)
    {
        List<MonsterObject> monsterRange = new List<MonsterObject>();
        // 倒序遍历便于直接移除失效引用
        for (int i = monsterObjects.Count - 1; i >= 0; i--)
        {
            MonsterObject m = monsterObjects[i];
            if (m == null)
            {
                monsterObjects.RemoveAt(i);
                continue;
            }
            if (!m.isDead && Vector3.Distance(pos, m.transform.position) <= range)
            {

                monsterRange.Add(m);
            }
        }
        return monsterRange;
    }


}

