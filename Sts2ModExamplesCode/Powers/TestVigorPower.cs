using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2ModExamples.Powers;

/// <summary>
/// 【skill 全面测试】自定义能力：力量式伤害修正（持有者造成攻击伤害 +Amount）。
/// 对照 skill：power/power-core.md（模板/Type/StackType）+ power-callbacks.md（真实回调）。
/// 真实 API：ModifyDamageAdditive(Creature?, decimal, ValueProp, Creature?, CardModel?) @ AbstractModel.cs，
/// ValuePropExtensions.IsPoweredAttack()。写法与原生 StrengthPower 一致。
/// </summary>
public class TestVigorPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // 动态变量（用于 smartDescription：{TestVigorPower}）
    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get { yield return new DynamicVar("TestVigorPower", 1m); }
    }

    public override decimal ModifyDamageAdditive(
        Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (Owner != dealer) return 0m;
        return props.IsPoweredAttack() ? Amount : 0m;
    }
}
