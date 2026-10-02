using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2ModExamples.Modifiers;

/// <summary>
/// 【skill 全面测试】测试 Modifier：玻璃大炮（玩家受到攻击伤害 +50%，乘算钩子）。
/// 对照 skill：modifier/modifier-core.md + modifier-effects.md。
/// 真实 API：AfterRunCreated(RunState) protected virtual、ModifyDamageMultiplicative 钩子。
/// </summary>
public class TestGlassCannonModifier : ModifierModel
{
    protected override void AfterRunCreated(RunState runState)
    {
        // 可选：开局改全局配置
    }

    // 玩家受到的攻击伤害 ×1.5
    public override decimal ModifyDamageMultiplicative(
        Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target?.Player != null && props.IsPoweredAttack())
            return amount * 1.5m;
        return amount;
    }
}
