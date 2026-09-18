using BaseLib.Abstracts;
using BaseLib.Commands;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using MlynarMod.MlynarModCode.Core;
using MlynarMod.MlynarModCode.Powers;

namespace MlynarMod.MlynarModCode.Cards;

public class UnlitGlory() : MlynarCard(3, CardType.Skill, CardRarity.Rare, TargetType.AllEnemies), IDrawSwordCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new MlynarDamageVar(12, ValueProp.Move), new DynamicVar("True", 8)];
    public override List<(string, string)> Localization => new CardLoc("未照耀的荣光", "进入拔剑（2 回合）。对所有敌人造成 {Damage:diff()} 点伤害，再造成 {True} 点真实伤害。这次拔剑中每击杀 1 名敌人，蓄势 -1。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await MlynarRuntime.EnterDrawn(ctx, play.Player, 2, this);
        await HitAll(ctx, play);
        await CreatureCmd.Damage(ctx, play.Player.Creature.CombatState!.HittableEnemies, DynamicVars["True"].BaseValue, ValueProp.Unpowered, play.Player.Creature, this, play);
        var brain = MlynarRuntime.Brain(play.Player.Creature);
        if (brain.KilledThisWindow && play.Player.Creature.HasPower<DrawnPower>())
        {
            var drawn = play.Player.Creature.GetPower<DrawnPower>();
            drawn.StoredPoise = Math.Max(0, drawn.StoredPoise - 1);
            await MlynarRuntime.GainPoise(ctx, play.Player.Creature, -1);
        }
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(4);
        DynamicVars["True"].UpgradeValueBy(2);
    }
}

public class ManInTheSheathCard() : MlynarCard(1, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    public override List<(string, string)> Localization => new CardLoc("鞘中人", "每场战斗开始时，蓄势 +3。本场第一次拔剑持续 +1 回合。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await PowerCmd.Apply<ManInSheathPower>(ctx, play.Player.Creature, 1, play.Player.Creature, this);
        await MlynarRuntime.GainPoise(ctx, play.Player.Creature, 3);
        await MlynarRuntime.EnsureBrain(ctx, play.Player.Creature);
        MlynarRuntime.Brain(play.Player.Creature).FirstDrawExtraTurn = true;
    }
    protected override void OnUpgrade() => AddKeyword(CardKeyword.Innate);
}

public class FearNoDarkCard() : MlynarCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(8, ValueProp.Move)];
    public override List<(string, string)> Localization => new CardLoc("不畏苦暗", "每当你收剑：若这次拔剑中受伤或没有击杀，保留一半蓄势，并获得 {Block:diff()} 点格挡。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await PowerCmd.Apply<FearNoDarkPower>(ctx, play.Player.Creature, 1, play.Player.Creature, this);
        await MlynarRuntime.EnsureBrain(ctx, play.Player.Creature);
        var brain = MlynarRuntime.Brain(play.Player.Creature);
        brain.FearNoDark = true;
        brain.FearNoDarkBlock = DynamicVars.Block.IntValue;
    }
    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(4);
}

public class TenYearBalloonCard() : MlynarCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Per", 4)];
    public override List<(string, string)> Localization => new CardLoc("十年气球人", "每个完整鞘中回合结束，获得 1 层灌满。进入拔剑时每层兑成 {Per} 点伤害。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await PowerCmd.Apply<TenYearBalloonPower>(ctx, play.Player.Creature, 1, play.Player.Creature, this);
        await MlynarRuntime.EnsureBrain(ctx, play.Player.Creature);
        MlynarRuntime.Brain(play.Player.Creature).BalloonPerStack = DynamicVars["Per"].IntValue;
    }
    protected override void OnUpgrade() => DynamicVars["Per"].UpgradeValueBy(1);
}

public class GoldenWrathCard() : MlynarCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Stacks", 2)];
    public override List<(string, string)> Localization => new CardLoc("金色的愠怒", "鞘中攻击施加 {Stacks} 层愠怒。进入拔剑时全部结算为真实伤害。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await PowerCmd.Apply<GoldenWrathPower>(ctx, play.Player.Creature, DynamicVars["Stacks"].IntValue, play.Player.Creature, this);
        await MlynarRuntime.EnsureBrain(ctx, play.Player.Creature);
        MlynarRuntime.Brain(play.Player.Creature).WrathPerSheathAttack += DynamicVars["Stacks"].IntValue;
    }
    protected override void OnUpgrade() => DynamicVars["Stacks"].UpgradeValueBy(1);
}

