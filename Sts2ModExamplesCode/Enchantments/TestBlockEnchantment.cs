using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2ModExamples.Enchantments;

/// <summary>
/// 【skill 全面测试】测试附魔：防御牌可附魔，打出时获得格挡，附魔层数加成格挡。
/// 对照 skill：enchantment/enchantment-core.md。
/// 真实 API：CanEnchantCardType(CardType)、EnchantBlockAdditive(decimal)（单参）、
/// OnPlay(PlayerChoiceContext, CardPlay?)（不是 OnCardPlayed）、RecalculateValues()。
/// </summary>
public class TestBlockEnchantment : EnchantmentModel
{
    public override bool ShowAmount => true;

    // 只可附到防御牌上
    public override bool CanEnchantCardType(CardType cardType)
    {
        return cardType == CardType.Skill;
    }

    // 格挡增量 = 附魔层数（真实签名：单参）
    public override decimal EnchantBlockAdditive(decimal originalBlock)
    {
        return originalBlock + Amount;
    }

    // 打出附魔卡时：获得 Amount 点格挡
    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        await CreatureCmd.GainBlock(
            Card.Owner.Creature, Amount, ValueProp.Move, cardPlay);
    }

    // 数值跟随层数
    public override void RecalculateValues()
    {
        // 如需动态变量同步：DynamicVars.X.BaseValue = Amount;
    }
}
