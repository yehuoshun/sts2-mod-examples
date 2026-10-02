using Godot;
using System.Reflection;

namespace Sts2ModExamples.Settings;

/// <summary>
/// 配置声明 Attribute（自研，仿 BaseLib 灵感，零第三方依赖）。
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class ConfigSectionAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class ConfigSliderAttribute(double min = 0, double max = 100, double step = 1) : Attribute
{
    public double Min { get; } = min;
    public double Max { get; } = max;
    public double Step { get; } = step;
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class ConfigIgnoreAttribute : Attribute;

/// <summary>
/// 示例配置声明：静态属性 + Attribute 描述 UI。
/// </summary>
public static class ExampleModConfig
{
    [ConfigSection("通用")]
    public static bool EnableDeathEffect { get; set; } = true;

    [ConfigSection("通用")]
    [ConfigSlider(0.5, 4.0, 0.5)]
    public static double CursorScale { get; set; } = 2.0;

    [ConfigSection("战斗")]
    public static bool ShowDamageNumbers { get; set; } = true;
}

/// <summary>
/// 配置持久化：Godot ConfigFile → user://mod_configs/&lt;ModId&gt;/config.cfg。
/// 对照 skill：settings/settings-core.md（纯原生方案）。
/// </summary>
public static class ModConfigStorage
{
    private const string ConfigPath = "user://mod_configs/Sts2ModExamples/config.cfg";
    private const string SectionName = "mod_config";

    public static void Save()
    {
        var cf = new ConfigFile();
        foreach (var prop in typeof(ExampleModConfig).GetProperties())
        {
            if (prop.GetCustomAttribute<ConfigIgnoreAttribute>() != null) continue;
            cf.SetValue(SectionName, prop.Name, prop.GetValue(null));
        }
        cf.Save(ConfigPath);
    }

    public static void Load()
    {
        var cf = new ConfigFile();
        if (cf.Load(ConfigPath) != Error.Ok) return;   // 首次运行用默认值

        foreach (var prop in typeof(ExampleModConfig).GetProperties())
        {
            if (prop.GetCustomAttribute<ConfigIgnoreAttribute>() != null) continue;
            var value = cf.GetValue(SectionName, prop.Name, prop.GetValue(null));
            prop.SetValue(null, Convert.ChangeType(value, prop.PropertyType));
        }
    }
}