public class UnworthyOfTheSword() : MlynarCard(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy), IDrawSwordCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new MlynarDamageVar(4, ValueProp.Move), new DynamicVar("Alt", 6)];
    public override List<(string, string)> Localization => new CardLoc("不配我拔剑", "蓄势少于 5：造成 {Damage:diff()} 点伤害，不进入拔剑。否则进入拔剑（1 回合），造成 {Alt} + 当前蓄势 点伤害。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var poise = MlynarRuntime.GetPoise(play.Player.Creature);
        if (poise < 5)
        {
            await Hit(ctx, play);
            return;
        }

        await MlynarRuntime.EnterDrawn(ctx, play.Player, 1, this);
        await DamageCmd.Attack(DynamicVars["Alt"].BaseValue + poise).FromCard(this, play).Targeting(play.Target!).Execute(ctx);
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars["Alt"].UpgradeValueBy(2);
    }
}

public class IAmNoKnight() : MlynarCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(0, ValueProp.Move)];
    public override List<(string, string)> Localization => new CardLoc("我并非骑士", "解除你的所有负面状态。本回合你可以提前收剑，且本次收剑不重置。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        foreach (var debuff in play.Player.Creature.Powers.Where(p => p.Type == MegaCrit.Sts2.Core.Entities.Powers.PowerType.Debuff).ToArray())
            await PowerCmd.Remove(debuff);
        await MlynarRuntime.EnsureBrain(ctx, play.Player.Creature);
        var brain = MlynarRuntime.Brain(play.Player.Creature);
        brain.ForceNoResetOnce = true;
        if (MlynarRuntime.IsDrawn(play.Player.Creature))
            play.Player.Creature.GetPower<DrawnPower>().AllowEarlySheathe = true;
        if (DynamicVars.Block.IntValue > 0)
            CreatureCmd.GainBlock(play.Player.Creature, DynamicVars.Block.BaseValue, default, play, false);
        await Task.CompletedTask;
    }
    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(8);
}

public class TitleOfGrandKnight() : MlynarCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Poise", 6)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    public override List<(string, string)> Localization => new CardLoc("长骑之名", "蓄势 +{Poise}。本场你无法再获得力量。消耗。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await MlynarRuntime.GainPoise(ctx, play.Player.Creature, DynamicVars["Poise"].IntValue);
        await MlynarRuntime.EnsureBrain(ctx, play.Player.Creature);
        MlynarRuntime.Brain(play.Player.Creature).NoStrengthFocus = true;
    }
    protected override void OnUpgrade() => DynamicVars["Poise"].UpgradeValueBy(2);
}

public class LetterPutBack() : MlynarCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Bonus", 4)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    public override List<(string, string)> Localization => new CardLoc("装回去的信", "获得 1 张伤口。下一次拔剑：收剑时不重置，且攻击伤害 +{Bonus}。消耗。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await CardPileCmd.AddGeneratedCardToCombat(ModelDb.Card<Wound>(), PileType.Hand, play.Player);
        await MlynarRuntime.EnsureBrain(ctx, play.Player.Creature);
        var brain = MlynarRuntime.Brain(play.Player.Creature);
        brain.NextDrawNoReset = true;
        brain.NextDrawExtraDamage += DynamicVars["Bonus"].IntValue;
    }
    protected override void OnUpgrade() => DynamicVars["Bonus"].UpgradeValueBy(2);
}

public class WhyWasItMeCard() : MlynarCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    public override List<(string, string)> Localization => new CardLoc("留下的是我", "每当你收剑：抽 2 张牌，下个回合能量 +1。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await PowerCmd.Apply<WhyMePower>(ctx, play.Player.Creature, 1, play.Player.Creature, this);
        await MlynarRuntime.EnsureBrain(ctx, play.Player.Creature);
        var brain = MlynarRuntime.Brain(play.Player.Creature);
        brain.DrawOnSheathe += 2;
        brain.EnergyOnSheathe += 1;
    }
    protected override void OnUpgrade()
    {
        // 升到 1 费：CustomCard 能量升级视编译结果再调
    }
}

