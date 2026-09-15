# 杀戮尖塔 2 模组制作与创意工坊发布流程

供后续对照用。游戏处于抢先体验，工具、模板和路径可能随版本变动；以文末官方/社区文档为准。

---

## 1. 先建立正确预期

- 二代模组与一代（Java / ModTheSpire）**不是同一套东西**，一代模组不能直接移植。
- 技术栈是 **Godot + C#（.NET 9+）**。不会 C# 会非常吃力。
- 网上若出现「Lua / `modinfo.lua` / 游戏内一键发布」之类说法，**不要采信**。当前主流是：C# 工程 + 社区模板（BaseLib）+ Mega Crit 官方上传器。
- 抢先体验期间游戏更新频繁，**模组和 BaseLib 经常被打断**，发布后要跟着修。
- 有模组 / 无模组是**两套存档**。改玩法内容前先备份。

---

## 2. 整条流程一览

```
Steam 安装《Slay the Spire 2》
    ↓
装开发环境（.NET SDK、MegaDot/Godot、IDE、订阅 BaseLib）
    ↓
用社区模板创建模组工程
    ↓
配置 Godot 路径和游戏安装路径
    ↓
写内容 → Publish 到本地 mods 文件夹
    ↓
进游戏 Settings → Mod Settings 打开并实测
    ↓
用官方 Mod Uploader 上传 Steam 创意工坊
    ↓
游戏更新后维护、再上传新版本
```

最小闭环（第一次建议只做到这一步）：

1. 空/内容模板能出现在模组列表里。
2. 加一张最简单的卡，游戏里能看到、能打出。
3. 用上传器发一个 **私有** 工坊物品，自己订阅验证。

---

## 3. 需要准备什么

| 项目 | 说明 | 地址 / 位置 |
|------|------|-------------|
| 游戏本体 | Steam 正版，创意工坊只认 Steam 版 | 库中的 Slay the Spire 2 |
| .NET SDK | 9.0 或更高 | https://dotnet.microsoft.com/download/dotnet/9.0 |
| IDE | 社区教程多用 Rider；Visual Studio 也行 | 注意工程格式必须是 `.sln`，不要 `.slnx` |
| MegaDot | Mega Crit 定制 Godot，优先用这个 | https://megadot.megacrit.com/ |
| Godot 备选 | 版本必须和当前 MegaDot 对齐 | 目前资料多对应 **Godot 4.5.1 Mono / .NET 版**，不要下普通版 |
| BaseLib | 社区标准依赖，加卡/遗物/角色几乎都靠它 | 工坊订阅：https://steamcommunity.com/sharedfiles/filedetails/?id=3737335127 |
| 官方上传器 | 发工坊用 | https://github.com/megacrit/sts2-mod-uploader |

BaseLib 订阅后的本机目录（工坊 AppID 为 `2868840`）：

```
Steam\steamapps\workshop\content\2868840\3737335127\BaseLib
```

若工坊订阅失败，可从 GitHub 下 dll / pck / json 放到游戏 `mods` 里，但不会自动更新：

https://github.com/Alchyr/BaseLib-StS2

---

## 4. 本机常见路径

按自己的 Steam 库位置改。游戏不在默认盘时，在 Steam 里右键游戏 → **管理 → 浏览本地文件**。

```
游戏安装目录
  Steam\steamapps\common\Slay the Spire 2\

游戏数据（Windows，编译时会引用这里的 dll）
  ...\Slay the Spire 2\data_sts2_windows_x86_64\

本地开发模组（Publish 的目标）
  ...\Slay the Spire 2\mods\你的模组名\

工坊已订阅模组
  Steam\steamapps\workshop\content\2868840\<工坊物品ID>\

游戏日志
  %AppData%\Roaming\SlayTheSpire2\Player.log
```

纯原版启动（关掉所有模组）：Steam 启动项加 `-nomods`。

---

## 5. 创建工程

### 5.1 安装模板

```powershell
dotnet new install Alchyr.Sts2.Templates
```

源仓库与说明：https://github.com/Alchyr/ModTemplate-StS2

### 5.2 按类型选模板

模组名：**不要空格、不要下划线**。

| 用途 | 模板短名 | 命令示例 |
|------|----------|----------|
| 最小起步 / 机制向 | `alchyrsts2mod` | `dotnet new alchyrsts2mod --ModAuthor 你的名字 -o MyFirstMod` |
| 卡、遗物、药水等 | `alchyrsts2contentmod` | `dotnet new alchyrsts2contentmod --ModAuthor 你的名字 -o MyFirstMod` |
| 新角色 | `alchyrsts2charmod` | `dotnet new alchyrsts2charmod --ModAuthor 你的名字 -o MyFirstMod` |

查看选项：

```powershell
dotnet new alchyrsts2mod --help
```

在 Rider 里也可以：`File → New Solution`，选对应的 Slay the Spire 2 模板。勾选 **Put solution and project in same directory**，工程格式选 `.sln`。

