using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using Sts2ModExamples.Core;
using Sts2ModExamples.Powers;

namespace Sts2ModExamples.Cards;

/// <summary>
/// 示例「回合末延迟交互」卡：打出时不选牌，只给每个存活玩家队友各挂一份 ExampleDeferredSelectPower，
/// 由 Power 在自己的回合结束时弹选牌、结算、再移除。与 ExampleAnySelectExhaustCard 的即时版对照。
/// 对照 skill：power/power-deferred-turnend.md（MarisaMod HeatDeathOfUniverse）
///          + card/card-api-select.md（CardSelectorPrefs / CardSelectCmd.FromHand）。
/// </summary>
[CardPool(typeof(ColorlessCardPool))]
public class ExampleDeferredSelectExhaustCard : CardModel
{
    public ExampleDeferredSelectExhaustCard() : base(1, CardType.Skill, CardRarity.Rare, TargetType.AllAllies)
    {
    }

    public override CardMultiplayerConstraint MultiplayerConstraint =>
        CardMultiplayerConstraint.MultiplayerOnly;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // 每个存活玩家队友的角色各挂一份（同一玩家多角色时去重，防重复弹窗）
        var playersAvail = CombatState!.GetTeammatesOf(Owner.Creature)
            .Where(c => c is { IsAlive: true, IsPlayer: true })
            .Select(c => c.Player!)
            .Distinct()
            .ToList();

        foreach (var player in playersAvail)
        {
            // PowerCmd.Apply 返回刚挂上的实例：用它把「给谁结算」回填进 Power，不必改构造函数
            var pow = await PowerCmd.Apply<ExampleDeferredSelectPower>(
                choiceContext, player.Creature, 1, Owner.Creature, this);
            if (pow != null) pow.PowerTarget = player.Creature;
        }
    }
}
