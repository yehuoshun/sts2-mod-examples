using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace Sts2ModExamples.Characters;

/// <summary>
/// 【skill 全面测试】测试角色：圣骑士（高生命，低金币）。
/// 对照 skill：character/character-core.md。
/// ⚠️ 真实 API：CharacterModel 12 个抽象成员；无 CharacterId 属性（ID 由类名生成）；
/// Title 走本地化 characters 表。
/// </summary>
public class TestPaladinCharacter : CharacterModel
{
    public override Color NameColor => new Color("C8D8F5");
    public override CharacterGender Gender => CharacterGender.Neutral;
    protected override CharacterModel? UnlocksAfterRunAs => null;
    public override int StartingHp => 80;
    public override int StartingGold => 50;
    public override float AttackAnimDelay => 0.2f;
    public override float CastAnimDelay => 0.3f;

    public override CardPoolModel CardPool => ModelDb.CardPool<ColorlessCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<SharedRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<SharedPotionPool>();

    public override IEnumerable<CardModel> StartingDeck =>
        new List<CardModel>
        {
            ModelDb.Card<Cards.ExampleStrike>(),
            ModelDb.Card<Cards.ExampleStrike>(),
            ModelDb.Card<Cards.ExampleDefend>(),
            ModelDb.Card<Cards.ExampleDefend>(),
        };

    public override IReadOnlyList<RelicModel> StartingRelics =>
        new List<RelicModel> { ModelDb.Relic<Relics.ExampleRelic>() };

    public override List<string> GetArchitectAttackVfx() => new();
}
