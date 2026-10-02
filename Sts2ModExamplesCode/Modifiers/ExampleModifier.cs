using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2ModExamples.Modifiers;

/// <summary>
/// 示例修改器：所有攻击伤害 +2，带 Neow 选项与存档属性。
/// 对照 skill：modifier/modifier-core.md + modifier/modifier-effects.md。
/// 真实 API：AfterRunCreated(RunState)、ModifyDamageAdditive(Creature?, decimal, ValueProp, Creature?, CardModel?)。
/// </summary>
public class ExampleModifier : ModifierModel
{
    [SavedProperty]
    public int Example_Stacks { get; set; }

    protected override void AfterRunCreated(RunState runState)
    {
        // 可选：开局改全局配置
    }

    public override decimal ModifyDamageAdditive(
        Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (dealer?.Player != null && props.IsPoweredAttack())
            return amount + 2m;
        return amount;
    }
}
