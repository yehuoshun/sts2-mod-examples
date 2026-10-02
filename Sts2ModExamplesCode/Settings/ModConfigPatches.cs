using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;

namespace Sts2ModExamples.Settings;

/// <summary>把 NModConfigSubmenu 注册进主菜单子菜单栈（GetSubmenuType 拦截）。</summary>
[HarmonyPatch(typeof(NMainMenuSubmenuStack), nameof(NMainMenuSubmenuStack.GetSubmenuType), typeof(Type))]
public static class InjectModConfigSubmenuPatch
{
    public static bool Prefix(NMainMenuSubmenuStack __instance, Type type, ref NSubmenu __result)
    {
        if (type != typeof(NModConfigSubmenu)) return true;

        var menu = new NModConfigSubmenu { Visible = false };
        __instance.AddChild(menu);
        __result = menu;
        return false;   // 跳过原生查找
    }
}

/// <summary>主菜单加「Mod Settings」按钮（复制 Settings 按钮，改本地化键）。</summary>
[HarmonyPatch(typeof(NMainMenu), nameof(NMainMenu._Ready))]
public static class InjectMainMenuModConfigButtonPatch
{
    public static void Postfix(NMainMenu __instance)
    {
        var settingsButton = __instance.GetNodeOrNull<NMainMenuTextButton>(
            "MainMenuTextButtons/SettingsButton");
        if (settingsButton == null) return;

        var modButton = (NMainMenuTextButton)settingsButton.Duplicate();
        modButton.Name = "ModConfigButton";
        modButton.Connect(NClickableControl.SignalName.Released, Callable.From(
            new Action<NButton>(_ =>
                __instance.SubmenuStack.PushSubmenuType<NModConfigSubmenu>())));
        settingsButton.AddSibling(modButton);
        modButton.SetLocalization("STS2MODEXAMPLES-MOD_CONFIGURATION");
    }
}
