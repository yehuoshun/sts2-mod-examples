using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2ModExamples.Orbs;

/// <summary>
/// 【skill 全面测试】测试球体：暗影球（被动/激发造成伤害）。
/// 对照 skill：orb/orb-core.md。
/// 真实 API：PassiveVal/EvokeVal/DarkenedColor 抽象必填；音效 protected virtual；
/// Passive(PlayerChoiceContext, Creature?)/Evoke(PlayerChoiceContext) public virtual；
/// CreatureCmd.Damage(choiceContext, targets, value, ValueProp, dealer)。
/// </summary>
public class TestShadowOrb : OrbModel
{
    public override Color DarkenedColor => new("2D2D4A");
    public override decimal PassiveVal => 2m;
    public override decimal EvokeVal => 5m;

    protected override string PassiveSfx => "event:/sfx/characters/defect/defect_dark_passive";
    protected override string EvokeSfx => "event:/sfx/characters/defect/defect_dark_evoke";

    public override async Task BeforeTurnEndOrbTrigger(PlayerChoiceContext choiceContext)
    {
        await Passive(choiceContext, null);
    }

    public override async Task Passive(PlayerChoiceContext choiceContext, Creature? target)
    {
        Trigger();
        await DealDamage(PassiveVal, target, choiceContext);
    }

    public override async Task<IEnumerable<Creature>> Evoke(PlayerChoiceContext choiceContext)
    {
        return await DealDamage(EvokeVal, null, choiceContext);
    }

    private async Task<IEnumerable<Creature>> DealDamage(
        decimal value, Creature? target, PlayerChoiceContext choiceContext)
    {
        var opponents = CombatState.GetOpponentsOf(Owner.Creature)
            .Where(e => e.IsHittable).ToList();
        if (opponents.Count == 0) return [];

        // 原生 DarkOrb 同款写法：先 await Damage，再手动构造被击中目标列表返回
        // （CreatureCmd.Damage 返回 IEnumerable<DamageResult>，不能直接当 Creature 用）
        var targets = target == null
            ? new List<Creature> { Owner.RunState.Rng.CombatTargets.NextItem(opponents)! }
            : new List<Creature> { target };

        await CreatureCmd.Damage(
            choiceContext, targets, value, ValueProp.Unpowered, Owner.Creature);
        return targets;
    }
}
