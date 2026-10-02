using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using Sts2ModExamples.Core;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Models;

namespace Sts2ModExamples.Cards;

/// <summary>
/// 示例防御牌（格挡 + 升级）。对照 skill：card/card-constructor.md。
/// 真实 API：CreatureCmd.GainBlock(Creature, BlockVar, CardPlay?, bool)。
/// ⚠️ 原生 DynamicVar 没有 WithUpgrade()（那是自研扩展）——升级一律在 OnUpgrade 里 UpgradeValueBy。
/// </summary>
[CardPool(typeof(ColorlessCardPool))]
public class ExampleDefend : CardModel
{
    public ExampleDefend() : base(
        baseCost: 1,
        type: CardType.Skill,
        rarity: CardRarity.Common,
        target: TargetType.None)
    {
    }

    protected override IEnumerable<DynamicVar> CanonicalVars
    {
        get
        {
            // BlockVar(decimal, ValueProp)：ValueProp.Move = 随敏捷修正
            yield return new BlockVar(5m, ValueProp.Move);
        }
    }

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }

    public override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Block.UpgradeValueBy(2m);   // 升级 +2 格挡
    }
}
