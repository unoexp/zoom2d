# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 项目概述

Unity 2D项目《根与废土》(Roots & Ruin) — 2D横版生存建造探索游戏（Unity 2022.3.62f3）。采用五层菱形分层架构，通过数字前缀目录强制依赖流向。

## 开发环境

- **Unity版本**：2022.3.62f3
- **UI框架**：UGUI（`com.unity.ugui`），TextMeshPro
- **无 `.asmdef`**：所有脚本在默认程序集 `Assembly-CSharp`
- **无 Addressables**：资源加载使用 `Resources.Load` + 自定义 `ResourceManager`（支持 AssetBundle）
- **测试**：`com.unity.test-framework 1.1.33`，通过 Unity Test Runner 执行（目前无测试文件）
- **包管理**：`Packages/manifest.json`，仅 Unity Registry 包 + `com.coplaydev.unity-mcp`（AI工具链）
- **无 CI/CD 配置**

## 架构与代码结构

### 五层菱形分层架构

`Assets/_Game/Scripts/` 下各层：

| 层 | 职责 | 当前状态 |
|----|------|----------|
| `01_Data/` | 纯数据定义（ScriptableObjects、存档结构体） | 已实现 |
| `02_Base/` | 引擎无关核心机制（EventBus、ServiceLocator、StateMachine、Timer、ResourceManager） | 已实现 |
| `03_Core/` | 生存游戏业务规则（背包、生存属性） | 已实现 |
| `04_Gameplay/` | 运行时游戏行为（角色FSM、AI、战斗） | **空目录，未实现** |
| `05_Show/` | 表现层（UI、动画、特效） | 背包UI已实现 |
| `06_Extensions/` | MOD系统和编辑器工具 | 仅有 `ItemAssetValidator`，MOD目录为空 |
| `07_Shared/` | 枚举常量和扩展方法 | 已实现 |

**依赖流向规则**：
- 低编号层可依赖高编号层，反之禁止
- **业务层→表现层**：严禁直接调用，必须通过 `EventBus.Publish`
- **表现层→业务层**：通过 `ServiceLocator.Get<T>()` 或发布 UI事件
- **跨业务层通信**：通过 EventBus

### 命名空间约定

| 层级 | 命名空间 |
|------|---------|
| 01_Data | `SurvivalGame.Data.Inventory`、`SurvivalGame.Data.Inventory.Expansion` |
| 03_Core | `SurvivalGame.Core.Inventory`、`SurvivalGame.Core.Inventory.Expansion` |
| 05_Show | 通常无命名空间（全局），或 `SurvivalGame.Show.Inventory` |
| 07_Shared | 无命名空间（全局枚举） |

### 核心基础设施系统

#### EventBus（`02_Base/EventBus/IEvent.cs`）

```csharp
EventBus.Subscribe<MyEvent>(handler);   // 订阅
EventBus.Publish(new MyEvent { ... });  // 发布（struct，零GC）
EventBus.Unsubscribe<MyEvent>(handler); // OnDestroy 中必须调用
EventBus.Clear();                       // 场景切换时调用
```

**事件定义位置**：
- 业务事件：`02_Base/EventBus/Events/`（`InventoryEvents.cs`、`SurvivalEvents.cs` 等）
- UI交互事件：`05_Show/.../Events/`（仅表现层内部使用）

所有事件必须为 `struct` 并实现 `IEvent` 接口。

#### ServiceLocator（`02_Base/ServiceLocater/ServiceLocator.cs`）

```csharp
// 注册（Awake 中，同时注册具体类和接口）
ServiceLocator.Register<InventorySystem>(this);
ServiceLocator.Register<IInventorySystem>(this);

// 使用
var inv = ServiceLocator.Get<IInventorySystem>();

// 注销（OnDestroy 中必须调用）
ServiceLocator.Unregister<InventorySystem>();
ServiceLocator.Unregister<IInventorySystem>();
```

#### TimerSystem（`02_Base/Timer/TimerSystem.cs`）

继承 `MonoSingleton<TimerSystem>`，同时注册到 ServiceLocator。可通过两种方式访问：
```csharp
TimerSystem.Instance.Create(duration, callback);
ServiceLocator.Get<TimerSystem>().Create(duration, callback);
```
零GC，通过 `TimerHandle` 安全取消。优先使用此系统而非 Unity 的 `Invoke`。

#### ResourceManager（`02_Base/ResourceManager/ResourceManager.cs`）

继承 `MonoSingleton<ResourceManager>`，支持同步/异步加载、AssetBundle、LRU缓存、3次重试。

#### StateMachine（`02_Base/StateMachine/IState.cs`）

泛型状态机 `StateMachine<TStateKey> where TStateKey : Enum`。

### 05_Show 层的 MVP 模式

背包UI为参考实现：

```
Presenter（MonoBehaviour）→ ViewModel（纯C#类）→ View（纯显示组件）
```

- **Presenter**：订阅 EventBus → 更新 ViewModel；处理用户交互 → 调用 ServiceLocator 获取业务系统。**不直接调用 View 方法**，UI反馈也通过 EventBus 发布 `UIFeedbackEvent`
- **ViewModel**：纯C#，持有UI状态，暴露 `event Action<T>` 给View订阅
- **View**：仅负责渲染，监听 ViewModel 的事件回调。也需订阅 EventBus 接收 UI反馈信号
- **Adapter**（可选）：数据格式转换层（`InventoryViewModelAdapter.cs`）

