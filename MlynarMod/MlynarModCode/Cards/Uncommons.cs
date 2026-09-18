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

public class UnresolvedSorrow() : MlynarCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy), IDrawSwordCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new MlynarDamageVar(6, ValueProp.Move)];
    public override List<(string, string)> Localization => new CardLoc("未宽解的悲哀", "进入拔剑（1 回合）。造成 {Damage:diff()} 点伤害两次。这次拔剑中击杀敌人则收剑不重置。可提前收剑。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await MlynarRuntime.EnsureBrain(ctx, play.Player.Creature);
        var brain = MlynarRuntime.Brain(play.Player.Creature);
        brain.PendingNoResetOnKill = true;
        brain.PendingAllowEarlySheathe = true;
        await MlynarRuntime.EnterDrawn(ctx, play.Player, 1, this);
        await DamageCmd.Attack(Dmg).FromCard(this, play).Targeting(play.Target!).WithHitCount(2).Execute(ctx);
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2);
}

public class SecondCut() : MlynarCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new MlynarDamageVar(5, ValueProp.Move)];
    public override List<(string, string)> Localization => new CardLoc("再一剑", "造成 {Damage:diff()} 点伤害两次。不进入拔剑。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        await DamageCmd.Attack(Dmg).FromCard(this, play).Targeting(play.Target!).WithHitCount(2).Execute(ctx);
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2);
}

public class SheatheCard() : MlynarCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5, ValueProp.Move)];
    public override List<(string, string)> Localization => new CardLoc("入鞘", "若在拔剑中：立即收剑。否则获得 {Block:diff()} 点格挡。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (MlynarRuntime.IsDrawn(play.Player.Creature))
            await MlynarRuntime.Sheathe(ctx, play.Player.Creature);
        else
            Block(play);
    }
    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3);
}

public class LeaveAnInch() : MlynarCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Keep", 1)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    public override List<(string, string)> Localization => new CardLoc("留一寸", "下一次收剑：保留 {Keep} 点蓄势。不是不重置。消耗。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await MlynarRuntime.EnsureBrain(ctx, play.Player.Creature);
        MlynarRuntime.Brain(play.Player.Creature).NextSheatheKeepPoints += DynamicVars["Keep"].IntValue;
    }
    protected override void OnUpgrade() => DynamicVars["Keep"].UpgradeValueBy(1);
}

public class WandererCard() : MlynarCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Dmg", 2), new DynamicVar("Crowd", 4), new DynamicVar("Red", 2)];
    public override List<(string, string)> Localization => new CardLoc("游侠", "攻击伤害 +{Dmg}。场上敌人不少于 3 时改为 +{Crowd}，且受到伤害 -{Red}。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await PowerCmd.Apply<WandererPower>(ctx, play.Player.Creature, 1, play.Player.Creature, this);
        await MlynarRuntime.EnsureBrain(ctx, play.Player.Creature);
        var brain = MlynarRuntime.Brain(play.Player.Creature);
        brain.WandererDamage = DynamicVars["Dmg"].IntValue;
        brain.WandererCrowdDamage = DynamicVars["Crowd"].IntValue;
        brain.WandererReduce = DynamicVars["Red"].IntValue;
    }
    protected override void OnUpgrade()
    {
        DynamicVars["Dmg"].UpgradeValueBy(1);
        DynamicVars["Crowd"].UpgradeValueBy(1);
        DynamicVars["Red"].UpgradeValueBy(1);
    }
}

public class Beside() : MlynarCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5, ValueProp.Move), new DynamicVar("Red", 3)];
    public override List<(string, string)> Localization => new CardLoc("身侧", "获得 {Block:diff()} 点格挡。本回合受到的伤害 -{Red}。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        Block(play);
        await MlynarRuntime.EnsureBrain(ctx, play.Player.Creature);
        MlynarRuntime.Brain(play.Player.Creature).IncomingReduceThisTurn += DynamicVars["Red"].IntValue;
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
        DynamicVars["Red"].UpgradeValueBy(1);
    }
}

