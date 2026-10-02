using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;

namespace Sts2ModExamples.Energy;

/// <summary>
/// 示例自定义能量图标卡池（能源池）。
/// 对照 skill：energy/energy-custom-icon-core.md。
/// 真实 API：CardPoolModel.EnergyColorName 是 abstract（必须 override）。
/// ⚠️ 池模型不标 [CardPool]（那是卡牌的 attribute）；池由角色 CardPool 属性引用或手动注册。
/// </summary>
public class ExampleEnergyPool : CardPoolModel, ICustomEnergyIcon
{
    public override string Title => "能源池";
    public override string EnergyColorName =>
        ModEnergyIconCodec.EncodePoolId(Id);
    public override string CardFrameMaterialPath => "";
    public override Color DeckEntryCardColor => new("FFD700");
    public override bool IsColorless => false;

    protected override CardModel[] GenerateAllCards() => [];

    public string? BigIconPath =>
        "res://Sts2ModExamples/images/energy/energy_big.png";

    public string? TextIconPath =>
        "res://Sts2ModExamples/images/energy/energy_text.png";
}
