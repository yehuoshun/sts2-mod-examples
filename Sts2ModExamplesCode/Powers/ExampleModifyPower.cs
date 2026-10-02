using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2ModExamples.Powers;

/// <summary>
/// 示例伤害修正能力：持有者攻击伤害 +3。
/// 对照 skill：power/power-effects.md（Modify* 钩子）。
/// 真实 API：ModifyDamageAdditive(Creature?, decimal, ValueProp, Creature?, CardModel?)。
/// </summary>
public class ExampleModifyPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override decimal ModifyDamageAdditive(
        Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (dealer == Owner)
            return amount + 3m * Amount;
        return amount;
    }
}
