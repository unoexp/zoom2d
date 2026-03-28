// ══════════════════════════════════════════════════════════════════════
// 📁 Assets/_Game/03_Core/Crafting/CraftingValidator.cs
// 制作条件验证器。纯逻辑，无状态，易于测试。
// ══════════════════════════════════════════════════════════════════════
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 制作条件验证器（静态工具类）。
///
/// 将验证逻辑从 CraftingSystem 中分离，便于单独测试。
/// 按照优先级依次检查：配方存在 → 已解锁 → 工作台 → 材料充足 → 背包空间。
/// </summary>
public static class CraftingValidator
{
    /// <summary>
    /// 验证制作条件是否满足。
    /// </summary>
    /// <param name="recipeId">配方ID</param>
    /// <param name="recipeMap">配方查找表</param>
    /// <param name="unlockedRecipes">已解锁配方集合</param>
    /// <param name="inventorySystem">背包系统接口</param>
    /// <param name="nearWorkbench">是否在工作台附近</param>
    /// <returns>验证结果</returns>
    public static CraftingResult Validate(
        string recipeId,
        Dictionary<string, RecipeDefinitionSO> recipeMap,
        HashSet<string> unlockedRecipes,
        IInventorySystem inventorySystem,
        bool nearWorkbench)
    {
        // 1. 配方是否存在
        if (string.IsNullOrEmpty(recipeId) || !recipeMap.TryGetValue(recipeId, out var recipe))
        {
            Debug.LogWarning($"[CraftingValidator] 配方不存在：{recipeId}");
            return CraftingResult.Failed_Unknown;
        }

        // 2. 是否已解锁
        if (!unlockedRecipes.Contains(recipeId))
        {
            return CraftingResult.Failed_NoUnlock;
        }

        // 3. 是否需要工作台
        if (recipe.RequiresWorkbench && !nearWorkbench)
        {
            return CraftingResult.Failed_NoWorkbench;
        }

        // 4. 材料是否充足
        if (recipe.Ingredients != null)
        {
            // [PERF] 无 LINQ，直接遍历
            for (int i = 0; i < recipe.Ingredients.Length; i++)
            {
                var ingredient = recipe.Ingredients[i];
                if (ingredient.Item == null) continue;

                int owned = inventorySystem.GetTotalItemCount(ingredient.Item.ItemId);
                if (owned < ingredient.Amount)
                {
                    return CraftingResult.Failed_NoMaterial;
                }
            }
        }

        // 所有条件通过
        return CraftingResult.Success;
    }
}
