using System.Threading.Tasks;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Models;

namespace Sts2ModExamples.Pets;

/// <summary>
/// 【skill 全面测试】测试宠物：守护精灵（显示血条，固定不行动）。
/// 对照 skill：pet/pet.md。
/// 真实 API：IsHealthBarVisible virtual（宠物可显示血条）、GenerateMoveStateMachine protected abstract、
/// MoveState + HiddenIntent（无行动意图）。
/// </summary>
public class TestGuardianSpirit : MonsterModel
{
    public override int MinInitialHp => 25;
    public override int MaxInitialHp => 25;

    public override bool IsHealthBarVisible => true;   // 与 ExamplePet 相反：显示血条

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var nothing = new MoveState("NOTHING", _ => Task.CompletedTask, new HiddenIntent());
        nothing.FollowUpState = nothing;
        return new MonsterMoveStateMachine(
            new List<MonsterState> { nothing }, nothing);
    }
}
