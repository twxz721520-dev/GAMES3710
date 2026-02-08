# 收集-使用系统使用手册

本文档面向关卡设计人员，介绍如何在场景中配置和使用收集-使用系统。

---

## 目录

1. [前置准备](#前置准备)
2. [钥匙-门系统](#钥匙-门系统)
3. [Sanity-药片系统](#sanity-药片系统)
4. [后处理效果](#后处理效果)
5. [集成测试清单](#集成测试清单)

---

## 前置准备

### 必需的场景组件

在开始配置关卡之前，确保场景中存在以下物体：

| 物体 | 组件 | 说明 |
|------|------|------|
| PlayerCapsule | `PlayerInventory` | 玩家道具管理 |
| PlayerCapsule | `SanityManager` | San值管理（关卡1、2需要） |
| PromptUI | `PromptUI` | 临时提示UI（底部） |
| InteractionPromptUI | `InteractionPromptUI` | 持续提示UI（交互提示） |
| SanityUI | `SanityUI` | San条和药片数量显示 |
| SanityPostProcess | `FullscreenEffect` + `SanityPostProcess` | 低San噪点效果 |

### UI层级结构参考

```
Canvas
├── PromptText          → 拖入 PromptUI.Prompt Text
├── InteractionText     → 拖入 InteractionPromptUI.Prompt Text
├── SanitySlider        → 拖入 SanityUI.Sanity Slider
└── PillCountText       → 拖入 SanityUI.Pill Count Text
```

---

## 钥匙-门系统

### 创建可拾取钥匙

1. 创建物体（Cube/自定义模型）
2. 添加 **Collider** 组件，勾选 **Is Trigger**
3. 添加 **ItemPickup** 组件
4. 配置参数：

| 参数 | 说明 |
|------|------|
| Item Type | 选择道具类型（枚举） |
| Display Name | 拾取时显示的名称（如 "Basement Key"） |

### 创建门/机关

1. 创建物体（门模型/机关模型）
2. 添加 **Collider** 组件，勾选 **Is Trigger**
3. 添加 **LockedDoor** 组件
4. 配置参数：

| 参数 | 说明 |
|------|------|
| Required Item | 所需道具类型，None 表示不需要道具 |
| Required Item Name | 缺少道具时显示的名称（如 "Basement Key"） |
| Consume Item | 是否消耗道具（勾选=用后消失） |
| Required Mechanisms | 前置依赖的机关列表 |
| Is Open | 当前状态（运行时会自动更新） |

### 设置机关依赖

**场景示例**：玩家必须先开启电闸，才能操作水泵

1. 创建电闸物体，添加 `LockedDoor`，Required Item = None
2. 创建水泵物体，添加 `LockedDoor`
3. 将电闸物体拖入水泵的 **Required Mechanisms** 数组

**运行时行为**：
- 电闸未开时，交互水泵显示 "Requires another mechanism first"
- 电闸开启后，水泵可正常交互

### 道具类型说明

当前可用的道具类型（在 `ItemType.cs` 中定义）：

| 枚举值 | 建议用途 |
|--------|----------|
| None | 不需要道具 |
| KeyBasement | 地下室钥匙 |
| KeyLibrary | 图书馆钥匙 |
| KeyStorage | 储藏室钥匙 |
| Fuse | 保险丝 |
| Valve | 阀门 |

**添加新道具类型**：编辑 `Assets/Scripts/Core/ItemType.cs`，在枚举中添加新值。

---

## Sanity-药片系统

### SanityManager 参数配置

在 PlayerCapsule 的 `SanityManager` 组件中配置：

| 参数 | 说明 | 建议值 |
|------|------|--------|
| Max Sanity | San值上限 | 100 |
| Current Sanity | 初始San值 | 100 |
| Decay Rate | 每秒衰减量 | 1-3 |
| Pill Restore Amount | 每颗药片恢复量 | 20-30 |
| Low Sanity Threshold | 触发后处理的阈值 | 30 |

### 创建可拾取药片

1. 创建物体（Capsule/自定义模型）
2. 添加 **Collider** 组件，勾选 **Is Trigger**
3. 添加 **PillPickup** 组件
4. 配置参数：

| 参数 | 说明 |
|------|------|
| Display Name | 拾取时显示的名称（如 "Pill"） |

### 玩家操作

| 按键 | 功能 |
|------|------|
| E | 拾取药片 |
| Q | 服用药片（恢复San值） |

---

## 后处理效果

### SanityPostProcess 参数配置

| 参数 | 说明 | 建议值 |
|------|------|--------|
| Noise Effect | 引用 FullscreenEffect 组件 | 拖入同物体的组件 |
| Noise Material | 引用 NoiseEffectMaterial | 拖入材质 |
| Min Noise Intensity | 刚进入低San时的噪点强度 | 0 |
| Max Noise Intensity | San归零时的噪点强度 | 0.3-0.5 |

### NoiseEffectMaterial 参数

| 参数 | 说明 | 建议值 |
|------|------|--------|
| Noise Intensity | 噪点强度（由脚本控制） | - |
| Noise Frame Rate | 噪点更新帧率 | 30 |

### FullscreenEffect 参数

| 参数 | 说明 | 建议值 |
|------|------|--------|
| Material | NoiseEffectMaterial | 拖入材质 |
| Injection Point | 注入时机 | AfterRenderingPostProcessing |
| Input Requirements | 输入需求 | Color |

---

## 关卡适用性

| 系统 | 关卡1 | 关卡2 | 关卡3 |
|------|:-----:|:-----:|:-----:|
| 钥匙-门 | ✓ | ✓ | ✗ |
| Sanity-药片 | ✓ | ✓ | ✗ |
| 后处理噪点 | ✓ | ✓ | ✗ |

**关卡3**：禁用 SanityManager、SanityUI、SanityPostProcess 组件。

---

## 集成测试清单

### 场景准备

1. 放置2-3个钥匙（使用不同 ItemType）
2. 放置对应的门（配置依赖关系）
3. 放置3-4个药片
4. 临时将 San Decay Rate 调高（如 10）加快测试

### 钥匙-门系统测试

- [ ] 靠近钥匙，显示 "Press E to pick up"
- [ ] 按E拾取，底部显示 "Obtained [名称]"
- [ ] 靠近门，显示 "Press E to interact"
- [ ] 无钥匙时按E，底部显示 "Requires [名称]"
- [ ] 有钥匙时按E，门消失
- [ ] 钥匙从背包中移除（再次拾取同类型钥匙可验证）

### 机关依赖测试

- [ ] 创建门A（无需道具）和门B（依赖门A）
- [ ] 门A未开时，交互门B显示 "Requires another mechanism first"
- [ ] 门A开启后，门B可正常交互

### Sanity-药片系统测试

- [ ] 运行游戏，观察San条随时间下降
- [ ] 靠近药片按E拾取，药片数量+1
- [ ] 按Q服用，San值恢复，药片数量-1
- [ ] 无药片时按Q，无反应
- [ ] San高于阈值时，无噪点效果
- [ ] San低于阈值时，噪点效果出现
- [ ] San越低，噪点越强

### UI提示测试

- [ ] 持续提示（交互提示）在离开范围后消失
- [ ] 临时提示（拾取反馈）几秒后自动消失
- [ ] 两种提示不会互相覆盖

---

## 常见问题

### Q: 拾取物品没有反应
- 检查物体是否有 Collider 且勾选 Is Trigger
- 检查玩家是否有 "Player" Tag
- 检查场景中是否有 PlayerInventory 和 PromptUI 单例

### Q: 门无法打开
- 检查 Required Item 是否与钥匙的 Item Type 匹配
- 检查 Required Mechanisms 中的前置机关是否已开启

### Q: 噪点效果不显示
- 检查 SanityPostProcess 引用是否完整
- 检查 FullscreenEffect 的 Input Requirements 是否为 Color
- 检查 San 值是否低于 Low Sanity Threshold

### Q: 持续提示不显示
- 检查场景中是否有 InteractionPromptUI 单例
- 检查 Prompt Text 引用是否正确
