
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using Sts2ModExamples.Core;

namespace Sts2ModExamples.Cards;

/// <summary>
/// 示例选牌卡：从手牌选 1 张牌升级。
/// 对照 skill：card/card-api.md（选择器）+ card/card-api-select.md。
/// 真实 API：CardSelectCmd.FromHandForUpgrade(PlayerChoiceContext, Player, AbstractModel)。
/// </summary>
[CardPool(typeof(ColorlessCardPool))]
public class ExampleSelectCard : CardModel
{
    public ExampleSelectCard() : base(
        1,
        CardType.Skill,
        CardRarity.Uncommon,
        TargetType.None)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = await CardSelectCmd.FromHandForUpgrade(
            choiceContext, Owner, this);
        if (card != null)
        {
            await CardCmd.Upgrade(card);
        }
    }
}
