using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;

namespace Sts2ModExamples.Encounters;

/// <summary>
/// 示例遭遇：普通怪物房，只有 ExampleMonster。
/// 对照 skill：monster/monster-encounter.md（⚠️ 该文档 Slots 部分为虚构，原生无 Slots）。
/// 真实 API：RoomType abstract、GenerateMonsters() protected abstract 返回 (MonsterModel, string?) 列表。
/// </summary>
public class ExampleEncounter : EncounterModel
{
    public override RoomType RoomType => RoomType.Monster;

    public override IEnumerable<MonsterModel> AllPossibleMonsters =>
        new List<MonsterModel> { new Monsters.ExampleMonster() };

    protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        return new List<(MonsterModel, string?)>
        {
            (new Monsters.ExampleMonster(), null),   // 第二参：站位槽 ID（原生无 Slots 列表）
        };
    }
}
