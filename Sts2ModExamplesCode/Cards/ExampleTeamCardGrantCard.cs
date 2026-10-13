using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using Sts2ModExamples.Core;

namespace Sts2ModExamples.Cards;

/// <summary>
/// 示例队友发卡卡：攻击后按「实际造成伤害」（未格挡伤害汇总）给目标叠中毒，
/// 再给每个存活玩家队友各生成一张 ExampleTokenAmplifyCard 随机入抽牌堆。
/// 对照 skill：card/card-api-effects.md（MarisaMod FourOfASpark）。
/// 真实 API：AttackCommand.Results（IEnumerable&lt;List&lt;DamageResult&gt;&gt;，Execute 后填充）、
/// CombatState.CreateCard&lt;T&gt;(Player)、CardPileCmd.AddGeneratedCardToCombat、LocalContext.IsMe、CardCmd.PreviewCardPileAdd。
/// </summary>
[CardPool(typeof(ColorlessCardPool))]
public class ExampleTeamCardGrantCard : CardModel
{
    public ExampleTeamCardGrantCard() : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
    {
    }

    public override CardMultiplayerConstraint MultiplayerConstraint =>
        CardMultiplayerConstraint.MultiplayerOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(10m, ValueProp.Move)
    ];

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

        var attackCommand = await DamageCmd.Attack(DynamicVars.Damage.BaseValue).FromCard(this)
            .Targeting(cardPlay.Target)
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(choiceContext);

        // 实际造成伤害 = 所有目标未格挡伤害之和（格挡掉的、miss 的不算）
        var amount = attackCommand.Results.SelectMany(r => r).Sum(r => r.UnblockedDamage);
        if (amount > 0)
        {
            await PowerCmd.Apply<PoisonPower>(choiceContext, cardPlay.Target, amount, Owner.Creature, this);
        }

        // 队友全员发卡：每个存活玩家队友各生成一张 Token 卡，随机塞进抽牌堆
        var teammates = CombatState?.GetTeammatesOf(Owner.Creature)
            .Where(c => c is { IsAlive: true, IsPlayer: true });
        if (teammates == null) return;
        foreach (var creature in teammates)
        {
            if (creature.Player == null) continue;

            var card = CombatState?.CreateCard<ExampleTokenAmplifyCard>(creature.Player);
            if (card == null) continue;
            var result = await CardPileCmd.AddGeneratedCardToCombat(
                card, PileType.Draw, Owner, CardPilePosition.Random);

            // 本机玩家才播抽牌堆预览动画，防多人串扰
            if (LocalContext.IsMe(creature))
            {
                CardCmd.PreviewCardPileAdd(result);
            }
        }
    }
}
