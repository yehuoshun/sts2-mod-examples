using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace Sts2ModExamples.Characters;

/// <summary>
/// 示例角色。对照 skill：character/character-core.md。
/// 真实 API：CharacterModel 12 个抽象成员（NameColor/Gender/UnlocksAfterRunAs/StartingHp/StartingGold/
/// CardPool/RelicPool/PotionPool/StartingDeck/StartingRelics/AttackAnimDelay/CastAnimDelay/GetArchitectAttackVfx）。
/// ⚠️ 无 CharacterId 属性（YuWan 自研）——ID 由类名生成；Title 走本地化 characters 表。
/// 正式角色应建自定义池（CardPoolModel 子类），这里复用原生池简化。
/// </summary>
public class ExampleCharacter : CharacterModel
{
    public override Color NameColor => new Color("F5C48C");
    public override CharacterGender Gender => CharacterGender.Neutral;
    protected override CharacterModel? UnlocksAfterRunAs => null;
    public override int StartingHp => 70;
    public override int StartingGold => 99;
    public override float AttackAnimDelay => 0.15f;
    public override float CastAnimDelay => 0.25f;

    public override CardPoolModel CardPool => ModelDb.CardPool<ColorlessCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<SharedRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<SharedPotionPool>();

    public override IEnumerable<CardModel> StartingDeck =>
        new List<CardModel>
        {
            ModelDb.Card<Cards.ExampleStrike>(),
            ModelDb.Card<Cards.ExampleStrike>(),
            ModelDb.Card<Cards.ExampleDefend>(),
        };

    public override IReadOnlyList<RelicModel> StartingRelics =>
        new List<RelicModel> { ModelDb.Relic<Relics.ExampleRelic>() };

    public override List<string> GetArchitectAttackVfx() => new();
}
