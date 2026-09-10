using Assets.Scripts.UI;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;

public class MonsterObject : MonoBehaviour
{

    private Animator animator;

    private NavMeshAgent agent;

    private MonsterData monsterData;

    private int hp;

   public bool isDead=false;

    public float frontTime = 0;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    public void InitInfo(MonsterData Data)
    {

        this.monsterData = Data;
        animator.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>(Data.animator);

        hp = Data.hp;

        agent.speed = agent.acceleration = Data.moveSpeed;

        agent.angularSpeed = Data.roundSpeed;
    }

    public void Wound(int dmg)
    {
        if (isDead)
        {
            return;

        }
        hp-=dmg;
        animator.SetTrigger("wound");
        if(hp <= 0)
        {
            Dead();


        }
        else
        {
            DataManager.Instance.PlaySound("Music/Wound");
        }
     
    }

    public void Dead()
    {
        isDead = true;
        agent.isStopped = true;
         animator.SetBool("Dead", true);
        LevelDataMgr.Instance.playerObject.AddMoney(30);
        //��Ǯ
        DataManager.Instance.PlaySound("Music/dead");
        //�Ƴ�ʬ��



    }

    public void DeadEvent()
    {
        LevelDataMgr.Instance.Remove(this);

        Destroy(this.gameObject);

        // 已结束（如失败后残怪死亡动画播完）不重复结算；全部清空则胜利
        if (!LevelDataMgr.Instance.isGameOver && LevelDataMgr.Instance.CheckOver())
        {
            // 游戏胜利：冻结战场（玩家停止控制、波次停止刷新）
            LevelDataMgr.Instance.GameOver();

            UIManager.Instance.ShowPanel<GameOverPanel>().InitInfo(LevelDataMgr.Instance.playerObject.HasMoney,true);

        }
    }
    
    public void BornOver()
    {
        agent.SetDestination(MainTowerObject.Instance.transform.position);

        animator.SetBool("Run",true);

    }
    // Start is called before the first frame update
   public void AtkEvent()
    {
        Collider[] collider=Physics.OverlapSphere(this.transform.position + this.transform.forward + this.transform.up, 1, 1 << LayerMask.NameToLayer("MainTower"));
        DataManager.Instance.PlaySound("Music/Eat");
        for (int i = 0;i < collider.Length; i++)
        {
            if (MainTowerObject.Instance.gameObject == collider[i].gameObject)
            {
                MainTowerObject.Instance.Wound(monsterData.atk);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead == true)
            return;
        // 游戏结束后怪物冻结：停寻路、清攻击指令、切待机动画
        if (LevelDataMgr.Instance.isGameOver)
        {
            agent.isStopped = true;
            animator.SetBool("Run", false);
            animator.ResetTrigger("attack");
            return;
        }
        if (Vector3.Distance(this.transform.position, MainTowerObject.Instance.transform.position) < 3
          && Time.time - frontTime >= monsterData.atkOffset)
        {
            animator.SetBool("Run", false);
            animator.SetTrigger("attack");
            frontTime = Time.time;
        }
    }
}
