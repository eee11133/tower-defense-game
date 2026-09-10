using Assets.Scripts.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObject : MonoBehaviour
{
    public Animator animator;
    public int HasMoney;

    public int DefultWeapon;

    private MonsterObject target;
    public Transform firepos;
   public  WeaponData weaponData;
    /// <summary>鼠标转向灵敏度（TPS 常用 1~4，值越大转得越快）。
    /// Mouse X 本身已是每帧增量，旋转量不再乘 Time.deltaTime，否则会与帧率²相关</summary>
    public float mouseSensitivity = 2f;
    public Vector3 monsterPos;
    // Start is called before the first frame update
    void Start()
    {
        
        animator =this.gameObject.GetComponent<Animator>();
    }
    public void InitPlayer(int HasMoney, int DefultWeapon)
    {
        this.HasMoney = HasMoney;
        this.DefultWeapon = DefultWeapon;
        weaponData = DataManager.Instance.WeaponDataList[DefultWeapon-1];

        UpDateMoney();
    }
    // Update is called once per frame
    void Update()
    {
        // 游戏结束后禁止控制角色：输入全部失效，移动动画参数归零回到待机
        if (LevelDataMgr.Instance.isGameOver)
        {
            animator.SetFloat("XSpeed", 0);
            animator.SetFloat("YSpeed", 0);
            return;
        }

        animator.SetFloat("XSpeed", Input.GetAxis("Horizontal"));
        animator.SetFloat("YSpeed", Input.GetAxis("Vertical"));

        // 鼠标锁定屏幕中心（GamePanel 中设置 Locked），水平移动量控制角色左右转向。
        // Mouse X 已是"本帧移动增量"，不乘 deltaTime；鼠标不动时增量为 0，角色不转，
        // 不会出现指向式那种小幅移动导致的剧烈旋转，也无相机跟随反馈环
        this.transform.Rotate(Vector3.up, Input.GetAxis("Mouse X") * mouseSensitivity);


        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            animator.SetLayerWeight(1, 1);
        }
        else  if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            animator.SetLayerWeight(1, 0);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            animator.SetTrigger("Roll");
        }

        if (weaponData.bulletNum > 0 && firepos != null)
        {
            atkRandom();
        }
        // 2. �������������ս�������ֶ�����
        else if (Input.GetMouseButtonDown(0) && firepos == null)
        {
            animator.SetTrigger("Fire");
            // ��ս������ KnifeEvent ������ͨ�������¼���
        }

    }

    public void KnifeEvent()
    {
        Collider[] collider=Physics.OverlapSphere(this.transform.position + this.transform.forward + this.transform.up, 1, 1 << LayerMask.NameToLayer("Monster"));

        DataManager.Instance.PlaySound("Music/Knife");

        for(int i=0; i<collider.Length; i++)
        {
            MonsterObject monster = collider[i].gameObject.GetComponent<MonsterObject>();

            if(monster != null&&!monster.isDead)
            {
              

                monster.Wound(DataManager.Instance.WeaponDataList[DefultWeapon].atk);
            }
        }
    }

    public void ShootEvent()
    {

        //RaycastHit[] hits = Physics.RaycastAll(new Ray(this.transform.position, this.transform.forward), 1000, 1 << LayerMask.NameToLayer("Monster"));

        //DataManager.Instance.PlaySound("Music/Gun");

        //for (int j = 0; j < hits.Length; j++)
        //{
        //    MonsterObject monster = hits[j].collider.gameObject.GetComponent<MonsterObject>();

        //    if (monster != null && !monster.isDead)
        //    {

        //        GameObject obj = Resources.Load<GameObject>(DataManager.Instance.WeaponDataList[DefultWeapon].hitEff);
        //        GameObject effobj = Instantiate(obj, hits[j].transform.position, Quaternion.LookRotation(hits[j].normal));

        //        Destroy(effobj, 1);
        //        monster.Wound(DataManager.Instance.WeaponDataList[DefultWeapon].atk);
        //    }
        //}

        if (target != null && !target.isDead)
        {
            target.Wound(weaponData.atk);
            DataManager.Instance.PlaySound("Music/Gun");
            // 改用特效对象池播放命中特效；0.2s 对应旧版 Destroy 的定时销毁时长
            EffectMgr.Instance.PlayEffect(weaponData.hitEff, monsterPos, Quaternion.LookRotation(monsterPos.normalized), 0.2f);
        }



    }

    public void atkRandom()
    {
        // 计算当前视线方向（相机朝向的水平投影），相机缺失时退化为角色自身朝向
        Camera mainCamera = Camera.main;
        Vector3 viewForward = mainCamera != null ? mainCamera.transform.forward : this.transform.forward;
        viewForward.y = 0;
        viewForward.Normalize();

        // 目标失效（不存在/死亡/超射程/不在视线前方）时重新挑选最佳目标。
        // 特别注意"不在前方"这一条：旧逻辑中背后的怪会一直霸占目标位，
        // 导致正前方的敌人反而打不中
        if (target == null ||
        target.isDead ||
        Vector3.Distance(this.gameObject.transform.position, target.transform.position) > weaponData.range ||
        !IsTargetInFront(target.transform.position, viewForward))
        {
            target = FindBestTarget(viewForward);
        }

        if (target == null)
        {
            return;
        }

        monsterPos = target.transform.position;

        // 角色朝向完全交给鼠标转向（见 Update 中的增量旋转），射击时不再自动转向，
        // 避免两套旋转互相拉扯；ShootEvent 为自动锁定命中，不依赖朝向
        animator.SetTrigger("Fire");
    }

    /// <summary>
    /// 判断目标是否位于视线前方 90° 半区内（水平面投影后点乘大于 0）。
    /// </summary>
    private bool IsTargetInFront(Vector3 targetPos, Vector3 viewForward)
    {
        Vector3 dir = targetPos - this.transform.position;
        dir.y = 0;
        return Vector3.Dot(viewForward, dir.normalized) > 0f;
    }

    /// <summary>
    /// 从射程内的怪物中挑选最佳射击目标（两级择优）：
    /// 第一级：视野锥内（与视线夹角 60° 内，dot > 0.5）选距离最近的一个——
    ///         优先打准星方向的敌人，多只重叠时打最近的；
    /// 第二级：锥内没有目标时，退而选前方 90° 半区内视线夹角最小的一个；
    /// 全部在身后则返回 null（无法射击）。
    /// 解决旧逻辑按列表顺序取目标、导致面前的敌人被其它方向目标挡住的问题。
    /// </summary>
    private MonsterObject FindBestTarget(Vector3 viewForward)
    {
        List<MonsterObject> monsters = LevelDataMgr.Instance.findMonsters(this.transform.position, weaponData.range);
        if (monsters.Count == 0)
        {
            return null;
        }

        MonsterObject coneTarget = null;    // 视野锥内距离最近的目标
        float coneNearestDist = float.MaxValue;
        MonsterObject frontTarget = null;   // 前方半区内夹角最小的目标
        float frontBestDot = float.MinValue;

        for (int i = 0; i < monsters.Count; i++)
        {
            Vector3 dir = monsters[i].transform.position - this.transform.position;
            dir.y = 0;
            float dist = dir.magnitude;
            if (dist < 0.01f)
            {
                // 贴脸的怪物直接锁定，无需比较
                return monsters[i];
            }
            dir /= dist;

            float dot = Vector3.Dot(viewForward, dir);
            if (dot <= 0f)
            {
                continue; // 身后的怪物不参与选目标
            }

            // 第一级：视野锥内取最近
            if (dot > 0.5f && dist < coneNearestDist)
            {
                coneNearestDist = dist;
                coneTarget = monsters[i];
            }

            // 第二级：前方半区内取夹角最小（dot 最大）
            if (dot > frontBestDot)
            {
                frontBestDot = dot;
                frontTarget = monsters[i];
            }
        }

        return coneTarget != null ? coneTarget : frontTarget;
    }
    public void UpDateMoney()
    {
        UIManager.Instance.GetPanel<GamePanel>().UpdateMoney(HasMoney);
    }

    public void AddMoney(int money)
    {
        this.HasMoney += money;
        UpDateMoney();
    }
}
