# Crouch System Tutorial

本文档面向关卡设计人员，介绍如何配置和使用下蹲系统。

---

## Overview

按住 **Left Ctrl** 下蹲，松开后站起。头顶有障碍物时无法站起，适用于管道等低矮空间的关卡设计。

---

## Setup

### HeadObstacle Layer

下蹲系统使用专用层 `HeadObstacle`（Layer 8）检测头顶障碍物：

1. Edit → Project Settings → Tags and Layers，将 Layer 8 命名为 `HeadObstacle`
2. 将需要阻挡站立的物体（管道顶部、低矮天花板等）设为 `HeadObstacle` 层
3. Inspector 中 FirstPersonController 的 **Crouch Obstacle Layers** 勾选 `HeadObstacle`

> **Note**: 只有被标记为 `HeadObstacle` 层的物体才会阻止站起。地面、墙壁等不需要设置此层。

### Inspector Parameters

在 PlayerCapsule 的 `FirstPersonController` 组件中配置：

| Parameter | Description | Default |
|-----------|-------------|---------|
| Crouch Height | CharacterController height when crouching | 1.0 |
| Crouch Speed Multiplier | Speed multiplier when crouching (based on walk speed) | 0.5 |
| Crouch Transition Speed | Smoothing speed for crouch/stand transition | 10 |
| Crouch Obstacle Layers | Layers that block standing up | HeadObstacle |

---

## Level Design Guide

### Tunnel / Pipe Design

- Tunnel height should be slightly larger than `CrouchHeight` (e.g. 1.1~1.2 for CrouchHeight=1.0)
- Tunnel width should be slightly larger than `CharacterController Radius × 2` (e.g. 0.7+ for Radius=0.3)
- The **top** collider of the tunnel must be set to `HeadObstacle` layer

### Notes

- Releasing Ctrl under a `HeadObstacle` keeps the player crouched; they stand up automatically after leaving
- Sprinting (Shift) is **disabled** while crouching; speed stays at `MoveSpeed × CrouchSpeedMultiplier`
- Camera height scales proportionally based on the standing ratio, no extra configuration needed
