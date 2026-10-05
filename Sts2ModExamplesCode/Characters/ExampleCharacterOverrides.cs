using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Saves.Managers;

namespace Sts2ModExamples.Characters;

/// <summary>
/// 示例：角色资源覆写点 + 解锁进度屏蔽。对照 skill：character/character-overrides.md + monster/monster-animator.md。
/// 纯原生 CharacterModel 只有 2 个路径覆写点（CharacterTransitionSfx / EnergyLabelOutlineColor）；
/// 动画状态机用 GenerateAnimator（原生虚方法）；解锁屏蔽 = 3 个 ProgressSaveManager Prefix。
/// </summary>
public class ExampleOverridesCharacter : CharacterModel
{
    public override Color NameColor => new Color("C8D8F5");
    public override CharacterGender Gender => CharacterGender.Feminine;
    protected override CharacterModel? UnlocksAfterRunAs => null;
    public override int StartingHp => 75;
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

    // ── 原生覆写点 ──
    public override string CharacterTransitionSfx => "event:/sfx/ui/wipe_ironclad";
    public override Color EnergyLabelOutlineColor => new Color("000A7D");

    // 动画状态机：动画名必须匹配 Spine 骨架实际动画
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        var idle = new AnimState("idle_loop", isLooping: true);
        var attack = new AnimState("attack");
        var cast = new AnimState("cast");
        var hurt = new AnimState("hurt");
        var die = new AnimState("die");
        attack.NextState = idle;
        cast.NextState = idle;
        hurt.NextState = idle;

        var animator = new CreatureAnimator(idle, controller);
        animator.AddAnyState("Idle", idle);
        animator.AddAnyState("Attack", attack);
        animator.AddAnyState("Cast", cast);
        animator.AddAnyState("Hit", hurt);
        animator.AddAnyState("Dead", die);
        return animator;
    }
}

/// <summary>解锁进度屏蔽：自定义角色不计入全局 15 精英/15 Boss/角色解锁进度。</summary>
[HarmonyPatch(typeof(ProgressSaveManager), "ObtainCharUnlockEpoch")]
public static class UnlockEpochPatch
{
    private static bool Prefix(ProgressSaveManager __instance, Player localPlayer)
        => localPlayer.Character is not ExampleOverridesCharacter;
}

[HarmonyPatch(typeof(ProgressSaveManager), "CheckFifteenElitesDefeatedEpoch")]
public static class FifteenElitesPatch
{
    private static bool Prefix(ProgressSaveManager __instance, Player localPlayer)
        => localPlayer.Character is not ExampleOverridesCharacter;
}

[HarmonyPatch(typeof(ProgressSaveManager), "CheckFifteenBossesDefeatedEpoch")]
public static class FifteenBossesPatch
{
    private static bool Prefix(ProgressSaveManager __instance, Player localPlayer)
        => !localPlayer.Character.Id.ToString().Contains("Example", StringComparison.OrdinalIgnoreCase);
}
