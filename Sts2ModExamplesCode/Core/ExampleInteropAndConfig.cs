using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;

namespace Sts2ModExamples.Core;

/// <summary>
/// 示例：公共 Interop（只读 + 全兜底返回 0）+ JSON 配置（位标志同步兜底）。
/// 对照 skill：run/run-interop-config.md。
/// 真实 API：RunManager.Instance.DebugOnlyGetState()；ProjectSettings.GlobalizePath("user://")。
/// </summary>
public static class ExampleInterop
{
    /// <summary>已完成轮次。无 run/异常/未进入 → 0，绝不抛出（调用方无需 try-catch）。</summary>
    public static int GetCompletedLoopCount()
    {
        try
        {
            if (RunManager.Instance?.DebugOnlyGetState() is not RunState state)
            {
                return 0;
            }

            return state.CurrentActIndex;   // 示例：用现有状态举例（真实项目这里是 mod 内部计数）
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"ExampleInterop failed: {ex.Message}");
            return 0;
        }
    }
}

/// <summary>示例配置：user:// 下 JSON + snake_case。</summary>
internal sealed class ExampleInteropConfig
{
    private const string ModId = "Sts2ModExamples";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
    };

    private static ExampleInteropConfig _current = LoadOrCreate();

    [JsonPropertyName("example_bool")]
    public bool ExampleBool { get; set; } = true;

    [JsonPropertyName("example_percent")]
    public int ExamplePercent { get; set; } = 50;

    internal static ExampleInteropConfig Current => _current;

    internal static string GetConfigPath()
        => Path.Combine(ProjectSettings.GlobalizePath("user://"), ModId, "config.json");

    private static ExampleInteropConfig LoadOrCreate()
    {
        string path = GetConfigPath();
        if (!File.Exists(path))
        {
            var config = new ExampleInteropConfig();
            WriteConfig(path, config);
            return config;
        }

        try
        {
            ExampleInteropConfig? config = JsonSerializer.Deserialize<ExampleInteropConfig>(File.ReadAllText(path), JsonOptions);
            if (config != null)
            {
                config.ExamplePercent = Math.Clamp(config.ExamplePercent, 0, 300);
                WriteConfig(path, config);
                return config;
            }
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"Failed to load config; rewriting defaults: {ex.Message}");
        }

        var defaults = new ExampleInteropConfig();
        WriteConfig(path, defaults);
        return defaults;
    }

    // 静态构造路径上的 IO 异常若外泄会变 TypeInitializationException 毒化整个类型 → 只告警不抛
    private static void WriteConfig(string path, ExampleInteropConfig config)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, JsonSerializer.Serialize(config, JsonOptions));
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"Failed to write config {path}: {ex.Message}");
        }
    }
}

/// <summary>位标志配置示例：多人同步不可用时两端必须一致 → 只用编译期默认（全开）。</summary>
internal static class ExampleFlagConfig
{
    private enum OptionalReward { A, B, C }

    private static int GetDefaultEnabledRewardFlags()
    {
        int flags = 0;
        foreach (OptionalReward reward in Enum.GetValues<OptionalReward>())
        {
            flags |= 1 << (int)reward;
        }

        return flags;
    }

    private static bool IsRewardEnabled(int enabledFlags, OptionalReward reward)
    {
        return (enabledFlags & (1 << (int)reward)) != 0;
    }
}