### 5.3 改路径

打开工程里的 `Directory.Build.props`（有的示例工程叫 `local.props`）：

1. **Godot / MegaDot 的 exe 路径**，必须指到可执行文件（Windows 以 `.exe` 结尾），路径两边不要加引号。
2. 游戏不在默认位置时，取消注释并填写游戏安装目录。编译报找不到 `data_sts2_windows_x86_64` 就是这项没配对。

改完先点一次 **Build**，确认能编过。角色模板若出现本地化相关错误，往往说明工程已接上，按模板 wiki 用 IDE 的 Generate localization 即可。

---

## 6. 模组文件结构

游戏能加载的发布物通常是这三个文件，建议单独放一个以模组 id 命名的文件夹：

```
mods\ModName\
  ├── ModName.json    # 清单：id、显示名、版本、依赖
  ├── ModName.dll     # 代码（没有代码改动可以没有）
  └── ModName.pck     # 文本、图片、场景（没有资源可以没有）
```

发给别人或上传工坊时，上传的是**这个文件夹**，不是整个 Visual Studio / Rider 工程。

### 6.1 清单字段（`ModName.json`）

```json
{
  "id": "ModName",
  "name": "显示名称",
  "author": "作者",
  "description": "简介",
  "version": "v0.0.1",
  "has_pck": true,
  "has_dll": true,
  "min_game_version": "0.105.0",
  "dependencies": [{ "id": "BaseLib", "min_version": "3.1.2" }],
  "affects_gameplay": true
}
```

| 字段 | 注意 |
|------|------|
| `id` | 技术 ID，决定游戏去找哪个 dll/pck。**创建后不要乱改** |
| `version` | 每次正式发布记得加版本号 |
| `min_game_version` | 游戏 0.105+ 才有；写你实际测试过的最低版本 |
| `dependencies` | 依赖 BaseLib 就必须写；`min_version` 同样是 0.105+ |
| `affects_gameplay` | 影响玩法必须 `true`。纯美化/信息类才 `false`。写错会导致多人不同步 |

模板生成的 `id` 不要手改去「更好看」，显示名改 `name` 即可。

---

## 7. 本地编译、安装、测试

### 7.1 Build 和 Publish 的区别

| 操作 | 做什么 | 什么时候用 |
|------|--------|------------|
| **Build** | 只编译 dll，拷到游戏 `mods` | 只改了 `.cs` |
| **Publish（本地文件夹）** | 编译 dll + 用 Godot 打 pck + 拷 json/dll/pck | 改了文本、图、场景，或第一次安装 |

**只点 Build、不 Publish，新文本和新图不会进游戏。**

Rider：项目右键 → Publish → Local folder，选项可保持默认。Godot 路径不对时，Publish 会失败。

### 7.2 进游戏检查

1. 正常启动 STS2（不要加 `-nomods`）。
2. **Settings → Mod Settings**，确认模组出现并能勾选。
3. 按提示重启后再开一局实测。
4. 没有出现：查 `mods` 目录是否有三件套、json 的 `id` 是否和文件名一致、看 `Player.log`。

Steam 启动项如果以后要区分：

- 纯原版：`-nomods`
- 模组存档与原版存档互相独立，不要混用同一套进度预期。

---

## 8. 做内容时怎么想

社区默认走 BaseLib，而不是从零 Hook 游戏。

1. 想清楚要做什么（一张卡、一件遗物、一个角色……）。
2. 在原版里找**最像的现成内容**，反编译看它调用了哪些 Command。
3. 继承 BaseLib 的 `Custom*Model`（或内容/角色模板里以你模组名开头的基类，例如 `MyFirstModCard`）。
4. **优先用 Command**（如 `DamageCmd.Attack`、`PowerCmd.Apply`），不要直接改底层数据。
5. 本地化：卡名、描述等用模板/分析器生成，再挪到 `localization/eng/...`（以及你需要的中文文件）。

角色模板首次打开角色类时，常会缺 character / ancient 本地化：Alt+Enter → Generate localization，把生成文本放到对应 json。

更细的「怎么加卡 / 遗物 / 事件」以模板 wiki 和 BaseLib Wiki 为准，见文末链接。

---

## 9. 上传 Steam 创意工坊

官方工具仓库：https://github.com/megacrit/sts2-mod-uploader  
到 **Releases** 下载 `ModUploader.exe`。上传时 **Steam 必须已登录**，且账号已拥有游戏。

### 9.1 首次上传

