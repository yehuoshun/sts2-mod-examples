using MegaCrit.Sts2.Core.Models;

namespace Sts2ModExamples.Energy;

/// <summary>
/// 自定义能量图标接口 + 编码器。对照 skill：energy/energy-custom-icon-core.md。
/// ⚠️ 游戏已有原生 EnergyIconHelper（GetPrefix/GetPath），编码辅助类改名 ModEnergyIconCodec 避免冲突。
/// </summary>
public interface ICustomEnergyIcon
{
    string? BigIconPath { get; }    // 大图标（卡牌左上角），null 走原生
    string? TextIconPath { get; }   // 文本内联图标，null 走原生
}

public static class ModEnergyIconCodec
{
    public const char Delimiter = '∴';

    public static string EncodePoolId(ModelId id) =>
        $"{id.Category}{Delimiter}{id.Entry}";

    public static T? DecodePool<T>(string prefix) where T : AbstractModel
    {
        int idx = prefix.IndexOf(Delimiter);
        if (idx < 0) return null;
        return ModelDb.GetById<T>(new ModelId(
            prefix[..idx], prefix[(idx + 1)..]));
    }
}
