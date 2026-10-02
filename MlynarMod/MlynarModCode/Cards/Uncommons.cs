using BaseLib.Abstracts;
using BaseLib.Cards.Variables;
using BaseLib.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MlynarMod.MlynarModCode.Core;
using MlynarMod.MlynarModCode.Powers;

namespace MlynarMod.MlynarModCode.Cards;

public class UnresolvedSorrow() : MlynarCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy), IDrawSwordCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6, ValueProp.Move)];
    public override List<(string, string)> Localization => new CardLoc("未宽解的悲哀", "进入拔剑（1 回合）。造成 {Damage:diff()} 点伤害两次，然后退出拔剑。这次退出不清空蓄势。");

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await MlynarRuntime.EnterDrawn(ctx, play.Player, 1, this);
        await Hit(ctx, play, 2);
        await MlynarRuntime.Sheathe(ctx, play.Player.Creature, auto: false, keepPoise: true);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2);
}

public class IfRotten() : MlynarCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(9, ValueProp.Move), new DynamicVar("Poise", 2)];
    public override List<(string, string)> Localization => new CardLoc("若也腐朽", "造成 {Damage:diff()} 点伤害。若目标是精英或首领，蓄势 +{Poise}。");

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Hit(ctx, play);
        if (MlynarRuntime.IsEliteOrBoss(play.Target))
            await MlynarRuntime.GainPoise(ctx, play.Player.Creature, DynamicVars["Poise"].IntValue);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2);
}

public class WhirlingCut() : MlynarCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(5, ValueProp.Move)];
    public override List<(string, string)> Localization => new CardLoc("回旋斩", "对所有敌人造成 {Damage:diff()} 点伤害。若在拔剑中，再对所有敌人造成等量伤害。");

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var times = MlynarRuntime.IsDrawn(play.Player.Creature) ? 2 : 1;
        await HitAll(ctx, play, times);
    }

    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2);
}

public class SheatheCard() : MlynarCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5, ValueProp.Move)];
    public override List<(string, string)> Localization => new CardLoc("收剑", "若在拔剑中，退出拔剑。否则，获得 {Block:diff()} 点格挡。");

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (MlynarRuntime.IsDrawn(play.Player.Creature))
            await MlynarRuntime.Sheathe(ctx, play.Player.Creature, auto: false);
        else
            Block(play);
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3);
}

public class UnfinishedWork() : MlynarCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Poise", 2)];
    public override List<(string, string)> Localization => new CardLoc("未完的公事", "蓄势 +{Poise}。弃 1 张牌。");

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await MlynarRuntime.GainPoise(ctx, play.Player.Creature, DynamicVars["Poise"].IntValue);
        await DiscardOne(ctx, play);
    }

    protected override void OnUpgrade() => DynamicVars["Poise"].UpgradeValueBy(1);
}

public class DrawMargaret() : MlynarCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Vigor", 6), new DynamicVar("Self", 5)];
    public override List<(string, string)> Localization => new CardLoc("拔剑，玛嘉烈", "获得 {Vigor} 点活力。本回合结束时，若你没有打出攻击牌，受到 {Self} 点伤害。");

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await PowerCmd.Apply<VigorPower>(ctx, play.Player.Creature, DynamicVars["Vigor"].BaseValue, play.Player.Creature, this);
        await MlynarRuntime.AddEndTurnDamageIfNoAttack(ctx, play.Player.Creature, DynamicVars["Self"].IntValue);
    }

    protected override void OnUpgrade() => DynamicVars["Vigor"].UpgradeValueBy(2);
}

public class FamilyLetter() : MlynarCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new ScryVar(3)];
    public override List<(string, string)> Localization => new CardLoc("家书", "预见 {Scry}。你可以将弃牌堆的 1 张牌放到抽牌堆顶。");

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await ScryCmd.Execute(ctx, play.Player, DynamicVars["Scry"].IntValue);
        await OfferTopdeck(ctx, play);
    }

    protected override void OnUpgrade() => DynamicVars["Scry"].UpgradeValueBy(1);
}

