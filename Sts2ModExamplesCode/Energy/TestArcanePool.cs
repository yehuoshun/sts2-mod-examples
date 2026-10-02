using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Sts2ModExamples.Energy;

/// <summary>
/// 【skill 全面测试】测试自定义能量图标卡池（奥术池，紫色调）。
/// 对照 skill：energy/energy-custom-icon-core.md。
/// 真实 API：CardPoolModel.EnergyColorName 是 abstract（必须 override）；
/// 池模型不标 [CardPool]（那是卡牌的 attribute）。
/// </summary>
public class TestArcanePool : CardPoolModel, ICustomEnergyIcon
{
    public override string Title => "奥术池";
    public override string EnergyColorName =>
        ModEnergyIconCodec.EncodePoolId(Id);
    public override string CardFrameMaterialPath => "";
    public override Color DeckEntryCardColor => new("9B30FF");
    public override bool IsColorless => false;

    protected override CardModel[] GenerateAllCards() => [];

    public string? BigIconPath =>
        "res://Sts2ModExamples/images/energy/arcane_big.png";

    public string? TextIconPath =>
        "res://Sts2ModExamples/images/energy/arcane_text.png";
}
