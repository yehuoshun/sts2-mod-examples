using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using Sts2ModExamples.Powers;

namespace Sts2ModExamples.Cards;

/// <summary>
/// 示例任意数量选择卡：每个存活玩家队友从手牌选任意张（0-99，可一张不选）烧掉，
/// 每烧一张给持有者叠 1 层 ExampleModifyPower（示例语义：烧牌换增幅）。
/// 对照 skill：card/card-api-select.md（MarisaMod HeatDeathOfUniverse）。
/// 真实 API：CardSelectorPrefs(LocString, minCount, maxCount) + CardSelectCmd.FromHand(context, player, prefs, filter, source)。
/// </summary>
[CardPool(typeof(ColorlessCardPool))]
public class ExampleAnySelectExhaustCard : CardModel
{
    public ExampleAnySelectExhaustCard() : base(1, CardType.Skill, CardRarity.Rare, TargetType.AllAllies)
    {
    }

    public override CardMultiplayerConstraint MultiplayerConstraint =>
        CardMultiplayerConstraint.MultiplayerOnly;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var count = 0;

        // 存活玩家队友去重（同一玩家多角色时防重复弹窗）
        var playersAvail = CombatState!.GetTeammatesOf(Owner.Creature)
            .Where(c => c is { IsAlive: true, IsPlayer: true })
            .Select(c => c.Player!)
            .Distinct()
            .ToList();

        foreach (var player in playersAvail)
        {
            // 0-99 = 任意数量（最小 0 = 允许一张不选直接确认）；filter 传 null 不过滤
            foreach (var item in await CardSelectCmd.FromHand(
                         choiceContext, player,
                         new CardSelectorPrefs(SelectionScreenPrompt, 0, 99), null, this))
            {
                await CardCmd.Exhaust(choiceContext, item);
                count += 1;
            }
        }

        if (count > 0)
        {
            await PowerCmd.Apply<ExampleModifyPower>(
                choiceContext, Owner.Creature, count, Owner.Creature, this);
        }
    }
}