public class ObscureWandererCard() : MlynarCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BaseLib.Cards.Variables.ScryVar(4)];
    public override List<(string, string)> Localization => new CardLoc("日暮寻路", "预见 {Scry}。弃掉的每张牌蓄势 +1。你可以将弃牌堆 1 张牌放到抽牌堆顶。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var result = await ScryCmd.Execute(ctx, play.Player, DynamicVars["Scry"].IntValue);
        if (result.Discarded.Count > 0)
            await MlynarRuntime.GainPoise(ctx, play.Player.Creature, result.Discarded.Count);
        var discard = play.Player.PlayerCombatState.DiscardPile;
        if (discard.IsEmpty) return;
        var selected = await CardSelectCmd.FromCombatPile(ctx, discard, play.Player, new(new LocString("characters", "mlynar_top"), 1));
        var card = selected?.FirstOrDefault();
        if (card != null)
            await CardPileCmd.Add(card, play.Player.PlayerCombatState.DrawPile, CardPilePosition.Top);
    }
    protected override void OnUpgrade() => DynamicVars["Scry"].UpgradeValueBy(1);
}

public class WildernessCard() : MlynarCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Poise", 2), new BlockVar(10, ValueProp.Move)];
    public override List<(string, string)> Localization => new CardLoc("没有灯光的荒野", "鞘中回合结束时若未打出攻击：蓄势 +{Poise}，获得 {Block:diff()} 点格挡。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await PowerCmd.Apply<WildernessPower>(ctx, play.Player.Creature, 1, play.Player.Creature, this);
        await MlynarRuntime.EnsureBrain(ctx, play.Player.Creature);
        var brain = MlynarRuntime.Brain(play.Player.Creature);
        brain.SitPoise += DynamicVars["Poise"].IntValue;
        brain.SitBlock += DynamicVars.Block.IntValue;
    }
    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(4);
}

public class GiveMeASword() : MlynarCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    public override List<(string, string)> Localization => new CardLoc("给我一把剑", "下一次拔剑持续 +1 回合。消耗。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await MlynarRuntime.EnsureBrain(ctx, play.Player.Creature);
        MlynarRuntime.Brain(play.Player.Creature).NextDrawExtraTurn = true;
        await Task.CompletedTask;
    }
}

public class SpearInTheRain() : MlynarCard(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new MlynarDamageVar(16, ValueProp.Move), new DynamicVar("Self", 4), new DynamicVar("Poise", 3)];
    public override List<(string, string)> Localization => new CardLoc("雨落枪尖", "对所有敌人造成 {Damage:diff()} 点伤害。若击杀：你受到 {Self} 点伤害。若没有击杀：蓄势 +{Poise}。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var before = play.Player.Creature.CombatState!.HittableEnemies.Count();
        await HitAll(ctx, play);
        var after = play.Player.Creature.CombatState!.HittableEnemies.Count();
        if (after < before)
            await CreatureCmd.Damage(ctx, play.Player.Creature, DynamicVars["Self"].BaseValue, ValueProp.Unpowered, play.Player.Creature);
        else
            await MlynarRuntime.GainPoise(ctx, play.Player.Creature, DynamicVars["Poise"].IntValue);
    }
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(4);
}

public class LetterOpener() : MlynarCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    public override List<(string, string)> Localization => new CardLoc("拆信刀", "蓄势 +4。消耗。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        await MlynarRuntime.GainPoise(ctx, play.Player.Creature, 4);
}

public class StandAloneCard() : MlynarCard(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(16, ValueProp.Move), new DynamicVar("Thorns", 4)];
    public override List<(string, string)> Localization => new CardLoc("独自拦下", "获得 {Block:diff()} 点格挡。本场在鞘中时，受击反弹 {Thorns} 点真实伤害。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        Block(play);
        await PowerCmd.Apply<StandAlonePower>(ctx, play.Player.Creature, DynamicVars["Thorns"].IntValue, play.Player.Creature, this);
        await MlynarRuntime.EnsureBrain(ctx, play.Player.Creature);
        MlynarRuntime.Brain(play.Player.Creature).SheathThorns += DynamicVars["Thorns"].IntValue;
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4);
        DynamicVars["Thorns"].UpgradeValueBy(2);
    }
}

public class KazimierzStillHasARoad() : MlynarCard(1, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    public override List<(string, string)> Localization => new CardLoc("卡西米尔仍然有路", "本场第一次生命降至半数或以下：蓄势变为上限。本回合不能进入拔剑。");
    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await PowerCmd.Apply<RoadRemainsPower>(ctx, play.Player.Creature, 1, play.Player.Creature, this);
        await Task.CompletedTask;
    }
}
