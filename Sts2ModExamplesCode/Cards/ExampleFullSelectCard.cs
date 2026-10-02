using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using Sts2ModExamples.Core;

namespace Sts2ModExamples.Cards;

/// <summary>
/// 示例完整选择器：CardSelectorPrefs 自定义提示 + filter 过滤（只能选攻击牌）。
/// 对照 skill：card/card-api-select.md。
/// 真实 API：CardSelectorPrefs(LocString, int) 构造 + CardSelectCmd.FromHand(PlayerChoiceContext, Player, CardSelectorPrefs, Func&lt;CardModel,bool&gt;?, AbstractModel)。
/// </summary>
[CardPool(typeof(ColorlessCardPool))]
public class ExampleFullSelectCard : CardModel
{
    public ExampleFullSelectCard() : base(
        1,
        CardType.Skill,
        CardRarity.Uncommon,
        TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var prefs = new CardSelectorPrefs(
            new LocString("card_selection", "TO_EXHAUST"), 1);

        var cards = await CardSelectCmd.FromHand(
            choiceContext, Owner, prefs,
            card => card.Type == CardType.Attack,   // 只选攻击牌
            this);
        if (cards != null)
        {
            await CardCmd.Exhaust(choiceContext, cards.First());
        }
    }
}
