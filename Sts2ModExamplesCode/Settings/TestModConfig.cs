using Godot;

namespace Sts2ModExamples.Settings;

/// <summary>
/// 【skill 全面测试】测试配置声明：静态属性 + Attribute 描述 UI（纯原生方案）。
/// 对照 skill：settings/settings-core.md + settings-attributes.md。
/// 真实 API：自研 [ConfigSection]/[ConfigSlider]/[ConfigIgnore] Attribute（仿 BaseLib 灵感，零第三方依赖），
/// 由 ModConfigStorage（ConfigFile → user://mod_configs/&lt;ModId&gt;/config.cfg）持久化。
/// </summary>
public static class TestModConfig
{
    [ConfigSection("显示")]
    public static bool ShowEnemyIntentNumbers { get; set; } = true;

    [ConfigSection("显示")]
    [ConfigSlider(0.5, 2.0, 0.1)]
    public static double UiScale { get; set; } = 1.0;

    [ConfigSection("战斗")]
    public static bool AutoEndTurn { get; set; } = false;

    [ConfigSection("调试")]
    [ConfigIgnore]
    public static string LastSessionId { get; set; } = "";   // 忽略：不生成 UI、不入配置
}
