# 任务系统使用手册

本文档面向关卡设计人员，介绍如何在场景中配置和使用任务（Objective）系统。

---

## 目录

1. [系统概述](#系统概述)
2. [前置准备](#前置准备)
3. [配置任务链](#配置任务链)
4. [配置触发器](#配置触发器)
5. [触发模式详解](#触发模式详解)
6. [高级用法](#高级用法)
7. [完整示例](#完整示例)
8. [常见问题](#常见问题)

---

## 系统概述

任务系统由三个脚本组成：

| 脚本 | 职责 |
|------|------|
| `ObjectiveManager` | 数据核心：存储任务链、管理推进逻辑 |
| `ObjectiveTrigger` | 桥接组件：挂在场景物体上，检测完成条件并通知 Manager |
| `ObjectiveUI` | 界面显示：右上角显示当前任务、子任务列表、长期目标 |

### 运行流程

```
玩家行为 → ObjectiveTrigger 检测到完成条件
         → 调用 ObjectiveManager.CompleteSubTask(id)
         → Manager 标记子任务完成，检查是否全部完成
         → 通知 ObjectiveUI 更新显示（划掉/切换任务）
```

---

## 前置准备

### 必需的场景组件

| 物体 | 组件 | 说明 |
|------|------|------|
| 任意 GameObject | `ObjectiveManager` | 全局唯一，管理任务数据 |
| Canvas 下的子物体 | `ObjectiveUI` | 任务显示面板 |

### UI 层级结构

```
Canvas
├── ...（现有 UI 元素）
└── ObjectiveUI        → ObjectiveUI component（运行时自动创建面板）
```

> **注意**：ObjectiveUI 面板默认位于右上角。如果与 SanityUI / PillCount 重叠，调整 Inspector 中的 **Top Offset** 参数。

---

## 配置任务链

在 `ObjectiveManager` 的 Inspector 中配置：

### 1. 长期目标（Long-term Goal）

显示在任务面板最上方的灰色斜体文字，用于告诉玩家当前的总体目标。

示例：`为了打开电控门，我必须恢复电力`

可以在运行时被 ObjectiveTrigger 动态修改（见 [高级用法](#高级用法)）。

### 2. 任务链（Objective Chain）

按顺序排列的主任务数组。游戏从第 0 个开始，当前任务的所有子任务完成后自动推进到下一个。

每个 Objective 包含：

| 字段 | 说明 |
|------|------|
| Description | 主任务描述，显示在面板中（如 "收集启动发动机所需的零件"） |
| Sub Tasks | 子任务列表（可为空） |

每个 SubTask 包含：

| 字段 | 说明 |
|------|------|
| Id | 唯一标识符，ObjectiveTrigger 通过此 ID 标记完成（如 `fuse`） |
| Description | 显示文字（如 "收集保险丝"） |

### 配置示例

```
ObjectiveManager
├── Long Term Goal: "恢复电力以打开电控门"
├── Objectives[0]:
│   ├── Description: "收集启动发动机所需的零件"
│   └── Sub Tasks:
│       ├── {id: "fuse",   description: "收集保险丝"}
│       ├── {id: "fuel",   description: "收集燃油"}
│       └── {id: "handle", description: "收集启动手柄"}
├── Objectives[1]:
│   ├── Description: "启动发电机"
│   └── Sub Tasks: (空)       ← 无子任务，需手动完成
└── Objectives[2]:
    ├── Description: "打开电控门"
    └── Sub Tasks: (空)
```

---

## 配置触发器

在需要触发任务完成的**场景物体**上添加 `ObjectiveTrigger` 组件。

### Inspector 参数

| 参数 | 说明 |
|------|------|
| Sub Task Id | 对应 ObjectiveManager 中子任务的 `id` |
| Mode | 触发模式（见下方详解） |
| Complete Objective | 勾选后直接完成整个当前主任务（用于无子任务的任务） |
| New Long Term Goal | 触发时更新长期目标文字（留空则不修改） |
| Keys | 仅 OnKeyPress 模式，配置监听的按键数组 |

---

## 触发模式详解

### OnPickup

适用于 `ItemPickup` 和 `PillPickup` 物体。物品被拾取后自动触发。

**配置方式**：在已有 ItemPickup/PillPickup 的物体上额外挂载 ObjectiveTrigger。

```
保险丝物体
├── ItemPickup (displayName: "Fuse")
└── ObjectiveTrigger (subTaskId: "fuse", mode: OnPickup)
```

> 兼容 ItemPickup（SetActive false）和 PillPickup（Destroy）两种销毁方式。

### OnDoorActivated

适用于 `LockedDoor` 物体。门/机关被成功激活后自动触发。

```
发电机开关
├── LockedDoor (...)
└── ObjectiveTrigger (subTaskId: "generator", mode: OnDoorActivated)
```

### OnPlayerEnter

玩家进入 Trigger 区域时触发。适用于区域到达类任务。

**配置方式**：
1. 创建空物体
2. 添加 Collider（如 Box Collider），勾选 **Is Trigger**
3. 添加 ObjectiveTrigger，模式选 OnPlayerEnter

```
到达出口触发区
├── BoxCollider (isTrigger: true)
└── ObjectiveTrigger (subTaskId: "reach_exit", mode: OnPlayerEnter)
```

### OnKeyPress

玩家按下指定按键时触发。适用于新手引导。

**配置方式**：在 Keys 数组中添加要监听的按键，任意一个按下即触发。

```
移动教学触发器
└── ObjectiveTrigger
    ├── subTaskId: "tutorial_move"
    ├── mode: OnKeyPress
    └── keys: [W, A, S, D]    ← 任意一个触发
```

> 此模式挂在任意活跃的 GameObject 上即可，不需要 Collider。

### Manual

由外部代码手动调用 `ObjectiveTrigger.Complete()` 触发。适用于：

- **Animation Event**：在动画关键帧上调用
- **Timeline Signal**：通过 Signal Receiver 回调
- **自定义脚本**：获取引用后调用

```csharp
// 从其他脚本触发
GetComponent<ObjectiveTrigger>().Complete();
```

---

## 高级用法

### 动态修改长期目标

在 ObjectiveTrigger 的 **New Long Term Goal** 字段填入新文字，触发时会自动更新：

```
电控门触发区
├── BoxCollider (isTrigger: true)
└── ObjectiveTrigger
    ├── mode: OnPlayerEnter
    ├── completeObjective: true
    └── newLongTermGoal: "找到出口逃离建筑"   ← 进入区域时更新
```

也可在代码中调用：
```csharp
ObjectiveManager.Instance.SetLongTermGoal("新的长期目标");
```

### 无子任务的主任务

当 Objective 的 Sub Tasks 为空时，不会自动完成。需要通过 ObjectiveTrigger 勾选 **Complete Objective** 来手动推进。

### 提前完成后续任务的子任务

系统支持在当前任务未完成时，预先完成后续任务的子任务。例如玩家在任务 0 期间就拾取了任务 2 需要的物品：

- 子任务数据会被标记为 `completed`
- 推进到该任务时，UI 直接显示已划掉的状态
- 如果所有子任务都被预完成，该任务会自动跳过

### 从代码直接完成子任务

不需要 ObjectiveTrigger，任何脚本都可以直接调用：

```csharp
ObjectiveManager.Instance.CompleteSubTask("fuse");
ObjectiveManager.Instance.CompleteCurrentObjective();
```

### UI 样式调整

在 ObjectiveUI 的 Inspector 中可调整：

| 参数 | 说明 | 默认值 |
|------|------|--------|
| Panel Width | 面板宽度（1080p 基准） | 320 |
| Panel Padding | 内边距 | 12 |
| Top Offset | 距顶部偏移，用于避开其他 UI | 80 |
| Goal Font Size | 长期目标字号 | 14 |
| Objective Font Size | 主任务字号 | 20 |
| Sub Task Font Size | 子任务字号 | 16 |
| Bg Color | 背景颜色 | 半透明黑 |

> 所有尺寸值以 1080p 为基准，运行时根据 Canvas 实际高度自动缩放。

---

## 完整示例

### 场景：收集零件 → 启动发电机 → 开门

**1. 创建 ObjectiveManager**

```
空物体 "ObjectiveManager"
└── ObjectiveManager 组件
    ├── Long Term Goal: "恢复电力以打开电控门"
    ├── Objectives[0]: "收集启动发动机所需的零件"
    │   ├── SubTask: {id: "fuse",   desc: "收集保险丝"}
    │   ├── SubTask: {id: "fuel",   desc: "收集燃油"}
    │   └── SubTask: {id: "handle", desc: "收集启动手柄"}
    ├── Objectives[1]: "启动发电机"
    │   └── (无子任务)
    └── Objectives[2]: "打开电控门"
        └── (无子任务)
```

**2. 配置物品触发器**

```
保险丝物体:  ItemPickup + ObjectiveTrigger(id:"fuse",   mode:OnPickup)
燃油物体:    ItemPickup + ObjectiveTrigger(id:"fuel",   mode:OnPickup)
启动手柄:    ItemPickup + ObjectiveTrigger(id:"handle", mode:OnPickup)
```

**3. 配置发电机交互**

```
发电机开关:  LockedDoor + ObjectiveTrigger(mode:OnDoorActivated, completeObjective:✓)
```
> 玩家交互后，LockedDoor 标记 isOpen，ObjectiveTrigger 完成整个 Objectives[1]。

**4. 配置电控门**

```
电控门:  LockedDoor + ObjectiveTrigger(mode:OnDoorActivated, completeObjective:✓, newLongTermGoal:"找到出口")
```

**5. 添加 ObjectiveUI**

在 Canvas 下创建空物体，挂载 `ObjectiveUI` 组件，调整 Top Offset 避开 SanityUI。

---

## 常见问题

### Q: 子任务完成了但 UI 没有划掉

检查 ObjectiveTrigger 的 `Sub Task Id` 是否与 ObjectiveManager 中的 `id` 完全一致（区分大小写）。

### Q: 任务没有自动推进到下一个

- 如果有子任务：确认所有子任务都已配置对应的 ObjectiveTrigger
- 如果无子任务：需要某个 ObjectiveTrigger 勾选 `Complete Objective`

### Q: 面板和其他 UI 重叠

调整 ObjectiveUI 的 `Top Offset` 值（默认 80），增大数值使面板下移。

### Q: 面板太小 / 太大

所有尺寸以 1080p 为基准自动缩放。如果仍不合适，调整 `Panel Width` 和各 Font Size 参数。
