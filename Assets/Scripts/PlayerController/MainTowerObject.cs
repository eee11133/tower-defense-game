using Assets.Scripts.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainTowerObject : MonoBehaviour
{
    // Start is called before the first frame update

    private int HP;
    private int MaxHP;
    private bool isDead;
    private static MainTowerObject instance;

    public static MainTowerObject Instance => instance;

    private void Awake()
    {
        instance = this;
    }

    public void UpDateHp(int hp, int maxHP)
    {
        this.HP = hp;
        this.MaxHP = maxHP;

        UIManager.Instance.GetPanel<GamePanel>().UpdateHp(hp, maxHP);
    }

    public void Wound(int dmg)
    {
        if(isDead) return;
        HP-=dmg;
        if(HP <= 0)
        {
            isDead = true;
            HP = 0;

            // 游戏失败：冻结战场（怪物停止活动、玩家停止控制、波次停止刷新）
            LevelDataMgr.Instance.GameOver();

                UIManager.Instance.ShowPanel<GameOverPanel>().InitInfo((int)(LevelDataMgr.Instance.playerObject.HasMoney*0.5f), false);


        }
        UpDateHp(HP, MaxHP);
    }
    // Update is called once per frame
    private void OnDestroy()
    {
        instance = null;
    }
}
