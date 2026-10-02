using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using MlynarMod.MlynarModCode.Core;

namespace MlynarMod.MlynarModCode.Cards;

public class ThisSwordOnly() : MlynarCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6, ValueProp.Move), new DynamicVar("Poise", 2)];
    public override List<(string, string)> Localization => new CardLoc("只握此剑", "造成 {Damage:diff()} 点伤害。若在鞘中，蓄势 +{Poise}。");

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Hit(ctx, play);
        if (!MlynarRuntime.IsDrawn(play.Player.Creature))
            await MlynarRuntime.GainPoise(ctx, play.Player.Creature, DynamicVars["Poise"].IntValue);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2);
}

public class ItRained() : MlynarCard(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(4, ValueProp.Move), new DynamicVar("Hits", 2)];
    public override List<(string, string)> Localization => new CardLoc("下雨了", "对所有敌人造成 {Damage:diff()} 点伤害 {Hits} 次。");

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        await HitAll(ctx, play, DynamicVars["Hits"].IntValue);

    protected override void OnUpgrade() => DynamicVars["Hits"].UpgradeValueBy(1);
}

public class YellowSand() : MlynarCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8, ValueProp.Move)];
    public override List<(string, string)> Localization => new CardLoc("黄沙", "造成 {Damage:diff()} 点伤害。弃 1 张牌，抽 1 张牌。");

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Hit(ctx, play);
        await DiscardOne(ctx, play);
        await CardPileCmd.Draw(ctx, 1, play.Player);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2);
}

public class OldBranch() : MlynarCard(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(4, ValueProp.Move)];
    public override List<(string, string)> Localization => new CardLoc("旧枝", "对随机敌人造成 {Damage:diff()} 点伤害 3 次。");

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) => await HitRandom(ctx, play, 3);
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(1);
}

public class ObliqueCut() : MlynarCard(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8, ValueProp.Move), new DynamicVar("Bonus", 6)];
    public override List<(string, string)> Localization => new CardLoc("斜劈", "造成 {Damage:diff()} 点伤害。若目标有格挡，再造成 {Bonus} 点伤害。");

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var blocked = play.Target?.Block > 0;
        await Hit(ctx, play);
        if (blocked)
            await HitAmount(ctx, play, DynamicVars["Bonus"].BaseValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars["Bonus"].UpgradeValueBy(2);
    }
}

public class SwordFlourish() : MlynarCard(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(4, ValueProp.Move)];
    public override List<(string, string)> Localization => new CardLoc("剑花", "造成 {Damage:diff()} 点伤害两次。");

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) => await Hit(ctx, play, 2);
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2);
}

public class KeepToOneself() : MlynarCard(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5, ValueProp.Move), new DynamicVar("Poise", 1)];
    public override List<(string, string)> Localization => new CardLoc("独善其身", "获得 {Block:diff()} 点格挡。蓄势 +{Poise}。");

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        Block(play);
        await MlynarRuntime.GainPoise(ctx, play.Player.Creature, DynamicVars["Poise"].IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2);
        DynamicVars["Poise"].UpgradeValueBy(1);
    }
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

public class UselessHabit() : MlynarCard(0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(3, ValueProp.Move), new DynamicVar("Poise", 1)];
    public override List<(string, string)> Localization => new CardLoc("无用习惯", "获得 {Block:diff()} 点格挡。本回合结束时，若你没有打出攻击牌，蓄势 +{Poise}。");

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        Block(play);
        await MlynarRuntime.AddEndTurnPoiseIfNoAttack(ctx, play.Player.Creature, DynamicVars["Poise"].IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(1);
        DynamicVars["Poise"].UpgradeValueBy(1);
    }
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