1. 本地已经测通，`mods\ModName\` 里 json / dll / pck 齐全。
2. 双击 `ModUploader.exe`，生成 `NewModWorkspace`，改成你自己的工作区名（例如 `MyFirstModWorkshop`）。
3. 把模组文件夹里的文件放进工作区的 `content` 目录（上传的是 content，不是源码工程）。
4. 填写同目录的 `workshop.json`（标题、简介、可见性、标签、依赖等；字段含义看工作区里另一份 README）。
5. 换成封面 `image.png`，**必须小于 1MB**（Steam 限制）。
6. 在 **ModUploader.exe 所在目录** 打开终端：

```powershell
.\ModUploader.exe upload -w 你的工作区文件夹名
```

7. 成功后会生成 `mod_id.txt`。这是工坊物品 ID，**以后更新全靠它，不要删、不要改丢。**

### 9.2 建议的可见性

第一次先设 **private（仅自己）** 或好友可见：

1. 自己订阅，确认工坊能下下来。
2. 换目录 / 清本地 `mods` 后再用「只订阅工坊」的方式验证。
3. 确认 `workshop.json` 和模组 json 都声明了 BaseLib 等依赖。
4. 再改公开。

### 9.3 更新已发布模组

1. 覆盖工作区 `content` 为新的 json / dll / pck。
2. 同步提高 `ModName.json` 里的 `version`。
3. 可选：在 `workshop.json` 填 `changeNote`（更新说明）。
4. 再执行同一条：

```powershell
.\ModUploader.exe upload -w 你的工作区文件夹名
```

上传器会读 `mod_id.txt`，更新同一个工坊物品，而不是新建一个。

### 9.4 上传失败时

同目录会生成 `mod-uploader.log`。把日志和操作说明留给官方或 Discord `#sts2-modding`。常见原因：Steam 没开、没买正版、封面超 1MB、`content` 结构不对。

---

## 10. 发布后维护

- 游戏有 **主分支** 和 **beta 分支**。Steam 右键游戏 → 属性 → 测试版。做模组时固定一条分支，不要混用。
- 主分支大约每月更，beta 大约一两周更。更新后先看 BaseLib 是否已适配。
- 只用 BaseLib API 的模组，很多时候等 BaseLib 更新就能好；自己打了 Harmony / 底层补丁则要自己跟。
- 工坊页写清楚：支持的游戏版本、依赖、会不会改存档、是否影响成就/多人。
- 模组开着时，官方成就追踪可能关闭；冲成就用原版启动。

---

## 11. 常见问题

| 现象 | 先查什么 |
|------|----------|
| 模组列表没有 | 没 Publish；文件不在 `mods`；`id` 和文件名不一致；json 损坏 |
| 有模组但没新文本/立绘 | 只 Build 没 Publish，pck 仍是旧的 |
| 找不到游戏数据 | `Directory.Build.props` 的游戏路径不对 |
| 找不到 Godot | Godot/MegaDot 路径没指到 exe，或下成了非 .NET 版 |
| `Godot.NET.Sdk/4.5.1` 找不到 | 终端执行：`dotnet nuget add source https://api.nuget.org/v3/index.json` |
| 别人订阅后缺功能 | 工坊依赖或模组 json 的 `dependencies` 没写 BaseLib |
| 更新游戏后全坏 | EA 打断 API；等/更新 BaseLib，再重编自己的模组 |
| 多人不同步 | `affects_gameplay` 填错，或双方模组/版本不一致 |

---

## 12. 对照清单

发布前自己勾一遍：

- [ ] `ModName.json` 的 `id` / `version` / `dependencies` / `affects_gameplay` 正确
- [ ] 本地 `mods\ModName\` 有需要的 json、dll、pck
- [ ] 游戏内 Mod Settings 能启用，实测过主流程
- [ ] 看过 `Player.log`，没有启动错误
- [ ] 上传器 `content` 里是发布文件，不是整个源码仓库
- [ ] `workshop.json` 填完，封面 `image.png` < 1MB
- [ ] 依赖（尤其 BaseLib）在工坊侧也声明了
- [ ] 首次为私有，自己订阅验证后再公开
- [ ] 备份了 `mod_id.txt` 和工作区

---

## 13. 主要文档与求助处

按这个顺序查，比到处搜帖子靠谱：

1. 环境搭建：https://github.com/Alchyr/ModTemplate-StS2/wiki/Setup
2. 模组结构、清单、工坊说明：https://github.com/Alchyr/ModTemplate-StS2/wiki/Modding-Basics
3. 模板总入口（加卡、调试、反编译等）：https://github.com/Alchyr/ModTemplate-StS2/wiki
4. BaseLib API：https://alchyr.github.io/BaseLib-Wiki/
5. BaseLib 源码：https://github.com/Alchyr/BaseLib-StS2
6. 更细的分步教程：https://fresh-milkshake.github.io/Modding-Tutorial/
7. 官方上传器：https://github.com/megacrit/sts2-mod-uploader
8. MegaDot：https://megadot.megacrit.com/
9. 提问：Slay the Spire Discord 的 **#sts2-modding**

---

## 14. 和本文件夹的关系

本文件只描述流程，不代替工程。源码工程、上传器工作区、`mod_id.txt` 建议分开放，避免把未发布的源码整包传上工坊。
