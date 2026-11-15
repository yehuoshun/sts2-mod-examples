using System.Threading.Tasks;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Models;

namespace Sts2ModExamples.Pets;

/// <summary>
/// 【skill 全面测试】测试宠物：守护精灵（可交互，固定不行动）。
/// 对照 skill：pet/pet.md。
/// 真实 API：IsInteractable virtual（0.113 起取代 IsHealthBarVisible）、GenerateMoveStateMachine protected abstract、
/// MoveState + HiddenIntent（无行动意图）。
/// </summary>
public class TestGuardianSpirit : MonsterModel
{
    public override int MinInitialHp => 25;
    public override int MaxInitialHp => 25;

    public override bool IsInteractable => true;   // 与 ExamplePet 相反：可交互（默认值也是 true，这里显式写出来对照）

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var nothing = new MoveState("NOTHING", _ => Task.CompletedTask, new HiddenIntent());
        nothing.FollowUpState = nothing;
        return new MonsterMoveStateMachine(
            new List<MonsterState> { nothing }, nothing);
    }
}
