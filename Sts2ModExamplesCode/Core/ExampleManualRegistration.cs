using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Saves.Managers;

namespace Sts2ModExamples.Core;

/// <summary>
/// 示例：角色手动注册 + 初始化时序（ModelDb 已初始化场景）。
/// 对照 skill：character/character-manual-register.md。
/// 真实 API：ModelDb.Inject(Type)；ModelDb.GetById&lt;AbstractModel&gt;(ModelId)；AbstractModel.InitId(ModelId)；
/// ModelDb 私有静态缓存字段（_allCards 等 8 个）反射重置；ProgressSaveManager 私有 epoch 方法。
/// </summary>
public static class ExampleManualRegistration
{
    // 全部模型集中清单：注册/兜底/测试三处共用
    public static readonly Type[] ModelTypes =
    [
        typeof(Pets.ExamplePersistentPet),
    ];

    /// <summary>ModelDb 已初始化时逐个兜底注入（未初始化则留给自动注册流程）。</summary>
    public static void EnsureRegisteredIfModelDbInitialized()
    {
        if (!ModelDb.Contains(typeof(Ironclad)))
        {
            return;
        }

        foreach (Type type in ModelTypes)
        {
            if (ModelDb.Contains(type))
            {
                continue;
            }

            ModelDb.Inject(type);
            ModelId id = ModelDb.GetId(type);
            ModelDb.GetById<AbstractModel>(id).InitId(id);
        }
    }

    /// <summary>ModelDb 静态缓存反射清空，下次访问重建（注册后才生效）。</summary>
    public static void ResetModelDbCaches()
    {
        string[] cacheFields =
        [
            "_allCards", "_allCardPools", "_allCharacterCardPools",
            "_allPotions", "_allPotionPools", "_allCharacterPotionPools",
            "_allRelics", "_allCharacterRelicPools",
        ];

        foreach (string fieldName in cacheFields)
        {
            FieldInfo? field = typeof(ModelDb).GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic);
            field?.SetValue(null, null);
        }
    }
}

/// <summary>
/// 示例：自定义角色解锁进度跳过（prefix 短路原版 epoch 检查）。
/// 真实 API：ProgressSaveManager.CheckFifteenElitesDefeatedEpoch(Player) 为 private instance 方法。
/// </summary>
[HarmonyPatch]
public static class ExampleProgressEpochPatch
{
    private static System.Reflection.MethodBase? TargetMethod()
    {
        return HarmonyLib.AccessTools.Method(
            typeof(ProgressSaveManager), "CheckFifteenElitesDefeatedEpoch", new[] { typeof(Player) });
    }

    private static bool Prefix(Player __0)
    {
        return __0.Character is not Characters.ExampleCharacter;  // 自定义角色跳过原版进度检查
    }
}
