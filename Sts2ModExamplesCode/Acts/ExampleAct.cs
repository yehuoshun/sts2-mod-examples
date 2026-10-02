using Godot;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Unlocks;

namespace Sts2ModExamples.Acts;

/// <summary>
/// 示例章节（第 2 章替代品）。对照 skill：act/act-core.md。
/// 真实 API：ActModel 20 个抽象成员（skill 文档漏了 5 个：AllEvents/IsUnlocked/
/// GetUnlockedAncients/ApplyActDiscoveryOrderModifications/GetMapPointTypes）。
/// </summary>
public class ExampleAct : ActModel
{
    public override int Index => 1;              // 0=Act1, 1=Act2, 2=Act3
    public override bool IsDefault => false;

    public override string[] BgMusicOptions =>
        ["event:/music/act1_a1_v1", "event:/music/act1_a2_v1"];
    public override string[] MusicBankPaths =>
        ["res://banks/desktop/act1_a1.bank", "res://banks/desktop/act1_a2.bank"];
    public override string AmbientSfx => "";

    public override Color MapTraveledColor => new("27221C");
    public override Color MapUntraveledColor => new("6E7750");
    public override Color MapBgColor => new("9B9562");

    public override string ChestSpineSkinNameNormal => "";
    public override string ChestSpineSkinNameStroke => "";
    public override string ChestOpenSfx => "";

    public override IEnumerable<EncounterModel> BossDiscoveryOrder => [];

    protected override int BaseNumberOfRooms => 15;

    public override IEnumerable<AncientEventModel> AllAncients => [];
    public override IEnumerable<EventModel> AllEvents => [];

    public override IEnumerable<EncounterModel> GenerateAllEncounters() => [];

    public override bool IsUnlocked(UnlockState unlockState) => true;
    public override IEnumerable<AncientEventModel> GetUnlockedAncients(UnlockState state) =>
        AllAncients;
    protected override void ApplyActDiscoveryOrderModifications(UnlockState unlockState) { }

    public override MapPointTypeCounts GetMapPointTypes(Rng mapRng) =>
        new MapPointTypeCounts();
}
