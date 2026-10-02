using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using Sts2ModExamples.Core;

namespace Sts2ModExamples.Cards;

/// <summary>
/// 示例多人限定卡牌：仅多人模式出现。
/// 对照 skill：multiplayer/multiplayer-core.md。
/// 真实 API：CardMultiplayerConstraint（CardModel.cs 295 行 virtual 属性）。
/// </summary>
[CardPool(typeof(ColorlessCardPool))]
public class ExampleMultiplayerOnlyCard : CardModel
{
    public ExampleMultiplayerOnlyCard() : base(
        1,
        CardType.Skill,
        CardRarity.Rare,
        TargetType.None)
    {
    }

    public override CardMultiplayerConstraint MultiplayerConstraint =>
        CardMultiplayerConstraint.MultiplayerOnly;
}
