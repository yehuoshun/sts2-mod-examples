using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.PotionPools;
using Sts2ModExamples.Core;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace Sts2ModExamples.Potions;

/// <summary>
/// 示例药水：使用时获得 2 层力量。
/// 对照 skill：potion/potion-core.md + potion/potion-callbacks.md。
/// 真实 API：OnUse(PlayerChoiceContext, Creature?) protected virtual；
/// ⚠️ PlayerChoiceContext 无 Player 属性（YuWan docs 简写错误），玩家用 Owner.Creature。
/// </summary>
[PotionPool(typeof(SharedPotionPool))]
public class ExamplePotion : PotionModel
{
    public override PotionRarity Rarity => PotionRarity.Common;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        await PowerCmd.Apply<StrengthPower>(
            choiceContext, Owner.Creature, 2, Owner.Creature, null);
    }
}
