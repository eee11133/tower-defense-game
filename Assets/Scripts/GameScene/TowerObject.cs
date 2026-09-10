using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerObject : MonoBehaviour
{

    public Transform Body;
    public Transform FirePos;
    private float RotateSpeed = 20;
    private MonsterObject target;
    private List<MonsterObject> targets;
    private TowerData towerdata;
    private float nowTime;

    private Vector3 monsterPos;


    // Update is called once per frame
    void Update()
    {
        // 游戏结束后炮塔停止索敌、旋转与攻击
        if (LevelDataMgr.Instance.isGameOver)
        {
            return;
        }

        if (towerdata.type == 1)
        {
            if (target == null ||
                target.isDead ||
                Vector3.Distance(this.gameObject.transform.position, target.transform.position) > towerdata.atkRange)
            {
               target= LevelDataMgr.Instance.findMonster(this.transform.position,towerdata.atkRange);

             

            }

            if (target == null)
            {
                return;
            }
            monsterPos = target.transform.position;

            monsterPos.y = Body.transform.position.y;
            Body.rotation = Quaternion.Slerp(Body.rotation, Quaternion.LookRotation(monsterPos - Body.position), RotateSpeed * Time.deltaTime);
           if( Vector3.Angle(Body.forward,monsterPos-Body.position) < 5&&Time.time-nowTime>=towerdata.offsetTime)
            {
                nowTime = Time.time;
                target.Wound(towerdata.atk);

                DataManager.Instance.PlaySound("Music/Tower");

                // 改用特效对象池播放攻击特效；0.2s 对应旧版 Destroy 的定时销毁时长
                EffectMgr.Instance.PlayEffect(towerdata.eff, FirePos.position, FirePos.rotation, 0.2f);

            }


        }
        else
        {
            targets = LevelDataMgr.Instance.findMonsters(this.transform.position, towerdata.atkRange);

         
            if( Time.time - nowTime >= towerdata.offsetTime)
            {
                // 改用特效对象池播放范围攻击特效；0.3s 对应旧版 Destroy 的定时销毁时长
                EffectMgr.Instance.PlayEffect(towerdata.eff, FirePos.position, FirePos.rotation, 0.3f);
                nowTime = Time.time;
                if (targets.Count > 0)
                {
                    for (int i = 0; i < targets.Count; i++)
                    {
                        targets[i].Wound(towerdata.atk);
                    }
                }
            }

        }


    }


 

    public void Initinfo(TowerData towerdata)
   {
        this.towerdata = towerdata;
    }
}
