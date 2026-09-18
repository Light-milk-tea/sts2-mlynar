using BaseLib.Abstracts;
using BaseLib.Commands;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MlynarMod.MlynarModCode.Core;
using MlynarMod.MlynarModCode.Powers;

namespace MlynarMod.MlynarModCode.Cards;

public class ThisSwordOnly() : MlynarCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy), IHalfWindowBonus
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new MlynarDamageVar(5, ValueProp.Move)];
    public override List<(string, string)> Localization => new CardLoc("只握此剑", "造成 {Damage:diff()} 点伤害。拔剑中时，只获得一半蓄势加成。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) => await Hit(ctx, play);
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2);
}

public class NeedNotSharpen() : MlynarCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new MlynarDamageVar(7, ValueProp.Move), new DynamicVar("Poise", 1)];
    public override List<(string, string)> Localization => new CardLoc("不必开刃", "造成 {Damage:diff()} 点伤害。蓄势 +{Poise}。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Hit(ctx, play);
        await MlynarRuntime.GainPoise(ctx, play.Player.Creature, DynamicVars["Poise"].IntValue);
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2);
}

public class SittingThere() : MlynarCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new MlynarDamageVar(6, ValueProp.Move), new BlockVar(4, ValueProp.Move)];
    public override List<(string, string)> Localization => new CardLoc("坐在那里", "造成 {Damage:diff()} 点伤害。获得 {Block:diff()} 点格挡。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Hit(ctx, play);
        Block(play);
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars.Block.UpgradeValueBy(2);
    }
}

public class ItRained() : MlynarCard(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new MlynarDamageVar(4, ValueProp.Move)];
    public override List<(string, string)> Localization => new CardLoc("下雨了", "对所有敌人造成 {Damage:diff()} 点伤害。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) => await HitAll(ctx, play);
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2);
}

public class AsItStands() : MlynarCard(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new MlynarDamageVar(4, ValueProp.Move)];
    public override List<(string, string)> Localization => new CardLoc("事到如今", "造成 {Damage:diff()} 点伤害。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) => await Hit(ctx, play);
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2);
}

public class Wearisome() : MlynarCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new MlynarDamageVar(6, ValueProp.Move), new PowerVar<WeakPower>(1)];
    public override List<(string, string)> Localization => new CardLoc("令人厌倦", "造成 {Damage:diff()} 点伤害。施加 {WeakPower} 层虚弱。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Hit(ctx, play);
        await PowerCmd.Apply<WeakPower>(ctx, play.Target!, DynamicVars.Weak.BaseValue, play.Player.Creature, this);
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2);
}

public class Shameless() : MlynarCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new MlynarDamageVar(6, ValueProp.Move), new PowerVar<VulnerablePower>(1)];
    public override List<(string, string)> Localization => new CardLoc("不知可耻", "造成 {Damage:diff()} 点伤害。施加 {VulnerablePower} 层易伤。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Hit(ctx, play);
        await PowerCmd.Apply<VulnerablePower>(ctx, play.Target!, DynamicVars.Vulnerable.BaseValue, play.Player.Creature, this);
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2);
}

public class OneWish() : MlynarCard(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new MlynarDamageVar(14, ValueProp.Move)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    public override List<(string, string)> Localization => new CardLoc("一次许愿", "造成 {Damage:diff()} 点伤害。若击杀，抽 1 张牌。消耗。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Hit(ctx, play);
        if (play.Target is { CurrentHp: <= 0 })
            await CardPileCmd.Draw(ctx, 1, play.Player);
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4);
}

public class SameSword() : MlynarCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new MlynarDamageVar(7, ValueProp.Move), new DynamicVar("Bonus", 4)];
    public override List<(string, string)> Localization => new CardLoc("同一柄剑", "造成 {Damage:diff()} 点伤害。若蓄势不少于 4，再造成 {Bonus} 点伤害。不进入拔剑。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Hit(ctx, play);
        if (MlynarRuntime.GetPoise(play.Player.Creature) >= 4)
            await DamageCmd.Attack(DynamicVars["Bonus"].BaseValue).FromCard(this, play).Targeting(play.Target!).Execute(ctx);
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars["Bonus"].UpgradeValueBy(2);
    }
}

public class YellowSand() : MlynarCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new MlynarDamageVar(8, ValueProp.Move)];
    public override List<(string, string)> Localization => new CardLoc("黄沙", "造成 {Damage:diff()} 点伤害。弃 1 张牌，抽 1 张牌。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Hit(ctx, play);
        await CardSelectCmd.FromHandForDiscard(ctx, play.Player, new(new LocString("characters", "mlynar_discard_one"), 1), _ => true, this);
        await CardPileCmd.Draw(ctx, 1, play.Player);
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2);
}