public class IndifferentCard() : MlynarCard(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Thorns", 4)];
    public override List<(string, string)> Localization => new CardLoc("无动于衷", "你每次受到攻击伤害，对来源造成 {Thorns} 点真实伤害。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await PowerCmd.Apply<IndifferentPower>(ctx, play.Player.Creature, DynamicVars["Thorns"].IntValue, play.Player.Creature, this);
        await MlynarRuntime.EnsureBrain(ctx, play.Player.Creature);
        MlynarRuntime.Brain(play.Player.Creature).Thorns += DynamicVars["Thorns"].IntValue;
        MlynarRuntime.Brain(play.Player.Creature).Taunt = true;
    }
    protected override void OnUpgrade() => DynamicVars["Thorns"].UpgradeValueBy(2);
}

public class RepayCard() : MlynarCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Thorns", 2)];
    public override List<(string, string)> Localization => new CardLoc("回击", "你每次受到攻击伤害，对来源造成 {Thorns} 点真实伤害。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await PowerCmd.Apply<RepayPower>(ctx, play.Player.Creature, DynamicVars["Thorns"].IntValue, play.Player.Creature, this);
        await MlynarRuntime.EnsureBrain(ctx, play.Player.Creature);
        MlynarRuntime.Brain(play.Player.Creature).Thorns += DynamicVars["Thorns"].IntValue;
    }
    protected override void OnUpgrade() => DynamicVars["Thorns"].UpgradeValueBy(1);
}

public class LightKept() : MlynarCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new MlynarDamageVar(10, ValueProp.Move)];
    public override List<(string, string)> Localization => new CardLoc("收着的光", "造成 {Damage:diff()} 点伤害。此伤害不是真实伤害。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) => await Hit(ctx, play);
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3);
}

public class CrossSword() : MlynarCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new MlynarDamageVar(9, ValueProp.Move), new DynamicVar("Alt", 13)];
    public override List<(string, string)> Localization => new CardLoc("横剑", "造成 {Damage:diff()} 点伤害。若目标意图攻击，改为 {Alt} 点。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var dmg = play.Target?.Monster?.IntendsToAttack == true ? DynamicVars["Alt"].BaseValue : Dmg;
        await DamageCmd.Attack(dmg).FromCard(this, play).Targeting(play.Target!).Execute(ctx);
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars["Alt"].UpgradeValueBy(3);
    }
}

public class PressTheBlade() : MlynarCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new MlynarDamageVar(8, ValueProp.Move), new DynamicVar("Red", 4)];
    public override List<(string, string)> Localization => new CardLoc("按剑", "造成 {Damage:diff()} 点伤害。该敌人本回合攻击伤害 -{Red}。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Hit(ctx, play);
        await PowerCmd.Apply<WeakPower>(ctx, play.Target!, 1, play.Player.Creature, this);
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2);
}

public class HandFirst() : MlynarCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new MlynarDamageVar(14, ValueProp.Move), new DynamicVar("Self", 3)];
    public override List<(string, string)> Localization => new CardLoc("先伸手", "受到 {Self} 点伤害。造成 {Damage:diff()} 点伤害。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CreatureCmd.Damage(ctx, play.Player.Creature, DynamicVars["Self"].BaseValue, ValueProp.Unpowered, play.Player.Creature, this, play);
        await Hit(ctx, play);
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars["Self"].UpgradeValueBy(-1);
    }
}

public class OldBranch() : MlynarCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new MlynarDamageVar(4, ValueProp.Move)];
    public override List<(string, string)> Localization => new CardLoc("旧枝", "对随机敌人造成 {Damage:diff()} 点伤害 3 次。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        await DamageCmd.Attack(Dmg).FromCard(this, play).TargetingRandomOpponents(play.Player.Creature.CombatState!, true).WithHitCount(3).Execute(ctx);
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(1);
}

public class Hostility() : MlynarCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new MlynarDamageVar(8, ValueProp.Move), new DynamicVar("Per", 3)];
    public override List<(string, string)> Localization => new CardLoc("敌视", "造成 {Damage:diff()} 点伤害。你每有一种负面，伤害 +{Per}。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var debuffs = play.Player.Creature.Powers.Count(p => p.Type == MegaCrit.Sts2.Core.Entities.Powers.PowerType.Debuff);
        await DamageCmd.Attack(Dmg + debuffs * DynamicVars["Per"].BaseValue).FromCard(this, play).Targeting(play.Target!).Execute(ctx);
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars["Per"].UpgradeValueBy(1);
    }
}

