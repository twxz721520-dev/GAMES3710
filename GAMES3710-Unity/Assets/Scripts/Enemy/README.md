# 敌人AI设置指南

## 前置条件
- Unity 6.3 + AI Navigation 包

## 设置步骤

### 1. 烘焙 NavMesh
1. 选择可行走的地面物体
2. 添加组件 > **NavMesh Surface**
3. 点击 **Bake**

### 2. 创建巡逻路径
1. 创建空物体，添加 **PatrolPath** 组件
2. 添加子物体作为路径点（位置有效，名称无所谓）
3. 可选：在 `Next Paths` 列表中指定其他 PatrolPath，形成随机路线网络

### 3. 创建敌人
1. 创建 Capsule（+ 小球放在本地坐标 (0, 0, 0.5) 标识正面）
2. 添加 **NavMeshAgent** 组件
3. 添加 **EnemyAI** 组件
4. 将 PatrolPath 指定给 `Initial Path`
5. 保存为 Prefab

## Inspector 参数说明

### PatrolPath
| 参数 | 说明 |
|------|------|
| Next Paths | 到达终点后可选的下一个路径片段 |

### EnemyAI
| 参数 | 说明 |
|------|------|
| Initial Path | 初始巡逻路径 |
| Waypoint Reach Threshold | 判定到达路径点的距离阈值 |
| Patrol Speed | 巡逻状态移动速度 |
| Chase Speed | 追逐状态移动速度 |
| Search Speed | 搜索状态移动速度 |
| View Distance | 视野距离 |
| View Angle | 视野角度（度） |
| Obstacle Mask | 障碍物Layer（阻挡视线） |
| Player Mask | 玩家Layer（检测目标） |
| Search Duration | 搜索持续时间（秒） |
| Search Radius | 搜索范围半径 |

## Layer 设置

1. 创建 `Player` Layer，分配给玩家
2. 创建 `Obstacle` Layer，分配给墙壁等遮挡物
3. 在 EnemyAI 组件上设置对应的 Layer Mask

---

## 躲藏点系统

### 设置步骤

1. **玩家设置**:
   - 给玩家的 PlayerCapsule 添加 `Player` Tag
   - 在玩家上添加 `PlayerHideState` 组件

2. **创建躲藏点**:
   - 创建 Cube 作为躲藏点
   - 添加 `HidingSpot` 组件
   - 添加 Collider 并勾选 `Is Trigger`
   - 调整 Collider 大小使其比躲藏点稍大（作为触发区域）

3. **创建提示UI**:
   - 创建 Canvas > Text (TMP)
   - 将 Text 物体拖入 HidingSpot 的 `Prompt UI` 字段

### HidingSpot 参数

| 参数 | 说明 |
|------|------|
| Prompt UI | 提示文字的 UI 物体 |
| Enter Prompt | 进入时显示的提示文字 |
| Exit Prompt | 躲藏中显示的提示文字 |

### 工作原理

- 玩家进入触发区域 → 显示提示 UI
- 按 E 键 → 切换躲藏状态
- 躲藏时敌人视线检测会忽略玩家
- 离开触发区域 → 自动退出躲藏状态