### 数据层关键类型

- **`ItemDefinitionSO`**（抽象基类）：子类有 `ArmorItemSO`、`MaterialItemSO`、`ToolItemSO`、`WeaponItemSO`、`ConsumableItemSO`
- **`InventoryContainer`**：`struct`（值类型），传递时注意拷贝语义
- **`InventorySlot`**、**`ItemStack`**：背包槽位和堆叠数据

### 全局枚举（`07_Shared/Constant/Enums.cs`）

**规则：所有全局枚举统一在此文件追加，不新建枚举文件。**

已定义：`SurvivalAttributeType`、`DeathCause`、`DamageType`、`ItemType`、`ItemQuality`、`GameState`、`PlayerState`、`EnemyState`、`DayPhase`、`WeatherType`、`EquipmentSlot`、`CraftingResult`、`InteractionType`、`AudioGroup`、`SlotType`

**注意**：`ItemDefinitionSO.cs` 中还定义了局部枚举 `ItemCategory` 和 `ItemRarity`，与 `07_Shared` 中的 `ItemType`/`ItemQuality` 是不同的枚举。

## 代码约定

- 文档注释和注释使用**中文**
- C#文件头部格式：`// 📁 路径/文件名.cs` + 中文说明
- 性能关键代码添加 `// [PERF]` 标注
- `MonoSingleton` 仅用于基础设施管理器，业务系统使用 ServiceLocator

## 实现注意事项

### 多类文件

以下文件包含多个类定义（文件名仅反映其中之一）：
- `IEvent.cs` → 包含 `IEvent` 接口 + `EventBus` 静态类
- `IState.cs` → 包含 `IState` 接口 + `StateMachine<TStateKey>` 泛型类

### 服务注册时序

- 核心系统在 `Awake()` 中注册到 ServiceLocator
- `InventorySystem.Start()` 中获取 `IItemDataService`（因为 `Start()` 在所有 `Awake()` 之后执行）
- `OnDestroy()` 中必须注销所有注册，防止悬空引用

### 订阅泄漏防护

`OnDestroy` 中务必调用 `EventBus.Unsubscribe`；场景卸载时调用 `EventBus.Clear()`。

## 数据驱动扩展点

| 扩展需求 | 操作方式 | 需修改的文件 |
|----------|----------|-------------|
| 新增物品类型 | 创建新的 `.asset` 文件 | 无需改代码 |
| 新增制作配方 | 创建 `RecipeDefinitionSO.asset` | 无需改代码 |
| 新增生存状态效果 | 实现 `IStatusEffect` 接口 | 仅新增1个类 |
| 新增枚举值 | 追加到 `07_Shared/Constant/Enums.cs` | 只改此一个文件 |

## 设计文档

`docs/` 目录包含核心设计文档（中文）：
- `docs/程序/Unity 底层程序架构设计方案 v1.0.md` — 完整架构规格
- `docs/程序/代码编写规范.md` + `代码编写规范_续.md` — 编码标准
- `docs/策划/完整游戏设计文档.md` — 完整游戏设计文档
- `05_Show/Inventory/ARCHITECTURE_OVERVIEW.md` — 背包UI的MVP架构详细说明

## 重要提醒

1. **`ThirdPart/` 目录**（claude-code-proxy）不是游戏代码，不要修改。
2. **`04_Gameplay/` 和 `06_Extensions/Mod/` 为空目录**，相关功能尚未实现。
3. **`SurvivalStatusSystem` 的死亡原因优先级**是硬编码的：脱水 > 饥饿 > 低温 > 高温 > 战斗。

## 多Agent调度器角色

本项目支持多Agent协同开发。作为调度器，首要职责是合理分配任务、协调Agent工作、确保架构一致性。

### 可用 Agent 映射

| 调度器名称 | Claude Code Agent类型 | 职责 |
|------------|---------------------|------|
| Product/UX Agent | `unity-ui-spec-writer` | 将模糊需求转化为详细UI/UX规格 |
| UI Architect Agent | `unity-ui-architect` | 设计五层架构中的表现层 |
| UI Implementation Agent | `unity-ui-implementer` | 基于规格实现Unity UI代码 |
| Data/ViewModel Agent | `ui-data-modeler` | 设计UI数据模型 |
| Backend Integration Agent | `backend-integration-agent` | Unity与后端服务集成 |
| Asset Integration Agent | `ui-art-integration-agent` | UI美术资源集成 |
| Animation/Effects Agent | `unity-animation-effects-advisor` | UI动画与交互反馈 |
| Performance Agent | `unity-ui-performance-agent` | UI性能分析与优化 |
| QA/Test Agent | `unity-frontend-qa-agent` | 测试用例设计 |
| DevOps/Orchestrator | `unity-dev-orchestrator` | 复杂任务分解与协调 |

### 调度工作流

1. 分析需求 → 2. 选择Agent → 3. 拆分子任务（标明串行/并行） → 4. 分配文件边界 → 5. 确定执行顺序 → 6. 识别风险与验收标准

### 架构约束

- 严格遵循五层依赖流向
- 业务层→表现层通过 EventBus，表现层→业务层通过 ServiceLocator
- 优先 ScriptableObjects 配置
- 避免GC分配，事件使用 struct
- 05_Show 遵循 Presenter → ViewModel → View 单向数据流
