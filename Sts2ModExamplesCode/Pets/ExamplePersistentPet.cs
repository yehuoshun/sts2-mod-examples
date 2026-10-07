using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Saves.Runs;

namespace Sts2ModExamples.Pets;

/// <summary>
/// 示例：宠物进阶——位置持久化 + 视觉复制 + 战斗内生成。
/// 对照 skill：pet/pet-advanced.md。
/// 真实 API：MonsterModel.MinInitialHp/MaxInitialHp 抽象；AbstractModel.ToMutable()；
/// CombatState.CreateCreature(MonsterModel, CombatSide, string?)；PlayerCmd.AddPet(Creature, Player)；
/// CreatureCmd.SetMaxAndCurrentHp(Creature, decimal)；ModelDb.GetByIdOrNull&lt;T&gt;(ModelId)。
/// </summary>
public class ExamplePersistentPet : MonsterModel
{
    // [SavedProperty] 不能直接存 Vector2 → 拆成两个 float
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public float VisualOffsetX { get; set; }

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public float VisualOffsetY { get; set; }

    public Vector2 VisualOffset
    {
        get => new(VisualOffsetX, VisualOffsetY);
        set
        {
            VisualOffsetX = value.X;
            VisualOffsetY = value.Y;
        }
    }

    // 视觉复制源：存 ModelId 两段字符串（不存对象引用，可序列化）
    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public string VisualSourceCategory { get; set; } = string.Empty;

    [SavedProperty(SerializationCondition.SaveIfNotTypeDefault)]
    public string VisualSourceEntry { get; set; } = string.Empty;

    public void SetVisualSource(MonsterModel sourceModel)
    {
        ModelId id = sourceModel.Id;
        VisualSourceCategory = id.Category;
        VisualSourceEntry = id.Entry;
    }

    public MonsterModel? ResolveVisualSourceModel()
    {
        if (string.IsNullOrEmpty(VisualSourceCategory) || string.IsNullOrEmpty(VisualSourceEntry))
        {
            return null;
        }

        return ModelDb.GetByIdOrNull<MonsterModel>(new ModelId(VisualSourceCategory, VisualSourceEntry));
    }

    public override int MinInitialHp => 1;

    public override int MaxInitialHp => 1;

    public override bool IsHealthBarVisible => false;

    public override bool HasDeathSfx => false;

    public override bool HasHurtSfx => false;

    protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/fallback");

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        var nothing = new MoveState("NOTHING", _ => Task.CompletedTask, new HiddenIntent());
        nothing.FollowUpState = nothing;
        return new MonsterMoveStateMachine(new System.Collections.Generic.List<MonsterState> { nothing }, nothing);
    }

    /// <summary>战斗内生成宠物：ToMutable 拷贝原型 + CreateCreature + AddPet。</summary>
    public static async Task<Creature?> Summon(Player player, MonsterModel visualSource)
    {
        var model = (ExamplePersistentPet)ModelDb.Monster<ExamplePersistentPet>().ToMutable();
        model.SetVisualSource(visualSource);

        ICombatState? combatState = player.Creature.CombatState;
        if (combatState == null || player.Creature.IsDead)
        {
            return null;
        }

        Creature pet = combatState.CreateCreature(model, player.Creature.Side, slot: null);
        await PlayerCmd.AddPet(pet, player);
        await CreatureCmd.SetMaxAndCurrentHp(pet, 1m);
        return pet;
    }
}
