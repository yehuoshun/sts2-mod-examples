using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rewards;

namespace Sts2ModExamples.Core;

/// <summary>
/// 运行时枚举注入：为 RewardType / PileType 等原生 enum 注入自定义值。
/// 对照 skill：reward/reward-core.md + pile/pile-inject.md（纯原生自研方案）。
/// 原理：ModelDb.Init 前用 Harmony Prefix 扫描静态字段，反射赋值下一个可用枚举值。
/// </summary>
public static class EnumInjector
{
    public sealed class RewardTypeAttribute : Attribute { }
    public sealed class CustomPileTypeAttribute : Attribute { }
}

/// <summary>自定义奖励类型静态字段（[RewardType] 标记）</summary>
public static class ExampleRewardTypes
{
    [EnumInjector.RewardType]
    public static RewardType CardTransform;
}

/// <summary>自定义牌堆类型静态字段（[CustomPileType] 标记）</summary>
public static class ExamplePileTypes
{
    [EnumInjector.CustomPileType]
    public static PileType VoidPile;
}

[HarmonyPatch(typeof(ModelDb), nameof(ModelDb.Init))]
public static class EnumInjectorPatch
{
    [HarmonyPrefix]
    public static void InjectEnums()
    {
        Inject(typeof(EnumInjector.RewardTypeAttribute), typeof(RewardType));
        Inject(typeof(EnumInjector.CustomPileTypeAttribute), typeof(PileType));
    }

    private static void Inject(Type markerAttr, Type enumType)
    {
        int baseValue = Enum.GetValues(enumType).Cast<int>().Max() + 1;
        int offset = 0;
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in asm.GetTypes())
            {
                foreach (var field in type.GetFields(
                    BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (!field.IsDefined(markerAttr)) continue;
                    if (field.FieldType != enumType) continue;
                    field.SetValue(null, Enum.ToObject(enumType, baseValue + offset));
                    offset++;
                }
            }
        }
    }
}
