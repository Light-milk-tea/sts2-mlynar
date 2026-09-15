# sts2-mlynar

杀戮尖塔 2 独立角色模组：明日方舟 **玛恩纳（Młynar）**。

明天开工前，先看这两份：

- [玛恩纳末端卡牌与机制设计](玛恩纳mod/玛恩纳末端卡牌与机制设计.md)
- [选题说明](玛恩纳mod/玛恩娜mod设计思路.md)

## 仓库里有什么

| 目录 / 文件 | 说明 |
| --- | --- |
| `玛恩纳mod/` | 角色设计稿 |
| `玛恩纳wiki/` | 玛恩纳资料库（给写卡用，不是原文库） |
| `MyFirstMod/` | 已跑通的试验角色工程（试炼者） |
| `mod制作发布流程.md` | 环境、发布、工坊流程 |
| `github提交SKILL.md` | 本仓库的提交规范 |

本机只读参照（观者 / 崩坠源码）放在 `_ref/`，**不进这个仓库**。换电脑后按下面两个原仓库自己浅克隆即可。

| 本机目录 | 原仓库 | 当前对照提交 |
| --- | --- | --- |
| `_ref/WatcherMod` | https://github.com/lamali292/WatcherMod | `a7afee6` |
| `_ref/Downfall` | https://github.com/lamali292/Downfall | `0dc5bb9` |

```
git clone --depth 1 https://github.com/lamali292/WatcherMod.git _ref/WatcherMod
git clone --depth 1 https://github.com/lamali292/Downfall.git _ref/Downfall
```

这两份是别人的模组源码，只读对照，不要改、不要再推回他们的仓库。

## 本机路径

`MyFirstMod/Directory.Build.props` 含本机 MegaDot 和游戏目录，已忽略。拷贝示例再改路径：

```
copy MyFirstMod\Directory.Build.props.example MyFirstMod\Directory.Build.props
```
