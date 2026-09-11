# 塔防游戏（Tower Defense）

一款基于 Unity 开发的 **3D 第三人称塔防游戏**：玩家在地图上建造并升级炮塔抵御多波次怪物进攻，同时可亲自操控角色使用近战/远程武器参战，守护主塔直至全部波次清空即胜利。

> 建议运行 Unity 版本：**Unity 2022.3 LTS**

---

## 游戏玩法

- **塔防核心**：怪物由多个出生点按波次生成，沿 NavMesh 寻路进攻主塔；玩家在建造点花费金币建造炮塔（单体/范围两种攻击模式），并可逐级升级
- **角色参战**：第三人称操控角色移动（WASD）、鼠标转向、近战挥刀/远程射击，武器与角色可在主界面选择
- **经济与结算**：击杀怪物获得金币，胜利后金币存入存档；主塔被攻破则失败结算

---

## 技术栈

Unity、C#、UGUI、NavMesh 寻路、Animator 动画事件、LitJson / JsonUtility 数据持久化、协程与 Invoke 时序控制、物理检测（OverlapSphere / Raycast / Plane）、自研对象池、单例 / 模板方法等设计模式

---

## 核心技术亮点

- **UI 管理框架**：基于泛型反射（`ShowPanel<T>()`）+ Dictionary 缓存实现 UIManager 面板管理，CanvasGroup.alpha 控制淡入淡出过渡与回调销毁，Canvas 节点 DontDestroyOnLoad 跨场景持久化，统一管理全流程 8+ 个 UI 面板；BasePanel 采用模板方法模式，子类仅需重写 Init 绑定事件。

- **JSON 数据驱动与存档体系**：角色、塔、怪物、武器等 7 类配置以纯 C# POCO 类 + JSON 存储，配置与逻辑分离；封装 JsonMgr 支持 LitJson / JsonUtility 双方案切换（策略模式），采用 StreamingAssets（只读默认配置）+ PersistentDataPath（可写存档）双路径读取，配置热更与玩家存档分离。

- **分层单例管理器架构**：数据层（DataManager）、UI 层（UIManager）、关卡逻辑层（LevelDataMgr）职责分离；关卡管理器作为战斗目标检索中心统一维护怪物列表，提供基于距离的单体/范围目标检索供塔与玩家武器复用，并基于 Unity "假空引用"（Fake Null）语义在遍历中自动清理失效对象引用。

- **塔防核心战斗闭环**：NavMeshAgent 怪物自动寻路、Invoke 递归驱动的多出生点波次生成器（波数/单波数量/间隔可配置）、Quaternion.Slerp 平滑旋转锁定的炮塔单体/范围攻击、**Animation Event 动画事件驱动伤害判定**（攻击/死亡/出生回调与动画帧精确同步）、Trigger 触发器检测的炮塔建造与升级链。

- **通用特效对象池（EffectMgr）**：按 Resources 路径分桶 Queue 复用 + 预制体字典缓存 + 协程定时回收，替代高频 Instantiate/Destroy；设计**双层容量策略**（总实例数硬上限防同屏特效爆炸 + 空闲队列上限保证峰值后内存回落），并完成拒绝式与固定容量抢占式（LRU 轮询覆盖）两种池满策略的选型分析；处理循环粒子复用的 Stop+Clear 状态重置，战斗高频特效 GC Alloc 降至接近 0。

- **战斗系统深度优化**：重写玩家射击目标选择算法，从"列表顺序取第一只"改为**两级择优**（视野锥 60° 内距离最近优先、退而取前方半区视线夹角最小），解决背后目标霸占锁定位导致面前敌人无法命中的问题；实现 isGameOver 全局状态冻结机制（各系统 Update 轮询 + CancelInvoke 停刷怪），统一胜负结算链路。

---

## 目录结构

```
Assets/
├── Scripts/
│   ├── UI/                  # UI 框架：UIManager（面板管理）、BasePanel（模板方法基类）
│   ├── Pool/                # EffectMgr：通用特效对象池
│   ├── Json/                # JsonMgr + LitJson：序列化/反序列化与存档
│   ├── Data/                # POCO 数据类：塔/怪物/武器/角色/地图/音乐配置
│   ├── GameScene/           # 关卡逻辑：LevelDataMgr、MonsterObject、TowerObject、
│   │                        #   MonsterPoint（波次生成）、TowerPoint（建造升级）
│   ├── PlayerController/    # PlayerObject（角色控制/射击）、MainTowerObject（主塔）
│   ├── BeginScene/          # 主菜单/选关/选角色/设置面板
│   └── Carmera/             # 第三人称相机平滑跟随
├── Resources/               # 运行时动态加载资源（角色/塔/怪物/特效/音效/UI）
├── Scenes/                  # 开始场景、战斗场景
└── StreamingAssets/         # JSON 配置表（默认数据）
```

---

## 运行说明

1. 使用 **Unity 2022.3 LTS** 打开项目；
2. 打开开始场景（BeginScene），点击运行即可从主菜单进入游戏；
3. 操作：WASD 移动、鼠标控制转向、鼠标左键攻击、数字键 1/2/3 建造炮塔、空格升级、Esc 返回。

> **说明**：仓库未包含 `Assets/ArtRes` 下约 3GB 的第三方 Asset Store 美术素材源包（植被、墓地场景套件等，涉及版权），场景中相关装饰引用会显示缺失，但**全部代码、玩法预制体、特效与音效资源（Assets/Resources）完整**，不影响阅读代码与理解架构。
