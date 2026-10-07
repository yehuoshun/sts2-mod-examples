using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Saves.Runs;
using Sts2ModExamples.Core;

namespace Sts2ModExamples;

/// <summary>
/// 模组入口。三阶段初始化：Harmony 补丁 → 内容注册 → 收尾。
/// 对照 skill 文档：setup/skeleton-build.md（生产级入口）。
/// </summary>
[ModInitializer(nameof(Initialize))]
public static class MainFile
{
    public const string ModId = "Sts2ModExamples";

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } =
        new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    public static void Initialize()
    {
        // Phase 1: Harmony 补丁（PatchAllSafe：逐类 try-catch，坏类型只跳过不中断）
        ModPatcher.PatchAllSafe(new Harmony(ModId), Assembly.GetExecutingAssembly());
        Logger.Info("[Phase 1] Harmony patches applied");

        // Phase 2: 内容注册（[CardPool]/[RelicPool]/[PotionPool] Attribute 扫描）
        ContentRegistry.RegisterAll(Assembly.GetExecutingAssembly());
        ContentRegistry.Freeze();
        Logger.Info($"[Phase 2] Content registered: {ContentRegistry.RegisteredCount} models");

        // 设置：加载持久化配置（缺失时用默认值）
        Settings.ModConfigStorage.Load();

        // 设置页「Mods」标签注入（纯原生转译 ModConfig 机制，零 Harmony）
        Settings.ModsTabInjector.Initialize();

        // 序列化类型注册（[SavedProperty] 类型必须，漏了读档丢数据）
        SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(Modifiers.ExampleModifier));
        SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(Cards.ExampleReplayCard));

        // 多人消息处理器（单人/未联网时内部跳过）
        Multiplayer.ExampleMessageHandler.Register();

        // Phase 3: 收尾（本项目无设置界面；有的话在这里注册）
        Logger.Info("Sts2ModExamples initialized");
    }
}
