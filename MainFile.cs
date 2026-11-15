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

        // 序列化类型注册（[SavedProperty]）：0.113 起游戏自己扫 mod 类型，无需手动注入。
        // 旧版（≤0.107）必须逐个 SavedPropertiesTypeCache.InjectTypeIntoCache(typeof(X))，漏一个读档就丢数据；
        // 0.113 把该缓存并入 ModelIdSerializationCache.CachePropertiesForType(type, ...) 且转为私有，mod 侧没有公开入口。
        // 本项目带 [SavedProperty] 的类型：Modifiers.ExampleModifier、Cards.ExampleReplayCard、Cards.TestStoredPowerCard、
        // Pets.ExamplePersistentPet、Enchantments.ExampleCompositeEnchantment。

        // 多人消息处理器（单人/未联网时内部跳过）
        Multiplayer.ExampleMessageHandler.Register();

        // Overlay 示例：动态光标（单 DLL 布局下自定义 Node 回调失效 → 内置节点 + ProcessFrame 信号方案）
        Overlays.ExampleCursorOverlay.EnsureStarted("MainFile.Initialize");

        // Phase 3: 收尾（本项目无设置界面；有的话在这里注册）
        Logger.Info("Sts2ModExamples initialized");
    }
}
