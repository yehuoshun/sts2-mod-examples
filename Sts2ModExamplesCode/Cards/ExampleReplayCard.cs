using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using Sts2ModExamples.Core;

namespace Sts2ModExamples.Cards;

/// <summary>
/// 示例跨战斗持久化卡牌：永久累计多次打出次数。
/// 对照 skill：serialization/serialization-save.md（BaseReplayCount + DeckVersion 模式）。
/// 真实 API：[SavedProperty]、AfterDeserialized() protected virtual、DeckVersion（CardModel 929 行）。
/// </summary>
[CardPool(typeof(ColorlessCardPool))]
public class ExampleReplayCard : CardModel
{
    [SavedProperty]
    public int Example_PermanentReplayCount { get; set; }

    public ExampleReplayCard() : base(
        1,
        CardType.Attack,
        CardRarity.Rare,
        TargetType.AnyEnemy)
    {
    }

    protected override void AfterDeserialized()
    {
        base.AfterDeserialized();
        BaseReplayCount = Example_PermanentReplayCount;   // 读档恢复
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        await DamageCmd.Attack(5)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        // 同步回牌组规范实例（DeckVersion）才能跨战斗持久化
        if (DeckVersion is ExampleReplayCard deckCard)
        {
            deckCard.Example_PermanentReplayCount += 1;
            deckCard.BaseReplayCount = deckCard.Example_PermanentReplayCount;
        }
    }
}