public class OutsideTheCity() : MlynarCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Poise", 1), new DynamicVar("Cards", 2)];
    public override List<(string, string)> Localization => new CardLoc("城外见闻", "蓄势 +{Poise}。抽 {Cards} 张牌。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await MlynarRuntime.GainPoise(ctx, play.Player.Creature, DynamicVars["Poise"].IntValue);
        await CardPileCmd.Draw(ctx, DynamicVars["Cards"].IntValue, play.Player);
    }
    protected override void OnUpgrade() => DynamicVars["Cards"].UpgradeValueBy(1);
}

public class Summons() : MlynarCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5, ValueProp.Move), new DynamicVar("Poise", 1)];
    public override List<(string, string)> Localization => new CardLoc("传唤", "获得 {Block:diff()} 点格挡。蓄势 +{Poise}。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        Block(play);
        await MlynarRuntime.GainPoise(ctx, play.Player.Creature, DynamicVars["Poise"].IntValue);
    }
    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3);
}

public class OfficialBusiness() : MlynarCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(8, ValueProp.Move)];
    public override List<(string, string)> Localization => new CardLoc("公事公办", "获得 {Block:diff()} 点格挡。");
    protected override Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        Block(play);
        return Task.CompletedTask;
    }
    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3);
}

public class ReadTheWind() : MlynarCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BaseLib.Cards.Variables.ScryVar(3)];
    public override List<(string, string)> Localization => new CardLoc("看风", "预见 {Scry}。你弃掉的每张牌：蓄势 +1。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var result = await ScryCmd.Execute(ctx, play.Player, DynamicVars["Scry"].IntValue);
        if (result.Discarded.Count > 0)
            await MlynarRuntime.GainPoise(ctx, play.Player.Creature, result.Discarded.Count);
    }
    protected override void OnUpgrade() => DynamicVars["Scry"].UpgradeValueBy(1);
}

public class ReadPaper() : MlynarCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Cards", 2)];
    public override List<(string, string)> Localization => new CardLoc("看报", "抽 {Cards} 张牌。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        await CardPileCmd.Draw(ctx, DynamicVars["Cards"].IntValue, play.Player);
    protected override void OnUpgrade() => DynamicVars["Cards"].UpgradeValueBy(1);
}

public class DoesNotBlock() : MlynarCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Reduce", 1)];
    public override List<(string, string)> Localization => new CardLoc("并不挡人", "本回合你受到的攻击伤害 -{Reduce}。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await MlynarRuntime.EnsureBrain(ctx, play.Player.Creature);
        MlynarRuntime.Brain(play.Player.Creature).IncomingReduceThisTurn += DynamicVars["Reduce"].IntValue;
    }
    protected override void OnUpgrade() => DynamicVars["Reduce"].UpgradeValueBy(1);
}

public class UselessHabit() : MlynarCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5, ValueProp.Move)];
    public override List<(string, string)> Localization => new CardLoc("无用习惯", "获得 {Block:diff()} 点格挡。本回合结束时若未打出攻击，蓄势 +1。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        Block(play);
        await MlynarRuntime.EnsureBrain(ctx, play.Player.Creature);
        var brain = MlynarRuntime.Brain(play.Player.Creature);
        brain.SitPoiseThisTurn += 1;
    }
    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3);
}

public class RoutineMail() : MlynarCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Poise", 2)];
    public override List<(string, string)> Localization => new CardLoc("例行信件", "弃 1 张牌。蓄势 +{Poise}。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CardSelectCmd.FromHandForDiscard(ctx, play.Player, new(new LocString("characters", "mlynar_discard_one"), 1), _ => true, this);
        await MlynarRuntime.GainPoise(ctx, play.Player.Creature, DynamicVars["Poise"].IntValue);
    }
    protected override void OnUpgrade() => DynamicVars["Poise"].UpgradeValueBy(1);
}

public class Ready() : MlynarCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Poise", 2)];
    public override List<(string, string)> Localization => new CardLoc("已做好准备", "下个回合开始时，蓄势 +{Poise}。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await MlynarRuntime.EnsureBrain(ctx, play.Player.Creature);
        MlynarRuntime.Brain(play.Player.Creature).NextTurnBonusPoise += DynamicVars["Poise"].IntValue;
    }
    protected override void OnUpgrade() => DynamicVars["Poise"].UpgradeValueBy(1);
}
