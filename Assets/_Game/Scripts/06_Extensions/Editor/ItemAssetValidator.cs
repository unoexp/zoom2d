// 📁 Assets/_Game/06_Extensions/EditorTools/ItemAssetValidator.cs
// ─────────────────────────────────────────────────────────────────────
// 物品 SO 路径校验工具
// 监听资产的 创建 / 移动 / 复制 操作，
// 检测 ItemDefinitionSO 子类是否存放在正确的目录下，
// 不符合规则时在 Console 输出错误并弹出确认对话框。
// ─────────────────────────────────────────────────────────────────────

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ItemAssetValidator : AssetPostprocessor
{
    // ══════════════════════════════════════════════════════
    // 路径规则表
    // Key   = 具体 SO 类型（精确匹配子类，不匹配基类）
    // Value = 该类型 .asset 文件必须存放的目录（前缀匹配）
    // ══════════════════════════════════════════════════════
    private static readonly Dictionary<Type, string> _rules
        = new Dictionary<Type, string>
    {
        { typeof(ConsumableItemSO), "Assets/GameData/Items/Consumable" },
        // { typeof(WeaponItemSO),     "Assets/GameData/Items/Weapon"     },
        // { typeof(MaterialItemSO),   "Assets/GameData/Items/Material"   },
        // { typeof(ToolItemSO),       "Assets/GameData/Items/Tool"       },
        // { typeof(EquipmentItemSO),  "Assets/GameData/Items/Equipment"  },
        // { typeof(AmmoItemSO),       "Assets/GameData/Items/Ammo"       },
        // 💡 新增物品子类时，在此追加一条规则即可
    };

    // ══════════════════════════════════════════════════════
    // AssetPostprocessor 钩子
    // OnPostprocessAllAssets 覆盖 创建/移动/复制 三种操作
    // ══════════════════════════════════════════════════════

    private static void OnPostprocessAllAssets(
        string[] importedAssets,    // 新创建 或 被修改的资产
        string[] deletedAssets,     // 已删除（本工具不关心）
        string[] movedAssets,       // 移动后的新路径
        string[] movedFromAssets)   // 移动前的旧路径（本工具不关心）
    {
        // 合并"新导入"和"移动后"的路径，统一校验
        var toValidate = new List<string>(importedAssets.Length + movedAssets.Length);
        toValidate.AddRange(importedAssets);
        toValidate.AddRange(movedAssets);

        foreach (var path in toValidate)
            ValidatePath(path);
    }

    // ══════════════════════════════════════════════════════
    // 核心校验逻辑
    // ══════════════════════════════════════════════════════

    private static void ValidatePath(string assetPath)
    {
        // 只处理 .asset 文件，其余一律跳过
        if (!assetPath.EndsWith(".asset", StringComparison.OrdinalIgnoreCase))
            return;

        // 加载资产对象
        var asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);
        if (asset == null) return;

        // 不是 ItemDefinitionSO 的子类，跳过
        if (asset is not ItemDefinitionSO) return;

        var assetType = asset.GetType();

        // 在规则表里查找对应类型
        if (!_rules.TryGetValue(assetType, out var expectedFolder))
        {
            // 类型存在但未配置规则：提示开发者补充规则
            LogWarningUnregistered(assetType, assetPath);
            return;
        }

        // 检查路径前缀是否匹配
        if (!assetPath.StartsWith(expectedFolder, StringComparison.OrdinalIgnoreCase))
            LogErrorWrongPath(assetType, assetPath, expectedFolder);
    }

    // ══════════════════════════════════════════════════════
    // 日志输出
    // ══════════════════════════════════════════════════════

    private static void LogErrorWrongPath(Type type, string actualPath, string expectedFolder)
    {
        // 加载资产对象用于 Console 双击定位
        var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(actualPath);

        Debug.LogError(
            $"[ItemAssetValidator] ❌ 路径错误！\n" +
            $"类型：{type.Name}\n" +
            $"当前路径：{actualPath}\n" +
            $"应存放于：{expectedFolder}/\n" +
            $"请将此文件移动到正确目录。",
            asset   // 绑定资产对象，双击 Console 可直接定位文件
        );

        // 弹出对话框，明确告知（避免被忽略的 Console 错误）
        EditorUtility.DisplayDialog(
            title:   "⚠️ 物品资产路径错误",
            message: $"{type.Name} 类型的资产必须放在：\n\n" +
                     $"  {expectedFolder}/\n\n" +
                     $"当前位置：\n  {actualPath}\n\n" +
                     $"请手动移动到正确目录。",
            ok:      "知道了"
        );
    }

    private static void LogWarningUnregistered(Type type, string path)
    {
        var asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);

        Debug.LogWarning(
            $"[ItemAssetValidator] ⚠️ 未配置路径规则！\n" +
            $"类型：{type.Name} 是 ItemDefinitionSO 的子类，但在 _rules 中没有对应规则。\n" +
            $"请在 ItemAssetValidator._rules 中为该类型添加路径规则。\n" +
            $"文件路径：{path}",
            asset
        );
    }
}
#endif