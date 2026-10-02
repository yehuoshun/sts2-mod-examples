using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;   // TargetType 枚举所在命名空间
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.PotionPools;
using Sts2ModExamples.Core;
using Sts2ModExamples.Powers;

namespace Sts2ModExamples.Potions;

/// <summary>
/// 【skill 全面测试】测试药水：使用时获得 3 层 TestVigorPower。
/// 对照 skill：potion/potion-core.md（模板/Rarity/Usage/TargetType）+ potion-callbacks.md（OnUse）。
/// 真实 API：OnUse(PlayerChoiceContext, Creature?) protected virtual。
/// </summary>
[PotionPool(typeof(SharedPotionPool))]
public class TestVigorPotion : PotionModel
{
    public override PotionRarity Rarity => PotionRarity.Common;
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;

    protected override async Task OnUse(PlayerChoiceContext choiceContext, Creature? target)
    {
        await PowerCmd.Apply<TestVigorPower>(
            choiceContext, Owner.Creature, 3, Owner.Creature, null);
    }
}
