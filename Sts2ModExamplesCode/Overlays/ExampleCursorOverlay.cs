using System;
using System.Collections.Generic;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.CommonUi;

namespace Sts2ModExamples.Overlays;

/// <summary>
/// 示例：游戏内 overlay 渲染模式（动态光标）。
/// 对照 skill：overlay/overlay.md + overlay/overlay-cursor.md。
/// 真实 API：NCursorManager.UpdateCursor 为 private instance 方法（反编译 sts2.dll 验证）。
/// ⚠️ 本示例仓库用 Godot.NET.Sdk 构建（源生成器运行，自定义 Node 子类回调正常）；
///    但标准单 DLL 发布布局（Microsoft.NET.Sdk）下源生成器不跑，自定义 Node 的
///    _Process/_Ready/_EnterTree 回调全部失效 —— 必须用本示例的「内置节点 + ProcessFrame
///    信号」方案，不依赖任何源生成器。
/// </summary>
[HarmonyPatch]
public static class ExampleCursorOverlay
{
    private static MethodBase? TargetMethod()
    {
        // private instance 无参方法：AccessTools.Method 可精确定位
        return AccessTools.Method(typeof(NCursorManager), "UpdateCursor");
    }

    // prefix 返回 false = 跳过原版方法体 → 游戏永远不会把系统光标刷回来
    private static bool Prefix()
    {
        return CursorOverlayController.BeforeVanillaUpdateCursor();
    }

    /// <summary>主入口调用（也可挂到 NGame._Ready / NCursorManager._EnterTree postfix）。</summary>
    public static void EnsureStarted(string source)
    {
        CursorOverlayController.EnsureStarted(source);
    }
}

/// <summary>
/// 光标 overlay 控制器：CanvasLayer(顶层) + Sprite2D 跟随鼠标，SceneTree.ProcessFrame 信号逐帧驱动。
/// 系统光标用 1x1 全透明图覆盖全部 17 种 CursorShape，防止任何控件露出系统指针。
/// </summary>
internal static class CursorOverlayController
{
    private const string AssetRoot = "res://Sts2ModExamples/cursors/default";
    private const string LayerNodeName = "Sts2ModExamplesCursorOverlay";
    private const int MaxFrameCount = 4096;
    private const double FrameDurationSeconds = 1.0 / 60.0;
    private static readonly Vector2 Hotspot = new(29f, 37f);

    private static Texture2D[] _frames = Array.Empty<Texture2D>();
    private static CanvasLayer? _layer;
    private static Sprite2D? _sprite;
    private static SceneTree? _connectedTree;
    private static Action? _processHandler;

    private static bool _framesLoaded;
    private static bool _transparentCursorApplied;
    private static double _frameAccumulator;
    private static int _frameIndex;
    private static ulong _lastTickMsec;

    // 引擎可请求的全部光标形态；全覆盖透明图保证屏幕上只有 overlay 光标
    private static readonly Input.CursorShape[] AllCursorShapes =
    {
        Input.CursorShape.Arrow, Input.CursorShape.Ibeam, Input.CursorShape.PointingHand,
        Input.CursorShape.Cross, Input.CursorShape.Wait, Input.CursorShape.Busy,
        Input.CursorShape.Drag, Input.CursorShape.CanDrop, Input.CursorShape.Forbidden,
        Input.CursorShape.Vsize, Input.CursorShape.Hsize, Input.CursorShape.Bdiagsize,
        Input.CursorShape.Fdiagsize, Input.CursorShape.Move, Input.CursorShape.Vsplit,
        Input.CursorShape.Hsplit, Input.CursorShape.Help,
    };

