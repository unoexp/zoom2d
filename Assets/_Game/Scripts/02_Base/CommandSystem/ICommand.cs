// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/02_Base/CommandSystem/ICommand.cs
// 命令模式接口与命令调度器。支持 Execute / Undo 的可撤销操作。
// ══════════════════════════════════════════════════════════════════════
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 可撤销命令接口。
/// 所有可撤销操作（建造、拆除、制作等）实现此接口。
/// </summary>
public interface ICommand
{
    /// <summary>命令描述（用于 UI 显示）</summary>
    string Description { get; }

    /// <summary>执行命令</summary>
    void Execute();

    /// <summary>撤销命令</summary>
    void Undo();
}

/// <summary>
/// 命令调度器。管理命令的执行、撤销和重做。
///
/// 核心职责：
///   · 执行命令并压入撤销栈
///   · 支持 Undo / Redo 操作
///   · 限制历史记录容量避免内存膨胀
///
/// 设计说明：
///   · 无 MonoBehaviour 依赖，纯 C# 类
///   · 通过 ServiceLocator 注册供全局访问
///   · 执行新命令时清空重做栈
/// </summary>
public class CommandInvoker
{
    // ══════════════════════════════════════════════════════
    // 配置
    // ══════════════════════════════════════════════════════

    private readonly int _maxHistorySize;

    // ══════════════════════════════════════════════════════
    // 数据
    // ══════════════════════════════════════════════════════

    private readonly LinkedList<ICommand> _undoStack = new LinkedList<ICommand>();
    private readonly Stack<ICommand> _redoStack = new Stack<ICommand>();

    // ══════════════════════════════════════════════════════
    // 属性
    // ══════════════════════════════════════════════════════

    /// <summary>是否可撤销</summary>
    public bool CanUndo => _undoStack.Count > 0;

    /// <summary>是否可重做</summary>
    public bool CanRedo => _redoStack.Count > 0;

    /// <summary>撤销栈深度</summary>
    public int UndoCount => _undoStack.Count;

    /// <summary>重做栈深度</summary>
    public int RedoCount => _redoStack.Count;

    // ══════════════════════════════════════════════════════
    // 构造
    // ══════════════════════════════════════════════════════

    /// <param name="maxHistorySize">最大历史记录数（默认 50）</param>
    public CommandInvoker(int maxHistorySize = 50)
    {
        _maxHistorySize = maxHistorySize;
    }

    // ══════════════════════════════════════════════════════
    // 公有 API
    // ══════════════════════════════════════════════════════

    /// <summary>执行命令并压入撤销栈</summary>
    public void Execute(ICommand command)
    {
        if (command == null) return;

        command.Execute();
        _undoStack.AddLast(command);

        // 容量限制：移除最旧的记录
        while (_undoStack.Count > _maxHistorySize)
        {
            _undoStack.RemoveFirst();
        }

        // 执行新命令后清空重做栈
        _redoStack.Clear();
    }

    /// <summary>撤销最近一个命令</summary>
    public void Undo()
    {
        if (_undoStack.Count == 0) return;

        var command = _undoStack.Last.Value;
        _undoStack.RemoveLast();

        command.Undo();
        _redoStack.Push(command);
    }

    /// <summary>重做最近一个被撤销的命令</summary>
    public void Redo()
    {
        if (_redoStack.Count == 0) return;

        var command = _redoStack.Pop();
        command.Execute();
        _undoStack.AddLast(command);
    }

    /// <summary>清空所有历史</summary>
    public void Clear()
    {
        _undoStack.Clear();
        _redoStack.Clear();
    }
}
