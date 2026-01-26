# Unity 敌人AI系统完整教程

本教程将指导你从零开始实现一个完整的敌人AI系统，包含**巡逻**、**追逐**和**搜索**三种状态。

## 目录

1. [概念介绍](#概念介绍)
2. [前置准备](#前置准备)
3. [创建巡逻路径系统](#创建巡逻路径系统)
4. [创建敌人AI控制器](#创建敌人ai控制器)
5. [设置Layer和物理检测](#设置layer和物理检测)
6. [场景搭建与测试](#场景搭建与测试)
7. [调参指南](#调参指南)

---

## 概念介绍

### 状态机

敌人AI采用**有限状态机（FSM）**模式，敌人在任意时刻只处于以下三种状态之一：

```
┌─────────┐    看到玩家    ┌─────────┐
│  Patrol │ ────────────> │  Chase  │
│  巡逻   │               │  追逐   │
└─────────┘               └─────────┘
     ^                         │
     │                         │ 丢失视野
     │    超时                 v
     │ <──────────────── ┌─────────┐
     └────────────────── │ Search  │
                         │  搜索   │
                         └─────────┘
```

- **Patrol（巡逻）**：沿预设路径移动，到达终点后随机选择下一条路径
- **Chase（追逐）**：发现玩家后，实时追踪玩家位置
- **Search（搜索）**：丢失玩家视野后，在玩家最后出现位置附近随机搜索

### NavMesh 寻路

Unity的NavMesh系统用于AI寻路。它需要：
1. 烘焙（Bake）可行走区域
2. 敌人使用NavMeshAgent组件进行移动

### 视线检测

敌人通过以下方式检测玩家：
1. **距离检测**：玩家是否在视野范围内
2. **角度检测**：玩家是否在视野锥形角度内
3. **遮挡检测**：敌人与玩家之间是否有障碍物

---

## 前置准备

### 1. 确保已安装 AI Navigation 包

打开 **Window > Package Manager**，搜索并安装 `AI Navigation`。

### 2. 项目结构

在 `Assets` 下创建以下文件夹结构：

```
Assets/
├── Scripts/
│   └── Enemy/
│       ├── PatrolPath.cs
│       └── EnemyAI.cs
└── Prefabs/
```

---

## 创建巡逻路径系统

### 设计思路

巡逻路径由多个**路径片段（PatrolPath）**组成，每个片段包含多个路径点。敌人走完一个片段后，随机选择下一个片段继续巡逻。

```
路径片段A ──┬──> 路径片段B
            │
            └──> 路径片段C
```

### PatrolPath.cs 代码

在 `Assets/Scripts/Enemy/` 下创建 `PatrolPath.cs`：

```csharp
using UnityEngine;
using System.Collections.Generic;

public class PatrolPath : MonoBehaviour
{
    [Tooltip("Available path segments to choose from when reaching the end")]
    public List<PatrolPath> nextPaths;

    private Vector3[] _waypoints;

    public Vector3[] Waypoints
    {
        get
        {
            if (_waypoints == null || _waypoints.Length != transform.childCount)
            {
                CacheWaypoints();
            }
            return _waypoints;
        }
    }

    public int WaypointCount => Waypoints.Length;

    private void CacheWaypoints()
    {
        _waypoints = new Vector3[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            _waypoints[i] = transform.GetChild(i).position;
        }
    }

    public Vector3 GetWaypoint(int index)
    {
        return Waypoints[Mathf.Clamp(index, 0, WaypointCount - 1)];
    }

    public PatrolPath GetRandomNextPath()
    {
        if (nextPaths == null || nextPaths.Count == 0)
            return this;
        return nextPaths[Random.Range(0, nextPaths.Count)];
    }

    private void OnDrawGizmos()
    {
        if (transform.childCount < 2) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < transform.childCount; i++)
        {
            Vector3 current = transform.GetChild(i).position;
            Gizmos.DrawSphere(current, 0.3f);

            if (i < transform.childCount - 1)
            {
                Vector3 next = transform.GetChild(i + 1).position;
                Gizmos.DrawLine(current, next);
            }
        }

        Gizmos.color = Color.yellow;
        if (nextPaths != null)
        {
            Vector3 lastPoint = transform.GetChild(transform.childCount - 1).position;
            foreach (var path in nextPaths)
            {
                if (path != null && path.transform.childCount > 0)
                {
                    Vector3 nextStart = path.transform.GetChild(0).position;
                    Gizmos.DrawLine(lastPoint, nextStart);
                }
            }
        }
    }
}
```

### 代码解析

| 部分 | 说明 |
|------|------|
| `nextPaths` | 到达终点后可选的下一个路径片段列表 |
| `Waypoints` | 自动读取所有子物体位置作为路径点 |
| `GetRandomNextPath()` | 随机返回一个可选的下一路径 |
| `OnDrawGizmos()` | 在Scene视图绘制路径可视化 |

---

## 创建敌人AI控制器

### EnemyAI.cs 代码

在 `Assets/Scripts/Enemy/` 下创建 `EnemyAI.cs`：

```csharp
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Patrol,
    Chase,
    Search
}

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Patrol Settings")]
    public PatrolPath initialPath;
    public float waypointReachThreshold = 0.5f;

    [Header("Movement Speed")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;
    public float searchSpeed = 3f;

    [Header("Vision Settings")]
    public float viewDistance = 15f;
    public float viewAngle = 90f;
    public LayerMask obstacleMask;
    public LayerMask playerMask;

    [Header("Search Settings")]
    public float searchDuration = 5f;
    public float searchRadius = 8f;

    private NavMeshAgent _agent;
    private EnemyState _currentState = EnemyState.Patrol;

    private PatrolPath _currentPath;
    private int _currentWaypointIndex;

    private Transform _player;
    private Vector3 _lastKnownPlayerPosition;
    private float _searchTimer;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if (initialPath != null)
        {
            SetPath(initialPath);
        }
        SetState(EnemyState.Patrol);
    }

    private void Update()
    {
        CheckVision();

        switch (_currentState)
        {
            case EnemyState.Patrol:
                UpdatePatrol();
                break;
            case EnemyState.Chase:
                UpdateChase();
                break;
            case EnemyState.Search:
                UpdateSearch();
                break;
        }
    }

    private void CheckVision()
    {
        Collider[] targetsInRange = Physics.OverlapSphere(transform.position, viewDistance, playerMask);

        foreach (var target in targetsInRange)
        {
            Transform targetTransform = target.transform;
            Vector3 directionToTarget = (targetTransform.position - transform.position).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

            if (angleToTarget < viewAngle / 2f)
            {
                float distanceToTarget = Vector3.Distance(transform.position, targetTransform.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask))
                {
                    _player = targetTransform;
                    _lastKnownPlayerPosition = _player.position;

                    if (_currentState != EnemyState.Chase)
                    {
                        SetState(EnemyState.Chase);
                    }
                    return;
                }
            }
        }

        if (_currentState == EnemyState.Chase)
        {
            SetState(EnemyState.Search);
        }
    }

    private bool CanSeePlayer()
    {
        if (_player == null) return false;

        Vector3 directionToPlayer = (_player.position - transform.position).normalized;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        if (angleToPlayer > viewAngle / 2f) return false;

        float distanceToPlayer = Vector3.Distance(transform.position, _player.position);
        if (distanceToPlayer > viewDistance) return false;

        if (Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleMask))
            return false;

        return true;
    }

    private void SetState(EnemyState newState)
    {
        _currentState = newState;
        switch (newState)
        {
            case EnemyState.Patrol:
                _agent.speed = patrolSpeed;
                if (_currentPath != null && _currentPath.WaypointCount > 0)
                {
                    _agent.SetDestination(_currentPath.GetWaypoint(_currentWaypointIndex));
                }
                break;
            case EnemyState.Chase:
                _agent.speed = chaseSpeed;
                break;
            case EnemyState.Search:
                _agent.speed = searchSpeed;
                _searchTimer = searchDuration;
                SetRandomSearchDestination();
                break;
        }
    }

    private void UpdateChase()
    {
        if (_player != null)
        {
            _lastKnownPlayerPosition = _player.position;
            _agent.SetDestination(_player.position);
        }
    }

    private void UpdateSearch()
    {
        _searchTimer -= Time.deltaTime;

        if (_searchTimer <= 0f)
        {
            SetState(EnemyState.Patrol);
            return;
        }

        if (!_agent.pathPending && _agent.remainingDistance <= waypointReachThreshold)
        {
            SetRandomSearchDestination();
        }
    }

    private void SetRandomSearchDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * searchRadius;
        randomDirection += _lastKnownPlayerPosition;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, searchRadius, NavMesh.AllAreas))
        {
            _agent.SetDestination(hit.position);
        }
    }

    private void SetPath(PatrolPath path)
    {
        _currentPath = path;
        _currentWaypointIndex = 0;
        if (_currentPath.WaypointCount > 0)
        {
            _agent.SetDestination(_currentPath.GetWaypoint(0));
        }
    }

    private void UpdatePatrol()
    {
        if (_currentPath == null || _currentPath.WaypointCount == 0)
            return;

        if (!_agent.isOnNavMesh)
            return;

        if (!_agent.pathPending && _agent.remainingDistance <= waypointReachThreshold)
        {
            _currentWaypointIndex++;

            if (_currentWaypointIndex >= _currentPath.WaypointCount)
            {
                PatrolPath nextPath = _currentPath.GetRandomNextPath();
                SetPath(nextPath);
            }
            else
            {
                _agent.SetDestination(_currentPath.GetWaypoint(_currentWaypointIndex));
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward * viewDistance;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward * viewDistance;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);

        if (_player != null && _currentState == EnemyState.Chase)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, _player.position);
        }

        if (_currentState == EnemyState.Search)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(_lastKnownPlayerPosition, searchRadius);
        }
    }
}
```

### 代码解析

#### 视线检测 (CheckVision)

```csharp
private void CheckVision()
{
    // 1. 获取视野范围内所有玩家Layer的物体
    Collider[] targetsInRange = Physics.OverlapSphere(transform.position, viewDistance, playerMask);

    foreach (var target in targetsInRange)
    {
        // 2. 计算到目标的方向和角度
        Vector3 directionToTarget = (targetTransform.position - transform.position).normalized;
        float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);

        // 3. 检查是否在视野角度内
        if (angleToTarget < viewAngle / 2f)
        {
            // 4. 射线检测是否有障碍物遮挡
            if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstacleMask))
            {
                // 发现玩家，进入追逐状态
                SetState(EnemyState.Chase);
            }
        }
    }
}
```

#### 状态切换 (SetState)

```csharp
private void SetState(EnemyState newState)
{
    _currentState = newState;
    switch (newState)
    {
        case EnemyState.Patrol:
            _agent.speed = patrolSpeed;           // 设置巡逻速度
            break;
        case EnemyState.Chase:
            _agent.speed = chaseSpeed;            // 设置追逐速度
            break;
        case EnemyState.Search:
            _agent.speed = searchSpeed;           // 设置搜索速度
            _searchTimer = searchDuration;        // 重置搜索计时器
            SetRandomSearchDestination();         // 设置随机搜索点
            break;
    }
}
```

---

## 设置Layer和物理检测

### 1. 创建Layer

1. 打开任意GameObject的Inspector
2. 点击Layer下拉菜单 > **Add Layer...**
3. 添加以下Layer：

| Layer编号 | 名称 | 用途 |
|-----------|------|------|
| 6 | Player | 玩家碰撞体 |
| 7 | Obstacle | 墙壁、障碍物 |

### 2. 分配Layer

| 物体 | Layer |
|------|-------|
| 玩家的Capsule（带Collider的） | Player |
| 所有墙壁、障碍物 | Obstacle |

### 3. 配置EnemyAI组件

在敌人的EnemyAI组件上：
- **Player Mask**：勾选 `Player`
- **Obstacle Mask**：勾选 `Obstacle`

---

## 场景搭建与测试

### 1. 烘焙NavMesh

1. 选择场景中的地面物体
2. **Add Component > NavMesh Surface**
3. 点击 **Bake** 按钮

地面上会出现蓝色网格，表示可行走区域。

### 2. 创建巡逻路径

1. **创建空物体**，命名为 `PatrolPath_A`
2. 添加 **PatrolPath** 组件
3. 在该物体下创建多个**空子物体**作为路径点，调整位置
4. 重复以上步骤创建更多路径片段
5. 在每个PatrolPath的 **Next Paths** 中添加可连接的其他路径

### 3. 创建敌人

1. **创建Capsule**，命名为 `Enemy`
2. 创建一个**小球**作为子物体，放在本地坐标 `(0, 0, 0.5)` 标识正面方向
3. 添加组件：
   - **NavMeshAgent**
   - **EnemyAI**
4. 配置EnemyAI：
   - **Initial Path**：拖入第一个PatrolPath
   - **Player Mask**：勾选 Player
   - **Obstacle Mask**：勾选 Obstacle
5. 保存为Prefab

### 4. 运行测试

1. 点击Play
2. 敌人应该开始沿路径巡逻
3. 进入敌人视野范围，敌人会追逐你
4. 躲到障碍物后面，敌人会进入搜索模式
5. 搜索超时后，敌人回归巡逻

---

## 调参指南

### 移动速度

| 参数 | 建议值 | 说明 |
|------|--------|------|
| Patrol Speed | 2-3 | 巡逻时慢速移动，给玩家反应时间 |
| Chase Speed | 4-6 | 追逐时应比玩家稍快，制造压迫感 |
| Search Speed | 3-4 | 搜索时中等速度 |

### 视野参数

| 参数 | 建议值 | 说明 |
|------|--------|------|
| View Distance | 10-20 | 视野距离，太远会太难躲避 |
| View Angle | 60-120 | 视野角度，90度是常见选择 |

### 搜索参数

| 参数 | 建议值 | 说明 |
|------|--------|------|
| Search Duration | 5-10 | 搜索持续时间，太短玩家容易脱身 |
| Search Radius | 5-10 | 搜索范围，应覆盖玩家可能躲藏的区域 |

---

## 常见问题

### Q: 敌人不移动

检查：
1. 是否烘焙了NavMesh
2. 敌人是否放在NavMesh上
3. PatrolPath是否有子物体作为路径点
4. InitialPath是否已分配

### Q: 敌人看不到玩家

检查：
1. 玩家是否分配了Player Layer
2. EnemyAI的Player Mask是否勾选了Player
3. View Distance是否足够大

### Q: 敌人能穿墙看到玩家

检查：
1. 墙壁是否分配了Obstacle Layer
2. EnemyAI的Obstacle Mask是否勾选了Obstacle
3. 墙壁是否有Collider

### Q: 报错 "GetRemainingDistance can only be called on an active agent"

敌人不在NavMesh上。确保：
1. 场景已烘焙NavMesh
2. 敌人初始位置在NavMesh区域内

---

## 下一步扩展

本教程实现了基础的敌人AI。后续可以扩展：

- **躲藏点系统**：玩家可以进入的隐藏区域
- **声音检测**：敌人可以听到玩家的脚步声
- **多敌人协作**：敌人之间可以通信
- **不同敌人类型**：不同的视野范围、速度、行为模式