public class ThisNight() : MlynarCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new MlynarDamageVar(10, ValueProp.Move)];
    public override List<(string, string)> Localization => new CardLoc("这一夜", "对所有敌人造成 {Damage:diff()} 点伤害。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) => await HitAll(ctx, play);
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3);
}

public class HuntersDeaths() : MlynarCard(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new MlynarDamageVar(16, ValueProp.Move)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    public override List<(string, string)> Localization => new CardLoc("不必轻描淡写", "造成 {Damage:diff()} 点伤害。消耗。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) => await Hit(ctx, play);
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4);
}

public class IfRotten() : MlynarCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new MlynarDamageVar(9, ValueProp.Move), new DynamicVar("Poise", 2)];
    public override List<(string, string)> Localization => new CardLoc("若也腐朽", "造成 {Damage:diff()} 点伤害。若目标是精英或首领，蓄势 +{Poise}。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Hit(ctx, play);
        if (MlynarRuntime.IsEliteOrBoss(play.Target))
            await MlynarRuntime.GainPoise(ctx, play.Player.Creature, DynamicVars["Poise"].IntValue);
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2);
}

public class LetPass() : MlynarCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new MlynarDamageVar(9, ValueProp.Move)];
    public override List<(string, string)> Localization => new CardLoc("默许", "造成 {Damage:diff()} 点伤害。若未击杀：蓄势 +1，抽 1 张牌。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Hit(ctx, play);
        if (play.Target is { CurrentHp: > 0 })
        {
            await MlynarRuntime.GainPoise(ctx, play.Player.Creature, 1);
            await CardPileCmd.Draw(ctx, 1, play.Player);
        }
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(2);
}

public class TheirOwnRoad() : MlynarCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Cards", 2)];
    public override List<(string, string)> Localization => new CardLoc("她们自己的路", "抽 {Cards} 张牌。你可以弃掉任意张攻击牌，每弃 1 张，蓄势 +1。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CardPileCmd.Draw(ctx, DynamicVars["Cards"].IntValue, play.Player);
        var discarded = await CardSelectCmd.FromHandForDiscard(ctx, play.Player, new(new LocString("characters", "mlynar_discard_attacks"), 0, 99), c => c.Type == CardType.Attack, this);
        var count = discarded?.Count() ?? 0;
        if (count > 0)
            await MlynarRuntime.GainPoise(ctx, play.Player.Creature, count);
    }
    protected override void OnUpgrade() => DynamicVars["Cards"].UpgradeValueBy(1);
}

public class UnfinishedWork() : MlynarCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Poise", 2)];
    public override List<(string, string)> Localization => new CardLoc("未完的公事", "蓄势 +{Poise}。弃 1 张牌。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await MlynarRuntime.GainPoise(ctx, play.Player.Creature, DynamicVars["Poise"].IntValue);
        await CardSelectCmd.FromHandForDiscard(ctx, play.Player, new(new LocString("characters", "mlynar_discard_one"), 1), _ => true, this);
    }
    protected override void OnUpgrade() => DynamicVars["Poise"].UpgradeValueBy(1);
}

public class NoneOfMyBusiness() : MlynarCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override List<(string, string)> Localization => new CardLoc("不关我事", "抽 1 张牌。将手牌 1 张放到抽牌堆底。下个回合抽 1 张牌。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CardPileCmd.Draw(ctx, 1, play.Player);
        var selected = await CardSelectCmd.FromHand(ctx, play.Player, new(new LocString("characters", "mlynar_bottom"), 1), _ => true, this);
        var card = selected?.FirstOrDefault();
        if (card != null)
            await CardPileCmd.Add(card, play.Player.PlayerCombatState.DrawPile, CardPilePosition.Bottom);
        await PowerCmd.Apply<DrawCardsNextTurnPower>(ctx, play.Player.Creature, 1, play.Player.Creature, this);
    }
    protected override void OnUpgrade() { }
}

