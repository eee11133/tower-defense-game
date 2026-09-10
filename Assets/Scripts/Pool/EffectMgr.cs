using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EffectMgr
{
    private static EffectMgr instance = new EffectMgr();
    public static EffectMgr Instance => instance;

    /// <summary>每种特效的最大实例总数（活跃中 + 空闲队列），超过后新播放请求直接放弃</summary>
    private const int MaxInstancesPerEffect = 20;

    /// <summary>每种特效空闲队列最多保留数量，回收时超出的实例直接销毁释放内存</summary>
    private const int MaxIdlePerEffect = 10;

    private readonly GameObject poolRoot;

    /// <summary>协程驱动组件：纯 C# 类本身不能启动协程，借它运行回收协程</summary>
    private readonly PoolDriver driver;

    /// <summary>空闲对象池：key = Resources 路径，value = 空闲特效队列</summary>
    private readonly Dictionary<string, Queue<GameObject>> effectPool = new Dictionary<string, Queue<GameObject>>();

    /// <summary>每种特效已创建的实例总数（活跃中 + 空闲中），用于总实例数上限判定</summary>
    private readonly Dictionary<string, int> instanceCount = new Dictionary<string, int>();

    /// <summary>预制体缓存：key = Resources 路径，避免重复加载</summary>
    private readonly Dictionary<string, GameObject> prefabCache = new Dictionary<string, GameObject>();

    /// <summary>特效存活时长硬上限（秒）：调用方传入的时长会被钳制到该值以内，
    /// 防止误配为循环(loop)的特效长期占用对象</summary>
    private const float MaxLifetime = 5f;

    private EffectMgr()
    {
        // 创建池根节点并跨场景持久化（符合项目"功能分组父物体"规范）
        poolRoot = new GameObject("EffectPool");
        GameObject.DontDestroyOnLoad(poolRoot);
        driver = poolRoot.AddComponent<PoolDriver>();
    }

    /// <summary>
    /// 播放特效：池中有空闲对象则复用，否则加载预制体实例化新对象。
    /// </summary>
    /// <param name="resPath">Resources 目录下的预制体路径（如 "Effect/TowerFire"）</param>
    /// <param name="position">生成世界坐标</param>
    /// <param name="rotation">生成朝向</param>
    /// <param name="maxLifeTime">特效可见时长（秒）：到期强制清空并回收，对应旧版
    /// Destroy(effobj, x) 的定时销毁语义</param>
    public void PlayEffect(string resPath, Vector3 position, Quaternion rotation, float maxLifeTime = 0.3f)
    {
        if (string.IsNullOrEmpty(resPath))
        {
            return;
        }

        GameObject effect = GetEffect(resPath);
        if (effect == null)
        {
            // 返回 null 有两种情况：资源加载失败（LoadPrefab 内已打警告）
            // 或实例数已达上限（性能保护，属预期行为，静默放弃本次播放）
            return;
        }

        // ===== 取出闭环：重置状态，保证复用时不残留上一次表现 =====
        effect.transform.SetPositionAndRotation(position, rotation);
        effect.SetActive(true);

        // 停掉并清空上一次残留粒子，再重新播放。
        // 只对"根粒子系统"（父级链上没有其它粒子系统的）操作并级联所有子节点，

        ParticleSystem[] systems = effect.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < systems.Length; i++)
        {
            ParticleSystem ps = systems[i];
            // 父级链上还存在其它粒子系统 → 它是子节点，由根系统的级联统一控制
            if (ps.transform.parent != null &&
                ps.transform.parent.GetComponentInParent<ParticleSystem>() != null)
            {
                continue;
            }
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play(true);
        }

        // 启动回收协程：到达指定时长后清空粒子并归池；
        // 时长用 MaxLifetime 钳制，防御调用方传入异常大的值
        driver.StartCoroutine(RecycleWhenFinished(resPath, effect, Mathf.Min(maxLifeTime, MaxLifetime)));
    }

    /// <summary>
    /// 从池中取出特效：优先复用空闲对象；池空时未达上限才实例化新对象，
    /// 已达总实例数上限则返回 null（本次播放放弃）。
    /// </summary>
    private GameObject GetEffect(string resPath)
    {
        // ① 池中有空闲对象：直接出队复用
        if (effectPool.TryGetValue(resPath, out Queue<GameObject> queue) && queue.Count > 0)
        {
            return queue.Dequeue();
        }

        // ② 池空：检查该特效总实例数是否已达上限（所有实例都在活跃中）
        instanceCount.TryGetValue(resPath, out int count);
        if (count >= MaxInstancesPerEffect)
        {
            return null;
        }

        // ③ 未达上限：加载预制体（带缓存）并实例化到池根节点下
        GameObject prefab = LoadPrefab(resPath);
        if (prefab == null)
        {
            return null;
        }
        GameObject effect = GameObject.Instantiate(prefab, poolRoot.transform);
        instanceCount[resPath] = count + 1;
        return effect;
    }

    /// <summary>加载预制体并缓存，避免重复 Resources.Load；加载失败时给出警告</summary>
    private GameObject LoadPrefab(string resPath)
    {
        if (!prefabCache.TryGetValue(resPath, out GameObject prefab))
        {
            prefab = Resources.Load<GameObject>(resPath);
            prefabCache[resPath] = prefab;
            if (prefab == null)
            {
                Debug.LogWarning("特效资源加载失败：" + resPath);
            }
        }
        return prefab;
    }

    /// <summary>
    /// 回收协程：等待调用方指定的存活时长后，强制停止并清空全部粒子（含子级），
    /// 再失活归池。循环(loop)特效、未播完的特效在此被彻底清干净，
    /// 保证归还后不带任何残留表现。
    /// </summary>
    private IEnumerator RecycleWhenFinished(string resPath, GameObject effect, float deadline)
    {
        yield return new WaitForSeconds(deadline);

        // 归还前强制停止并清空所有粒子系统（含子级）：
        // 循环特效在此被彻底清干净，确保下次从池中取出播放时
        // 不会带着上一次的残留粒子（否则表现为旧位置上特效不消失）
        ParticleSystem[] allSystems = effect.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < allSystems.Length; i++)
        {
            allSystems[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        ReturnToPool(resPath, effect);
    }

    /// <summary>
    /// 归还闭环：失活 → 归位池根节点 → 空闲队列未满则入队复用，已满则销毁释放。
    /// </summary>
    private void ReturnToPool(string resPath, GameObject effect)
    {
        // 兜底：对象可能已被外部销毁（Unity 重载的 == null 判定），此时仅维护计数
        if (effect == null)
        {
            DecreaseCount(resPath);
            return;
        }

        effect.SetActive(false);
        effect.transform.SetParent(poolRoot.transform);

        if (!effectPool.TryGetValue(resPath, out Queue<GameObject> queue))
        {
            queue = new Queue<GameObject>();
            effectPool[resPath] = queue;
        }

        // 空闲队列已达保留上限：直接销毁该实例（战斗峰值期创建的多余对象
        // 不再常驻内存），并递减实例计数
        if (queue.Count >= MaxIdlePerEffect)
        {
            DecreaseCount(resPath);
            GameObject.Destroy(effect);
            return;
        }

        queue.Enqueue(effect);
    }

    /// <summary>递减指定特效的实例计数，并防止出现负数</summary>
    private void DecreaseCount(string resPath)
    {
        if (instanceCount.TryGetValue(resPath, out int count) && count > 0)
        {
            instanceCount[resPath] = count - 1;
        }
    }

    /// <summary>协程驱动组件：仅用于让纯 C# 管理器可以启动协程，不写任何业务逻辑</summary>
    private class PoolDriver : MonoBehaviour
    {
    }
}
