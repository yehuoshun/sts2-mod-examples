using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using Sts2ModExamples.Core;
using Sts2ModExamples.Powers;

namespace Sts2ModExamples.Cards;

/// <summary>
/// 【skill 全面测试】测试攻击牌：伤害 + 给目标施加自定义能力 TestVigorPower。
/// 对照 skill：card/card-constructor.md（构造 5 参）+ card-api.md（DamageCmd 链/PowerCmd.Apply）+ card-api-effects.md（攻击+施加能力组合）。
/// 真实 API：DamageCmd.Attack(decimal).FromCard().Targeting().Execute(PlayerChoiceContext)；PowerCmd.Apply&lt;T&gt;。
/// </summary>
[CardPool(typeof(ColorlessCardPool))]   // 无色卡池（ContentRegistry 自动注册）
public class TestSynergyStrike : CardModel
{
    public TestSynergyStrike() : base(
        1,
        type: CardType.Attack,
        rarity: CardRarity.Common,
        targetType: TargetType.AnyEnemy,
        shouldShowInCardLibrary: true)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        await DamageCmd.Attack(5)
            .FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        // 给目标叠加 1 层 TestVigorPower（攻击+施加能力组合）
        await PowerCmd.Apply<TestVigorPower>(
            choiceContext, cardPlay.Target, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(2m);   // 升级 +2 伤害
    }
}
