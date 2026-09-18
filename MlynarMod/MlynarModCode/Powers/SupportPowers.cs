using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Powers;

namespace MlynarMod.MlynarModCode.Powers;

public class WrathMarkPower : MlynarPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "愠怒",
        "进入拔剑时结算为等量真实伤害。",
        "进入拔剑时结算为等量真实伤害。");
}

public class TenYearBalloonPower : MlynarPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "十年气球人",
        "每个完整鞘中回合结束获得 1 层灌满。进入拔剑时兑成伤害或格挡。",
        "每个完整鞘中回合结束获得 1 层灌满。进入拔剑时兑成伤害或格挡。");
}

public class ManInSheathPower : MlynarPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "鞘中人",
        "每场战斗开始蓄势 +3。本场第一次拔剑持续 +1 回合。",
        "每场战斗开始蓄势 +3。本场第一次拔剑持续 +1 回合。");
}

public class WandererPower : MlynarPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "游侠",
        "攻击加伤；场上敌人不少于 3 名时加得更多，并减伤。",
        "攻击加伤；场上敌人不少于 3 名时加得更多，并减伤。");
}

public class IndifferentPower : MlynarPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "无动于衷",
        "受击时对来源造成真实伤害。",
        "受击时对来源造成真实伤害。");
}

public class FearNoDarkPower : MlynarPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "不畏苦暗",
        "收剑时若这次拔剑中受伤或未击杀，保留一半蓄势并获得格挡。",
        "收剑时若这次拔剑中受伤或未击杀，保留一半蓄势并获得格挡。");
}

public class WhyMePower : MlynarPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "留下的是我",
        "每次收剑：抽 2，下回合能量 +1。",
        "每次收剑：抽 2，下回合能量 +1。");
}

public class WildernessPower : MlynarPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "没有灯光的荒野",
        "鞘中回合若未打出攻击：蓄势 +2，获得格挡。",
        "鞘中回合若未打出攻击：蓄势 +2，获得格挡。");
}

public class RoadRemainsPower : MlynarPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public bool Used;

    public override List<(string, string)> Localization => new PowerLoc(
        "卡西米尔仍然有路",
        "本场第一次生命降至半数或以下：蓄势回满。本回合不能拔剑。",
        "本场第一次生命降至半数或以下：蓄势回满。本回合不能拔剑。");
}

public class AfterSheathePower : MlynarPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "收回之后",
        "每当你收剑，抽牌。",
        "每当你收剑，抽牌。");
}

public class TideOfLightPower : MlynarPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "光潮",
        "在拔剑中时，攻击伤害增加。",
        "在拔剑中时，攻击伤害增加。");
}

public class BenchPower : MlynarPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "长椅",
        "鞘中回合结束时若未攻击，获得格挡。",
        "鞘中回合结束时若未攻击，获得格挡。");
}

public class NoLeavePower : MlynarPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "不曾请假",
        "每个鞘中回合开始，额外获得蓄势。",
        "每个鞘中回合开始，额外获得蓄势。");
}

public class NeedNotReturnPower : MlynarPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "不必回岗",
        "每次受到攻击伤害，获得蓄势。",
        "每次受到攻击伤害，获得蓄势。");
}

public class RepayPower : MlynarPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "回击",
        "每次受到攻击伤害，对来源造成真实伤害。",
        "每次受到攻击伤害，对来源造成真实伤害。");
}

public class GoldenWrathPower : MlynarPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "金色的愠怒",
        "鞘中攻击施加愠怒。进入拔剑时引爆为真实伤害。",
        "鞘中攻击施加愠怒。进入拔剑时引爆为真实伤害。");
}

public class StandAlonePower : MlynarPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "独自拦下",
        "本场在鞘中时，受击反弹真实伤害。",
        "本场在鞘中时，受击反弹真实伤害。");
}
