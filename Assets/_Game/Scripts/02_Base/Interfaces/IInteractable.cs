// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/02_Base/Interfaces/IInteractable.cs
// 可交互物体接口。场景中所有可被玩家交互的物体实现此接口。
// ══════════════════════════════════════════════════════════════════════
using UnityEngine;

/// <summary>
/// 可交互物体接口。
///
/// 使用方式：
///   · 场景物体（拾取物、门、箱子、工作台等）挂载实现此接口的组件
///   · InteractionSystem 检测范围内的 IInteractable 并驱动交互流程
///   · 🏗️ 定义在 02_Base 层，04_Gameplay 实现具体交互逻辑
/// </summary>
public interface IInteractable
{
    /// <summary>交互类型（驱动不同的 UI 提示和交互动画）</summary>
    InteractionType InteractionType { get; }

    /// <summary>交互提示文本（显示在 UI 上，如"拾取"、"打开"）</summary>
    string InteractionPrompt { get; }

    /// <summary>当前是否可以被交互</summary>
    bool CanInteract(GameObject interactor);

    /// <summary>执行交互</summary>
    void Interact(GameObject interactor);

    /// <summary>交互物体的 Transform（用于距离检测和 UI 定位）</summary>
    Transform Transform { get; }
}
