# AESX 0.94.0：基于 AES 0.40.0 的重构说明

## 做了什么

把 AESX 的 UI 改动整体搬到 **AES 0.40.0** 上，不再维护 0.35.0 的旧分支。

AESX 相对 AES 0.35.0 的改动只有两处，所以重做成本很低：

1. `UiTheme.cs`：AESX 的 `Studio 深灰` 配色（现在叫 **NEXT**）；
2. `EditorStudioOverlay.cs`：共用按钮的 `targetGraphic` 改为白色基底，由 `ColorBlock` 统一着色。

落到 AES 0.40.0 上：

- 新增 `UiTheme.Studio`（Id `next`，显示名 `NEXT`），作为默认主题；
- 原 AES 0.40.0 的 `Mono` 保留为 `UiTheme.Native`（Id `native`，显示名 `原生`），可在设置 → 常规设置 → 界面主题切换；
- `ById("mono")` / `ById("slate")` 回落到 NEXT，`ById("ember")` 回落到原生，旧 `Settings.xml` 不需要改；
- `targetGraphic` 白色基底应用到 0.40.0 的 `ApplyButtonColors`；
- 全局圆角：`CornerLarge = 12`、`CornerSmall = 6`，并给按钮、弹窗、输入框、行、开关补上圆角；
- 字体优先 `Microsoft YaHei UI Light`（失败时回退微软雅黑 / 思源黑体），字号刻度改为 12 / 14 / 16 / 19 / 22 / 28；
- 版本说明页顶部加入 `AesxLogo.png`，窗口标题改为「版本说明」，修掉原来两条 `v0.94.0 更新内容` 叠在一起的问题；
- 顶栏品牌、帮助窗口、日志统一改为 `ADOFAI Editor Studio X`；
- 开放联机做谱：顶栏「联机」按钮打开主机/加入面板；新增 UDP 21588 局域网房间发现，TCP 21587 传谱面/增量/媒体；异地由主机映射 TCP 端口后手填 `公网IP:端口`，无服务器、无账号、无密码；
- 修复新建事件块掉到最底行：真·新事件不再 `++maxRow`，改为贴在同一时间段重叠块的下方，没重叠就落第 0 行。

AES 0.40.0 自带的传送带翻页、设置窗淡入淡出、参数栏淡入、删除粒子、拖动拖尾、滚动惯性等全部原生保留，不再需要从 0.35 回移。

## 版本与名称

| 项目 | 值 |
|---|---|
| 显示名 | `ADOFAI Editor Studio X` |
| Mod Id | `ADOFAIEditorStudioX` |
| 程序集 | `ADOFAIEditorStudio.dll`（保持不变，兼容 UMM 与旧安装） |
| 版本 | `0.94.0` |
| 作者 | `NIANNA` |

`Info.json` 与 `.csproj` 的版本都是 `0.94.0`；`build-mod.ps1` 会强制校验两者一致，不一致直接失败。

## 打包/构建脚本的修正

AESX 原来的 `build-mod.ps1` 有几个问题，这次一起重写：

1. **版本硬编码**：原来 zip 名写死 `0.35.0`，升版本会忘改。现在从 `Info.json` 读版本，并校验 `.csproj`。
2. **输出目录名错了**：原来输出 `dist\ADOFAIEditorStudio`，和 UMM 的 `Mods\ADOFAIEditorStudioX` 不一致。现在输出 `dist\ADOFAIEditorStudioX\`，zip 里也带这个文件夹。
3. **依赖清单缺文件**：AES 0.40.0 需要 `UnityEngine.SpriteMaskModule.dll`，已补进检查列表。
4. **PowerShell 5.1 编码问题**：`build-mod.ps1` 含中文注释，原来是无 BOM 的 UTF-8，Windows PowerShell 5.1 会按 ANSI 读，导致变量被吞、`Info.json` 读成 null。已改为 **UTF-8 with BOM**。
5. **批处理提示**：`双击运行-自动构建.bat` 改为 ASCII 提示，避免 cmd 编码问题。
6. **自动更新**：关闭上游 AES 的自动更新清单（`VersionGate.Check` 已注释），避免某次上游抬 `min` 时把 AESX 覆盖成官方版。以后接 AESX 自己的 `version.json` 再启用。
7. **设置窗口重建崩溃**：`settingsNavFills` / `settingsNavLabels` 没在重建前清空，换分辨率/换主题后旧 Image/TMP 已销毁，`SelectSettingsPage` 摸到死引用抛 `NullReferenceException`。现在重建前清空，并按较小表长遍历。

## 构建

```powershell
cd ADOFAIEditorStudioX
.\build-mod.ps1 -GameDir "E:\SteamLibrary\steamapps\common\A Dance of Fire and Ice"
```

输出：

- `dist\ADOFAIEditorStudioX\`：可直接放进 `Mods` 的文件夹
- `dist\ADOFAIEditorStudioX-v0.94.0.zip`：同内容的压缩包

本机已实际运行脚本：**0 个错误**，生成 882,688 B 的 DLL 和约 663 KB 的 mod 包（含 logo）。

## 验证状态

- 源码编译：通过（0 错误）。
- 打包脚本：连续运行 3 次均成功，输出稳定。
- 游戏内：本机 Steam 当前离线/报错，无法稳定进入游戏实测；已把 0.94.0 DLL + Info + logo 临时装进 `Mods\ADOFAIEditorStudioX` 供直接验证。
- 之前的一次游戏日志里抓到并修复了「设置窗口建不出来」的 `NullReferenceException`；修复后的日志不再出现该异常。23:38 那次运行已经加载 NEXT 主题且没有设置窗口崩溃。
- 字体改为 Microsoft YaHei UI Light、字号上调、版本页 logo、双标题修复这一版编译通过，但**尚未在游戏内逐项点过**；请重点看版本说明页和设置窗口。
- 联机底层已做本机冒烟测试：TCP 连接/收发/拆帧 **PASS**；UDP 局域网房间发现/解析 **PASS**（详见 `tools/mptest`）。完整的双游戏实例联机仍需在游戏里实测。
