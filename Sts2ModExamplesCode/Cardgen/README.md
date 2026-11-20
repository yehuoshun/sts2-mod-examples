# Cardgen（AutoAnthony 生态参考）

AutoAnthony 是第三方工坊 mod，**不在本项目的 CI 依赖里**（CI 只下载 `sts2.dll` + `0Harmony.dll`）。所以这个模块：

- 装了 AutoAnthony 的机器上 → **参与编译**，编译期锁死卡片编辑/组件注册 API 的每个签名
- 没装的机器 / CI → csproj 里 `Compile Remove` 掉整个目录，构建照常通过

这就是 cardgen 的验证方式：`dotnet build` 能过 = 文档里那批签名和真 DLL 一致。文档改错参数类型、漏掉重载、写错默认值，这里就编译不过。

## 用法

```powershell
# AutoAnthony 工坊目录默认从 Steam 注册表推；也可显式指定
dotnet build Sts2ModExamples.csproj -v m -p:AutoAnthonyDir="E:/SteamLibrary/steamapps/workshop/content/2868840/3786611028"
```

构建日志里会有：

- `AutoAnthony.dll 已找到 → WitCardgen 示例参与编译`（条件成立）
- `AutoAnthony.dll 未找到 → 跳过 Cardgen 编译`（CI / 未装 mod）

## 文件

| 文件 | 覆盖的 API 面 |
|---|---|
| `CardgenTinkeringFlow.cs` | `CardTinkeringApi` v6 全流程（原型→改槽→校验→重建→序列化往返→预算分析→关键词原型）+ `AutoAnthonyEditorApi` v2 身份重掷 |
| `CardgenPackageRegistration.cs` | `ComponentApi`/`ComponentPackageApi` v3、`ComponentRuntimeApi` v1、`ComponentPresentationApi` v2、`ComponentKeywordRuntimeApi` v1、`ExternalComponentCharacterApi` v4、`NativeCardDecompositionApi` v1、`AutoAnthonyNativeCardApi` v3、`AutoAnthonyFreeformCardApi` v2、`AutoAnthonySettingsApi` v5、`AutoAnthonyBalanceAdjustmentApi` v1 |

## 运行

`CardgenTinkeringFlow.Run(seed)` 返回一份可读报告，不需要战斗上下文。示例 mod 默认不调用它——把调用挂到自己的调试入口即可（AutoAnthony 未加载时它自己会返回跳过说明）。

注册那一套**必须**在首次 profile/卡查询之前完成，且首用即冻结：`CardgenPackageRegistration.RegisterAll()` 只该在自己的初始化阶段调一次。