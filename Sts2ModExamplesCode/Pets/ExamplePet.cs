using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Models;

namespace Sts2ModExamples.Pets;

/// <summary>
/// 示例宠物：固定不行动（继承 MonsterModel，无 AI 行为，由卡牌/能力控制）。
/// 对照 skill：pet/pet.md。
/// 真实 API：IsInteractable virtual（0.113 起：是否为宠物由 PetOwner 非空判定，
/// 血条显示不再有 IsHealthBarVisible 开关，不可交互的宠物 override IsInteractable = false）、
/// GenerateMoveStateMachine protected abstract、MoveState + HiddenIntent（无行动意图）。
/// </summary>
public class ExamplePet : MonsterModel
{
    public override int MinInitialHp => 10;
    public override int MaxInitialHp => 10;

    public override bool IsInteractable => false;   // 不可交互宠物（血条/选中由 PetOwner + 该属性决定）

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var nothing = new MoveState("NOTHING", _ => Task.CompletedTask, new HiddenIntent());
        nothing.FollowUpState = nothing;
        return new MonsterMoveStateMachine(
            new List<MonsterState> { nothing }, nothing);
    }
}
