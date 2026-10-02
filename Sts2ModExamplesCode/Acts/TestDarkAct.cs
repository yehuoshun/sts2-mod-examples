using Godot;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Unlocks;

namespace Sts2ModExamples.Acts;

/// <summary>
/// 【skill 全面测试】测试章节：第 3 章替代品（暗色调）。
/// 对照 skill：act/act-core.md。
/// ⚠️ 真实 API：ActModel 20 个抽象成员全要 override（含 AllEvents/IsUnlocked/
/// GetUnlockedAncients/ApplyActDiscoveryOrderModifications/GetMapPointTypes）。
/// </summary>
public class TestDarkAct : ActModel
{
    public override int Index => 2;              // 0=Act1, 1=Act2, 2=Act3
    public override bool IsDefault => false;

    public override string[] BgMusicOptions =>
        ["event:/music/act3_a1_v1", "event:/music/act3_a2_v1"];
    public override string[] MusicBankPaths =>
        ["res://banks/desktop/act3_a1.bank", "res://banks/desktop/act3_a2.bank"];
    public override string AmbientSfx => "";

    public override Color MapTraveledColor => new("3A2A4A");
    public override Color MapUntraveledColor => new("7A6A9A");
    public override Color MapBgColor => new("1A1020");

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
        new MapPointTypeCounts(0, 0);
}
