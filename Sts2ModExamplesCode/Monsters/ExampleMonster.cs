using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;

namespace Sts2ModExamples.Monsters;

/// <summary>
/// 示例怪物：攻击/防御双状态 AI。
/// 对照 skill：monster/monster-core.md。
/// 真实 API：MinInitialHp/MaxInitialHp 抽象、GenerateMoveStateMachine() protected abstract、
/// MoveState(string, Func&lt;IReadOnlyList&lt;Creature&gt;, Task&gt;, params AbstractIntent[])、
/// MonsterMoveStateMachine(IEnumerable&lt;MonsterState&gt;, MonsterState)。
/// </summary>
public class ExampleMonster : MonsterModel
{
    public override int MinInitialHp => 40;
    public override int MaxInitialHp => 46;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var attack = new MoveState("ATTACK_MOVE", AttackMove, new SingleAttackIntent(6));
        var defend = new MoveState("DEFEND_MOVE", DefendMove, new DefendIntent());
        attack.FollowUpState = defend;
        defend.FollowUpState = attack;

        return new MonsterMoveStateMachine(
            new List<MonsterState> { attack, defend },
            attack);   // 第二参：初始状态对象（不是字符串 ID）
    }

    private async Task AttackMove(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(6).FromMonster(this)
            .Targeting(targets[0])
            .WithHitFx("vfx/vfx_attack_slash")
            .Execute(null);
    }

    private async Task DefendMove(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.GainBlock(Creature, 5, ValueProp.Move, null);
    }
}
