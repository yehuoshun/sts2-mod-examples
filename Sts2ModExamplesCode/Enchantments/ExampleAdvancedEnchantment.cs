using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Sts2ModExamples.Enchantments;

/// <summary>
/// 示例高级附魔：攻击牌可附魔，附魔后伤害 +2，并在卡面显示额外文本。
/// 对照 skill：enchantment/enchantment-advanced.md。
/// 真实 API：HasExtraCardText virtual（ExtraCardText 是 private，本地化键 <ID>.extraCardText 自动）。
/// </summary>
public class ExampleAdvancedEnchantment : EnchantmentModel
{
    public override bool HasExtraCardText => true;   // 显示卡面额外文本

    public override bool CanEnchant(CardModel card)
    {
        return card.Type == CardType.Attack;
    }

    protected override void OnEnchant()
    {
        Card.DynamicVars.Damage.UpgradeValueBy(2m);
    }
}
