// ─────────────────────────────────────────────────────────────────────
// 📁 Assets/_Game/01_Data/SaveData/SurvivalSaveData.cs
// 生存系统的存档数据结构，纯数据，可序列化
// ─────────────────────────────────────────────────────────────────────
using System;
using System.Collections.Generic;

[Serializable]
public class SurvivalSaveData
{
    /// <summary>各属性当前值</summary>
    public Dictionary<SurvivalAttributeType, float> AttributeValues
        = new Dictionary<SurvivalAttributeType, float>();

    /// <summary>各属性当前最大值（可被装备/升级修改，需持久化）</summary>
    public Dictionary<SurvivalAttributeType, float> AttributeMaxValues
        = new Dictionary<SurvivalAttributeType, float>();

    /// <summary>需要跨存档保留的永久状态效果 ID 列表</summary>
    public List<string> PermanentEffectIds
        = new List<string>();
}
