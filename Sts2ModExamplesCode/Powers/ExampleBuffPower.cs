using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models;

namespace Sts2ModExamples.Powers;

/// <summary>
/// 示例增益能力：每回合开始给持有者 1 层力量。
/// 对照 skill：power/power-core.md + power/power-effects.md。
/// 真实 API：AfterSideTurnStart(CombatSide, IReadOnlyList&lt;Creature&gt;, ICombatState)，
/// PowerCmd.Apply&lt;T&gt;(PlayerChoiceContext, Creature, decimal, Creature?, CardModel?, bool)。
/// </summary>
public class ExampleBuffPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    // 动态变量（用于 smartDescription：{ExampleBuffPower}）
    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get { yield return new DynamicVar("ExampleBuffPower", 1m); }
    }

    public override async Task AfterSideTurnStart(
        CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side == Owner.Side)
        {
            Flash();   // PowerModel.Flash()（protected，能力专用；遗物用 RelicModel.Flash()）
            await PowerCmd.Apply<StrengthPower>(
                new ThrowingPlayerChoiceContext(), Owner, 1, Owner, null);
        }
    }
}
