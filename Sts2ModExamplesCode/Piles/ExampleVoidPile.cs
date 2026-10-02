using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;

using Sts2ModExamples.Core;

namespace Sts2ModExamples.Piles;

/// <summary>
/// 示例自定义牌堆（虚空堆）：纯原生继承 CardPile + 注入的 PileType。
/// 对照 skill：pile/pile-base.md + pile/pile-inject.md。
/// ⚠️ 真实 API：原生 CardPile 只有 Type/Cards/IsEmpty/事件等成员，
/// CardShouldBeVisible/GetTargetPosition/GetNCard/CustomTween 等是 BaseLib CustomPile 的（原生无，旧文档编造）。
/// 纯原生自定义牌堆 = 自定义 PileType + new CardPile(type) + 内容管理走 CardPileCmd。
/// </summary>
public class ExampleVoidPile : CardPile
{
    public ExampleVoidPile() : base(ExamplePileTypes.VoidPile)
    {
    }

    // 可选：牌堆显示名（PileType 扩展，非 CardPile 成员）
    public LocString DisplayName =>
        new LocString("gameplay_ui", "VOID_PILE");
}
