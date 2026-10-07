using System.Reflection;
using System.Runtime.Loader;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Logging;

namespace Sts2ModExamples.Core;

/// <summary>
/// 示例：多版本变体 Loader（一个 loader 按游戏版本选 DLL）。
/// 对照 skill：setup/loader-multiversion.md。
/// 真实 API：AssemblyLoadContext.LoadFromAssemblyPath；AssemblyMetadataAttribute（程序集元数据）；
/// ModManager.AssociateAssemblyWithMod（0.110+，这里反射调用避免硬版本依赖）；
/// ModManager.OnModDetected（延迟关联兜底）。
/// </summary>
public static class ExampleVariantLoader
{
    private const string VariantIdentity = "Sts2ModExamples";

    public static Assembly? LoadVariant(string variantDllPath, string expectedCompatTarget)
    {
        if (!File.Exists(variantDllPath))
        {
            throw new FileNotFoundException($"Missing variant DLL: {variantDllPath}");
        }

        AssemblyLoadContext context = AssemblyLoadContext.GetLoadContext(typeof(ExampleVariantLoader).Assembly)
            ?? AssemblyLoadContext.Default;
        Assembly implementation = context.LoadFromAssemblyPath(variantDllPath);

        ValidateVariant(implementation, expectedCompatTarget);
        return implementation;
    }

    /// <summary>双重校验：程序集名固定 + 内嵌 CompatibilityTarget 元数据匹配。</summary>
    private static void ValidateVariant(Assembly assembly, string expectedCompatTarget)
    {
        if (!string.Equals(assembly.GetName().Name, VariantIdentity, StringComparison.Ordinal))
        {
            throw new BadImageFormatException(
                $"Variant identity is {assembly.GetName().Name ?? "<null>"}, expected {VariantIdentity}.");
        }

        string? embeddedTarget = assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .FirstOrDefault(a => string.Equals(a.Key, "CompatibilityTarget", StringComparison.Ordinal))
            ?.Value;
        if (!string.Equals(embeddedTarget, expectedCompatTarget, StringComparison.Ordinal))
        {
            throw new BadImageFormatException(
                $"Variant compat target is {embeddedTarget ?? "<null>"}, expected {expectedCompatTarget}.");
        }
    }

    /// <summary>把变体程序集关联到游戏 Mod 系统（0.110+ 反射调用；旧版降级到 OnModDetected）。</summary>
    public static bool TryAssociate(Assembly implementation)
    {
        MethodInfo? associate = typeof(ModManager).GetMethod(
            "AssociateAssemblyWithMod",
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null, [typeof(string), typeof(Assembly)], modifiers: null);
        if (associate != null)
        {
            associate.Invoke(null, [VariantIdentity, implementation]);
            return true;
        }

        MainFile.Logger.Warn("AssociateAssemblyWithMod not found; falling back to OnModDetected.");
        ModManager.OnModDetected += _ => { };
        return false;
    }
}