# ADOFAI Editor Studio X 构建说明

本目录是 **ADOFAI Editor Studio X 0.94.0** 源码工程（基于 AES 0.39.5，合入 NEXT UI 与全局圆角）。源码包不含游戏程序集，也不含已经编译的 DLL；需要在安装了 ADOFAI 和 Unity Mod Manager 的 Windows 机器上构建。

## 环境

- Windows
- .NET SDK（支持 SDK-style `net48` 项目）
- .NET Framework 4.8 Developer/Targeting Pack
- 已安装 A Dance of Fire and Ice 与 Unity Mod Manager

## 构建

在 PowerShell 中进入本目录：

```powershell
.\build-mod.ps1 -GameDir "E:\SteamLibrary\steamapps\common\A Dance of Fire and Ice"
```

`GameDir` 应指向包含 `A Dance of Fire and Ice.exe` 的游戏根目录。脚本会：

1. 检查游戏 Managed 目录和 UMM 依赖（含 `UnityEngine.SpriteMaskModule.dll`）；
2. 校验 `Info.json` 的 `Version` 与 `.csproj` 的 `<Version>` 是否一致，不一致直接失败；
3. 编译 Release DLL；
4. 打包成 `dist\ADOFAIEditorStudioX-v0.94.0.zip`，解压后就是 `Mods\ADOFAIEditorStudioX\`。

游戏装在默认路径时可以直接双击 `双击运行-自动构建.bat`。

## 安装

把 `dist\ADOFAIEditorStudioX-v0.94.0.zip` 解压到游戏的 `Mods` 目录，确保结构为：

```text
游戏目录\Mods\ADOFAIEditorStudioX\
    ADOFAIEditorStudio.dll
    Info.json
    AesIcon.rgba
    AesxLogo.png
    LICENSE
```

如果旧文件夹里还留着旧的 `ADOFAIEditorStudio.dll`，先删掉再放新 DLL。

## 版本号

- `Info.json` → `"Version": "0.94.0"`
- `ADOFAIEditorStudio.csproj` → `<Version>0.94.0</Version>`

两处必须一致；`build-mod.ps1` 会强制检查。

## 本机游戏路径

本工程默认使用：

`E:\SteamLibrary\steamapps\common\A Dance of Fire and Ice`

不在这个位置时用 `-GameDir` 指定。
