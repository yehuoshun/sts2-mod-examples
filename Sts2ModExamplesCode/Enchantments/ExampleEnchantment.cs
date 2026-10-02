using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Sts2ModExamples.Enchantments;

/// <summary>
/// 示例附魔：攻击牌可附魔，附魔后伤害 +2。
/// 对照 skill：enchantment/enchantment-core.md + enchantment/enchantment-advance.md。
/// 真实 API：CanEnchant(CardModel)、OnEnchant() protected virtual（原生无 ApplyEnchantment，那是自研名）。
/// </summary>
public class ExampleEnchantment : EnchantmentModel
{
    public override bool CanEnchant(CardModel card)
    {
        return card.Type == CardType.Attack;
    }

    protected override void OnEnchant()
    {
        Card.DynamicVars.Damage.UpgradeValueBy(2m);
    }
}