public class OnePaper() : MlynarCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Poise", 4), new DynamicVar("Cards", 2)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    public override List<(string, string)> Localization => new CardLoc("一纸辞呈", "蓄势 +{Poise}。抽 {Cards} 张牌。消耗。");

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await MlynarRuntime.GainPoise(ctx, play.Player.Creature, DynamicVars["Poise"].IntValue);
        await CardPileCmd.Draw(ctx, DynamicVars["Cards"].IntValue, play.Player);
    }

    protected override void OnUpgrade() => DynamicVars["Poise"].UpgradeValueBy(1);
}

public class LongRoad() : MlynarCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Poise", 2), new DynamicVar("Hold", 1)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];
    public override bool ShouldReceiveCombatHooks => true;
    public override List<(string, string)> Localization => new CardLoc("远路", "蓄势 +{Poise}。若回合结束时这张牌仍在手牌，蓄势 +{Hold}。");

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        await MlynarRuntime.GainPoise(ctx, play.Player.Creature, DynamicVars["Poise"].IntValue);

    public override async Task AfterSideTurnEnd(PlayerChoiceContext ctx, CombatSide side, IEnumerable<Creature> creatures)
    {
        if (Owner?.Creature == null || !creatures.Contains(Owner.Creature)) return;
        if (Pile?.Type != PileType.Hand) return;

        var creature = Owner.Creature;
        await MlynarRuntime.EnsureBrain(ctx, creature);
        var brain = MlynarRuntime.Brain(creature);
        var amount = DynamicVars["Hold"].IntValue;
        if (brain.SheatheResolvedThisEnd)
            await MlynarRuntime.GainPoise(ctx, creature, amount);
        else
            brain.HoldPoise += amount;
    }

    protected override void OnUpgrade() => DynamicVars["Poise"].UpgradeValueBy(1);
}

public class PersistsAnyway() : MlynarCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Cards", 2), new DynamicVar("Self", 3)];
    public override List<(string, string)> Localization => new CardLoc("一意孤行", "抽 {Cards} 张牌。下个回合能量 +1。本回合结束时受到 {Self} 点伤害。");

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CardPileCmd.Draw(ctx, DynamicVars["Cards"].IntValue, play.Player);
        await PowerCmd.Apply<EnergyNextTurnPower>(ctx, play.Player.Creature, 1, play.Player.Creature, this);
        await MlynarRuntime.AddEndTurnDamage(ctx, play.Player.Creature, DynamicVars["Self"].IntValue);
    }

    protected override void OnUpgrade() => DynamicVars["Self"].UpgradeValueBy(-1);
}

public class WandererCard() : MlynarCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Dmg", 2), new DynamicVar("Crowd", 4), new DynamicVar("Red", 2)];
    public override List<(string, string)> Localization => new CardLoc("游侠", "你的攻击伤害 +{Dmg}。若场上敌人不少于 3 名，改为 +{Crowd}，且你受到的伤害 -{Red}。");

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await PowerCmd.Apply<WandererPower>(ctx, play.Player.Creature, 1, play.Player.Creature, this);
        play.Player.Creature.GetPower<WandererPower>()!.Add(
            DynamicVars["Dmg"].IntValue,
            DynamicVars["Crowd"].IntValue,
            DynamicVars["Red"].IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Dmg"].UpgradeValueBy(1);
        DynamicVars["Crowd"].UpgradeValueBy(1);
        DynamicVars["Red"].UpgradeValueBy(1);
    }
}

public class IndifferentCard() : MlynarCard(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Thorns", 4)];
    public override List<(string, string)> Localization => new CardLoc("无动于衷", "获得 {Thorns} 层荆棘。");

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        await PowerCmd.Apply<ThornsPower>(ctx, play.Player.Creature, DynamicVars["Thorns"].BaseValue, play.Player.Creature, this);

    protected override void OnUpgrade() => DynamicVars["Thorns"].UpgradeValueBy(2);
}
