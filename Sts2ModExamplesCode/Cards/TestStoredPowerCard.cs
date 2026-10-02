using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using Sts2ModExamples.Core;

namespace Sts2ModExamples.Cards;

/// <summary>
/// 【skill 全面测试】测试序列化：带 [SavedProperty] 的卡牌（层数跨战斗/存档保存）。
/// 对照 skill：serialization/serialization.md + serialization-register.md。
/// 真实 API：[SavedProperty]（MegaCrit.Sts2.Core.Saves.Runs）标记属性，
/// 注册必须调 SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(T))（硬规则 5）。
/// </summary>
[CardPool(typeof(MegaCrit.Sts2.Core.Models.CardPools.ColorlessCardPool))]
public class TestStoredPowerCard : CardModel
{
    private int _storedStacks;

    public TestStoredPowerCard() : base(
        1,
        type: CardType.Skill,
        rarity: CardRarity.Uncommon,
        targetType: TargetType.Self,
        shouldShowInCardLibrary: true)
    {
    }

    // 存档属性：层数跨存档保存（硬规则 5：必须 InjectTypeIntoCache）
    [SavedProperty]
    public int StoredStacks
    {
        get => _storedStacks;
        set
        {
            AssertMutable();
            _storedStacks = value;
        }
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        StoredStacks++;
    }

    protected override void OnUpgrade()
    {
        base.OnUpgrade();
        DynamicVars.Damage.UpgradeValueBy(1m);
    }
}
