using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Sts2ModExamples.Patches;

/// <summary>
/// 示例 Harmony Prefix 补丁：修改方法参数（所有获得格挡 +2）。
/// 对照 skill：harmony/harmony-basics.md。
/// 真实 API：HarmonyPrefix + ref 参数；TargetMethod 精确定位重载（GainBlock 有 BlockVar/decimal 两个重载）。
/// </summary>
[HarmonyPatch]
public static class ExamplePrefixPatch
{
    private static MethodBase? TargetMethod()
    {
        return AccessTools.Method(typeof(CreatureCmd), nameof(CreatureCmd.GainBlock),
            new[] { typeof(Creature), typeof(decimal), typeof(ValueProp), typeof(CardPlay), typeof(bool) });
    }

    private static void Prefix(ref decimal amount)
    {
        amount += 2m;
    }
}
