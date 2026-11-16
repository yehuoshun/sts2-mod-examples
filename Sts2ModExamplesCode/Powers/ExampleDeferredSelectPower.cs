using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Sts2ModExamples.Powers;

/// <summary>
/// 示例「回合末延迟交互」能力：己方回合结束时从手牌选任意张（0-99，可一张不选）烧掉，
/// 每烧一张给 <see cref="PowerTarget"/> 叠 1 层 ExampleModifyPower；结算完在回合结束后整个移除（一次性开关）。
/// 对照 skill：power/power-deferred-turnend.md（MarisaMod HeatDeathOfUniversePower）。
/// 真实 API：BeforeSideTurnEnd(PlayerChoiceContext, CombatSide, IEnumerable&lt;Creature&gt;) + CardSelectCmd.FromHand + PowerCmd.Remove。
/// </summary>
public class ExampleDeferredSelectPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    /// <summary>由施加方（卡牌）在 Apply 之后回填的收益目标。</summary>
    public Creature? PowerTarget;

    public override async Task BeforeSideTurnEnd(
        PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        // 只在己方回合末弹选择
        if (side != Owner.Side) return;
        if (Owner.Player == null) return;

        var sum = 0;
        foreach (var item in await CardSelectCmd.FromHand(
                     choiceContext, Owner.Player,
                     new CardSelectorPrefs(SelectionScreenPrompt, 0, 99), null, this))
        {
            await CardCmd.Exhaust(choiceContext, item);
            sum += 1;
        }

        if (PowerTarget != null && sum > 0)
        {
            await PowerCmd.Apply<ExampleModifyPower>(choiceContext, PowerTarget, sum, Owner, null);
        }
    }

    public override async Task AfterSideTurnEnd(
        PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        // 交互已在上一个钩子里做完，这里直接移除
        if (side == Owner.Side) await PowerCmd.Remove(this);
    }
}
