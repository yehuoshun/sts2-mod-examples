using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2ModExamples.Monsters;

/// <summary>
/// 示例：怪物难度缩放（自定义难度模式）。
/// 对照 skill：run/run-difficulty-scale.md。
/// 真实 API：Creature.ScaleMonsterHpForMultiplayer（原版多人缩放入口，挂其后乘倍率）；
/// Creature.SetMaxHpInternal/SetCurrentHpInternal；PowerModel.ModifyDamageMultiplicative
/// （0.108+ 签名带 CardPlay，条件编译）；ValueProp.IsPoweredAttack()。
/// </summary>
[HarmonyPatch(typeof(Creature), nameof(Creature.ScaleMonsterHpForMultiplayer))]
public static class ExampleMonsterScalingPatch
{
    private const decimal HpMultiplier = 1.25m;   // 示例固定倍率（真实项目读设置中心）

    private static void Postfix(Creature __instance)
    {
        if (!__instance.IsMonster)
        {
            return;
        }

        if (HpMultiplier != 1m)
        {
            int scaledHp = Math.Max(1, (int)Math.Round(__instance.MaxHp * HpMultiplier, MidpointRounding.AwayFromZero));
            __instance.SetMaxHpInternal(scaledHp);      // 直接改内部（真实 API）
            __instance.SetCurrentHpInternal(scaledHp);
        }

        if (HpMultiplier != 1m && !__instance.HasPower<ExampleAttackScalePower>())
        {
            ExampleAttackScalePower power = (ExampleAttackScalePower)ModelDb.Power<ExampleAttackScalePower>().ToMutable();
            power.ApplyInternal(__instance, 125, silent: true);   // Amount 编码倍率百分比
        }
    }
}

/// <summary>隐藏攻击倍率 power：Amount 存百分比，只乘敌人对玩家的「攻击」类伤害。</summary>
public sealed class ExampleAttackScalePower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    protected override bool IsVisibleInternal => false;

#if STS2_108_OR_NEWER
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
#else
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
#endif
    {
        if (dealer != Owner || target == null || !target.IsPlayer)
        {
            return 1m;   // 只影响敌人打玩家
        }

        if (!props.IsPoweredAttack())
        {
            return 1m;   // 只乘「攻击」类伤害
        }

        return Amount > 0 ? Amount / 100m : 1m;
    }
}