public class DrawMargaret() : MlynarCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Bonus", 6), new DynamicVar("Hurt", 3)];
    public override List<(string, string)> Localization => new CardLoc("拔剑，玛嘉烈", "你的下一张攻击伤害 +{Bonus}。不进入拔剑。本回合结束时若未攻击，受到 {Hurt} 点伤害。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await PowerCmd.Apply<VigorPower>(ctx, play.Player.Creature, DynamicVars["Bonus"].BaseValue, play.Player.Creature, this);
        await MlynarRuntime.EnsureBrain(ctx, play.Player.Creature);
        MlynarRuntime.Brain(play.Player.Creature).SitPoiseThisTurn -= 0;
    }
    protected override void OnUpgrade() => DynamicVars["Bonus"].UpgradeValueBy(2);
}

public class DoesNotDecide() : MlynarCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(8, ValueProp.Move)];
    public override List<(string, string)> Localization => new CardLoc("说了不算", "解除你的 1 个负面状态。获得 {Block:diff()} 点格挡。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var debuff = play.Player.Creature.Powers.FirstOrDefault(p => p.Type == MegaCrit.Sts2.Core.Entities.Powers.PowerType.Debuff);
        if (debuff != null)
            PowerCmd.Remove(debuff);
        Block(play);
        await Task.CompletedTask;
    }
    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(2);
}

public class Clerk() : MlynarCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override List<(string, string)> Localization => new CardLoc("办事员", "抽 2 张牌。本回合手牌中费用最高的 1 张费用 -1。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CardPileCmd.Draw(ctx, 2, play.Player);
        var card = play.Player.PlayerCombatState.Hand.Cards.OrderByDescending(c => c.EnergyCost.Canonical).FirstOrDefault();
        card?.SetToFreeThisTurn();
    }
}

public class ThatShame() : MlynarCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Poise", 3)];
    public override List<(string, string)> Localization => new CardLoc("那桩耻辱", "蓄势 +{Poise}。本回合你造成的攻击伤害 -3。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await MlynarRuntime.GainPoise(ctx, play.Player.Creature, DynamicVars["Poise"].IntValue);
        await PowerCmd.Apply<WeakPower>(ctx, play.Player.Creature, 1, play.Player.Creature, this);
    }
    protected override void OnUpgrade() => DynamicVars["Poise"].UpgradeValueBy(1);
}

public class FellowTraveler() : MlynarCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Cards", 1)];
    public override List<(string, string)> Localization => new CardLoc("一时同行", "下个回合能量 +1，抽 {Cards} 张牌。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await PowerCmd.Apply<EnergyNextTurnPower>(ctx, play.Player.Creature, 1, play.Player.Creature, this);
        await PowerCmd.Apply<DrawCardsNextTurnPower>(ctx, play.Player.Creature, DynamicVars["Cards"].IntValue, play.Player.Creature, this);
    }
    protected override void OnUpgrade() => DynamicVars["Cards"].UpgradeValueBy(1);
}

public class FamilyLetter() : MlynarCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BaseLib.Cards.Variables.ScryVar(3)];
    public override List<(string, string)> Localization => new CardLoc("家书", "预见 {Scry}。你可以将弃牌堆的 1 张牌放到抽牌堆顶。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await ScryCmd.Execute(ctx, play.Player, DynamicVars["Scry"].IntValue);
        var discard = play.Player.PlayerCombatState.DiscardPile;
        if (discard.IsEmpty) return;
        var selected = await CardSelectCmd.FromCombatPile(ctx, discard, play.Player, new(new LocString("characters", "mlynar_top"), 1));
        var card = selected?.FirstOrDefault();
        if (card != null)
            await CardPileCmd.Add(card, play.Player.PlayerCombatState.DrawPile, CardPilePosition.Top);
    }
    protected override void OnUpgrade() => DynamicVars["Scry"].UpgradeValueBy(1);
}

public class DoesNotKnowTheRoad() : MlynarCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BaseLib.Cards.Variables.ScryVar(2), new BlockVar(6, ValueProp.Move)];
    public override List<(string, string)> Localization => new CardLoc("不识路", "预见 {Scry}。获得 {Block:diff()} 点格挡。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await ScryCmd.Execute(ctx, play.Player, DynamicVars["Scry"].IntValue);
        Block(play);
    }
    protected override void OnUpgrade()
    {
        DynamicVars["Scry"].UpgradeValueBy(1);
        DynamicVars.Block.UpgradeValueBy(2);
    }
}

