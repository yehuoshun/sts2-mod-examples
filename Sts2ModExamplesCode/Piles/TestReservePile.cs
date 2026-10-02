using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using Sts2ModExamples.Core;

namespace Sts2ModExamples.Piles;

/// <summary>
/// 【skill 全面测试】测试自定义牌堆：储备堆（ReservePile）。
/// 对照 skill：pile/pile-base.md + pile/pile-inject.md。
/// ⚠️ 真实 API：原生 CardPile 只有 Type/Cards/IsEmpty/事件等成员（CardShouldBeVisible/GetTargetPosition/
/// CustomTween/IconPath/Name 是 BaseLib CustomPile 的，原生不存在——pile-base.md 模板代码有误，待修）。
/// </summary>
public class TestReservePile : CardPile
{
    public TestReservePile() : base(TestPileTypes.ReservePile)
    {
    }

    // 可选：牌堆显示名（PileType 扩展，非 CardPile 成员）
    public LocString DisplayName =>
        new LocString("gameplay_ui", "RESERVE_PILE");
}

/// <summary>自定义 PileType 静态字段（[CustomPileType] 标记，EnumInjector 自动注入）</summary>
public static class TestPileTypes
{
    [EnumInjector.CustomPileType]
    public static PileType ReservePile;
}
