using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;

namespace Sts2ModExamples.Monsters;

/// <summary>
/// 【skill 全面测试】测试怪物：狂暴者（多段攻击/单段攻击/防御三状态循环）。
/// 对照 skill：monster/monster-core.md + monster-ai.md。
/// 真实 API：GenerateMoveStateMachine() protected abstract、MoveState(string, Func&lt;IReadOnlyList&lt;Creature&gt;, Task&gt;, params AbstractIntent[])、
/// MultiAttackIntent(decimal, int)、FollowUpState 链。
/// </summary>
public class TestBerserkerMonster : MonsterModel
{
    public override int MinInitialHp => 30;
    public override int MaxInitialHp => 36;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var flurry = new MoveState("FLURRY_MOVE", FlurryMove, new MultiAttackIntent(4, 3));
        var slam = new MoveState("SLAM_MOVE", SlamMove, new SingleAttackIntent(10));
        var defend = new MoveState("DEFEND_MOVE", DefendMove, new DefendIntent());
        flurry.FollowUpState = slam;
        slam.FollowUpState = defend;
        defend.FollowUpState = flurry;

        return new MonsterMoveStateMachine(
            new List<MonsterState> { flurry, slam, defend },
            flurry);   // 第二参：初始状态对象
    }

    private async Task FlurryMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(4).FromMonster(this)
            .WithHitCount(3)
            .Targeting(targets[0])
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(null);
    }

    private async Task SlamMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(10).FromMonster(this)
            .Targeting(targets[0])
            .WithHitFx("vfx/vfx_attack_slam")
            .Execute(null);
    }

    private async Task DefendMove(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.GainBlock(Creature, 7, ValueProp.Move, null);
    }
}