public class OnePaper() : MlynarCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Poise", 4)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    public override List<(string, string)> Localization => new CardLoc("一纸辞呈", "蓄势 +{Poise}。抽 2 张牌。消耗。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await MlynarRuntime.GainPoise(ctx, play.Player.Creature, DynamicVars["Poise"].IntValue);
        await CardPileCmd.Draw(ctx, 2, play.Player);
    }
    protected override void OnUpgrade() => DynamicVars["Poise"].UpgradeValueBy(1);
}

public class BeforeTheMistake() : MlynarCard(2, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(12, ValueProp.Move)];
    public override List<(string, string)> Localization => new CardLoc("大错之前", "获得 {Block:diff()} 点格挡。指定 1 名敌人，施加 1 层虚弱。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        CreatureCmd.GainBlock(play.Player.Creature, Blk, default, play, false);
        if (play.Target != null)
            await PowerCmd.Apply<WeakPower>(ctx, play.Target, 1, play.Player.Creature, this);
    }
    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(4);
}

public class NoLeaveCard() : MlynarCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    public override List<(string, string)> Localization => new CardLoc("不曾请假", "每个鞘中回合开始，额外蓄势 +1。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await PowerCmd.Apply<NoLeavePower>(ctx, play.Player.Creature, 1, play.Player.Creature, this);
        await MlynarRuntime.EnsureBrain(ctx, play.Player.Creature);
        MlynarRuntime.Brain(play.Player.Creature).ExtraSheathStartPoise += 1;
    }
}

public class AfterSheatheCard() : MlynarCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Cards", 1)];
    public override List<(string, string)> Localization => new CardLoc("收回之后", "每当你收剑，抽 {Cards} 张牌。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await PowerCmd.Apply<AfterSheathePower>(ctx, play.Player.Creature, DynamicVars["Cards"].IntValue, play.Player.Creature, this);
        await MlynarRuntime.EnsureBrain(ctx, play.Player.Creature);
        MlynarRuntime.Brain(play.Player.Creature).DrawOnSheathe += DynamicVars["Cards"].IntValue;
    }
    protected override void OnUpgrade() => DynamicVars["Cards"].UpgradeValueBy(1);
}

public class TideOfLightCard() : MlynarCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Dmg", 3)];
    public override List<(string, string)> Localization => new CardLoc("光潮", "在拔剑中时，你的攻击伤害 +{Dmg}。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await PowerCmd.Apply<TideOfLightPower>(ctx, play.Player.Creature, DynamicVars["Dmg"].IntValue, play.Player.Creature, this);
        await MlynarRuntime.EnsureBrain(ctx, play.Player.Creature);
        MlynarRuntime.Brain(play.Player.Creature).DrawnAttackBonus += DynamicVars["Dmg"].IntValue;
    }
    protected override void OnUpgrade() => DynamicVars["Dmg"].UpgradeValueBy(1);
}

public class BenchCard() : MlynarCard(2, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(7, ValueProp.Move)];
    public override List<(string, string)> Localization => new CardLoc("长椅", "鞘中回合结束时，若本回合未打出攻击，获得 {Block:diff()} 点格挡。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await PowerCmd.Apply<BenchPower>(ctx, play.Player.Creature, DynamicVars.Block.IntValue, play.Player.Creature, this);
        await MlynarRuntime.EnsureBrain(ctx, play.Player.Creature);
        MlynarRuntime.Brain(play.Player.Creature).SitBlock += DynamicVars.Block.IntValue;
    }
    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3);
}

public class NeedNotReturnCard() : MlynarCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    public override List<(string, string)> Localization => new CardLoc("不必回岗", "你每次受到攻击伤害，蓄势 +1。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await PowerCmd.Apply<NeedNotReturnPower>(ctx, play.Player.Creature, 1, play.Player.Creature, this);
        await MlynarRuntime.EnsureBrain(ctx, play.Player.Creature);
        MlynarRuntime.Brain(play.Player.Creature).HitPoise += 1;
    }
}