    public static bool BeforeVanillaUpdateCursor()
    {
        try
        {
            EnsureFramesLoaded();
            if (_sprite == null || !GodotObject.IsInstanceValid(_sprite))
            {
                return true; // 未就绪：放行原版逻辑
            }

            ApplyTransparentCursor();
            return false; // 抑制原版：游戏永远不刷新系统光标
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"[Overlay] suppress vanilla cursor failed: {ex.Message}");
            return true;
        }
    }

    public static void EnsureStarted(string source)
    {
        try
        {
            EnsureFramesLoaded();
            EnsureOverlay(source);
            ApplyTransparentCursor();
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"[Overlay] start failed from {source}: {ex.Message}");
        }
    }

    private static void EnsureFramesLoaded()
    {
        if (_framesLoaded)
        {
            return;
        }

        _frames = LoadFrameTextures(); // 无资源时返回空数组，降级为不启动动画
        _framesLoaded = true;
        if (_frames.Length > 0)
        {
            MainFile.Logger.Info($"[Overlay] loaded {_frames.Length} cursor frames");
        }
    }

    private static void EnsureOverlay(string source)
    {
        if (_frames.Length == 0 || IsHeadlessRun())
        {
            return;
        }

        if (Engine.GetMainLoop() is not SceneTree tree || tree.Root == null
            || !GodotObject.IsInstanceValid(tree.Root))
        {
            return;
        }

        // （重）建顶层 CanvasLayer + Sprite2D：只用内置节点，无需覆写回调
        if (_layer == null || !GodotObject.IsInstanceValid(_layer) || !_layer.IsInsideTree())
        {
            var layer = new CanvasLayer
            {
                Name = LayerNodeName,
                Layer = 4096,
                FollowViewportEnabled = false,
            };
            var sprite = new Sprite2D
            {
                Name = "Sts2ModExamplesCursorSprite",
                Centered = false,
                ZIndex = 4096,
                Visible = false,
                Texture = _frames[0],
            };
            layer.AddChild(sprite);

            try
            {
                tree.Root.AddChild(layer); // 树已就绪（postfix 时机）
            }
            catch (Exception)
            {
                tree.Root.CallDeferred(Node.MethodName.AddChild, layer); // 初始化期兜底
            }

            _layer = layer;
            _sprite = sprite;
            _frameIndex = 0;
            _frameAccumulator = 0.0;
            _lastTickMsec = 0;
        }

        // 逐帧驱动：SceneTree.ProcessFrame 信号 + 普通 Callable（不依赖源生成器）
        if (_connectedTree == null || !GodotObject.IsInstanceValid(_connectedTree))
        {
            _processHandler ??= OnProcessFrame;
            tree.ProcessFrame += _processHandler;
            _connectedTree = tree;
            MainFile.Logger.Info($"[Overlay] started from {source}");
        }
    }

    private static void OnProcessFrame()
    {
        if (_sprite == null || !GodotObject.IsInstanceValid(_sprite) || !_sprite.IsInsideTree())
        {
            return;
        }

        try
        {
            AdvanceFrame();
            UpdatePosition();
        }
        catch (Exception ex)
        {
            MainFile.Logger.Warn($"[Overlay] frame update failed: {ex.Message}");
        }
    }

    private static void AdvanceFrame()
    {
        ulong now = Time.GetTicksMsec();
        if (_lastTickMsec == 0)
        {
            _lastTickMsec = now;
        }

        double delta = Math.Clamp((now - _lastTickMsec) / 1000.0, 0.0, 0.25);
        _lastTickMsec = now;

        _frameAccumulator += delta;
        int steps = (int)(_frameAccumulator / FrameDurationSeconds);
        if (steps > 0)
        {
            _frameAccumulator -= steps * FrameDurationSeconds;
            _frameIndex = (_frameIndex + steps) % _frames.Length;
        }

        _sprite!.Texture = _frames[_frameIndex];
    }

    private static void UpdatePosition()
    {
        // 手柄模式下引擎隐藏系统光标，overlay 同步隐藏，避免悬浮在最后位置
        if (Input.MouseMode == Input.MouseModeEnum.Hidden)
        {
            _sprite!.Visible = false;
            return;
        }

        // GetGlobalMousePosition() 与 GlobalPosition 走同一 canvas transform，HiDPI 不自洽偏移
        Vector2 mousePosition = _sprite!.GetGlobalMousePosition();
        if (float.IsNaN(mousePosition.X) || float.IsNaN(mousePosition.Y)
            || float.IsInfinity(mousePosition.X) || float.IsInfinity(mousePosition.Y))
        {
            _sprite.Visible = false;
            return;
        }

        _sprite.GlobalPosition = mousePosition - Hotspot;
        _sprite.Visible = true;
    }

    private static void ApplyTransparentCursor()
    {
        if (_transparentCursorApplied || IsHeadlessRun())
        {
            return;
        }

        Image image = Image.CreateEmpty(1, 1, useMipmaps: false, Image.Format.Rgba8);
        image.Fill(new Color(0f, 0f, 0f, 0f));
        foreach (Input.CursorShape shape in AllCursorShapes)
        {
            Input.SetCustomMouseCursor(image, shape, Vector2.Zero);
        }

        _transparentCursorApplied = true;
    }

    private static bool IsHeadlessRun()
    {
        try
        {
            if (string.Equals(DisplayServer.GetName(), "headless", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        catch
        {
            // 显示服务器可能尚未初始化
        }

        foreach (string arg in OS.GetCmdlineArgs())
        {
            if (string.Equals(arg, "--headless", StringComparison.Ordinal)
                || arg.StartsWith("--headless=", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static Texture2D[] LoadFrameTextures()
    {
        var frames = new List<Texture2D>();
        for (int i = 0; i < MaxFrameCount; i++)
        {
            string path = $"{AssetRoot}_{i:00}.png";
            if (!Godot.FileAccess.FileExists(path) && !ResourceLoader.Exists(path))
            {
                break;
            }

            Texture2D? texture = ResourceLoader.Load<Texture2D>(path, cacheMode: ResourceLoader.CacheMode.Reuse);
            if (texture != null)
            {
                frames.Add(texture);
                continue;
            }

            Image? image = null;
            try
            {
                image = Image.LoadFromFile(path);
            }
            catch
            {
                image = null;
            }

            if (image != null)
            {
                frames.Add(ImageTexture.CreateFromImage(image));
            }
        }

        return frames.ToArray();
    }
}
