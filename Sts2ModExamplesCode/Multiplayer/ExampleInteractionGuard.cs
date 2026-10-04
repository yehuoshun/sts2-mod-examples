using MegaCrit.Sts2.Core.Models;

namespace Sts2ModExamples.Multiplayer;

/// <summary>
/// 示例：交互执行前后检查模型是否仍在 state（防对已移除模型重复执行 / 误触发结束回调）。
/// 对照 skill：multiplayer/multiplayer-netactions.md 第 5 节。
/// 真实 API：CardModel/RelicModel/PotionModel.HasBeenRemovedFromState；PowerModel 无此属性 → 用 Owner.Powers 判。
/// </summary>
public static class ExampleInteractionGuard
{
    public static bool IsModelStillInState(AbstractModel model)
    {
        try
        {
            return model switch
            {
                CardModel card => !card.HasBeenRemovedFromState,
                RelicModel relic => !relic.HasBeenRemovedFromState && relic.Owner.Relics.Contains(relic),
                PowerModel power => power.Owner.Powers.Contains(power),
                PotionModel potion => !potion.HasBeenRemovedFromState && potion.Owner.Potions.Contains(potion),
                _ => true
            };
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"state check failed: {ex}");
            return false;
        }
    }
}
