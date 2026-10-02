using BaseLib.Abstracts;
using BaseLib.Cards.Variables;
using BaseLib.Commands;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.ValueProps;
using MlynarMod.MlynarModCode.Core;
using MlynarMod.MlynarModCode.Powers;

namespace MlynarMod.MlynarModCode.Cards;

public class LightlessKnight() : MlynarCard(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy), ISheathOnly, IIgnorePoise
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(14, ValueProp.Move), new DynamicVar("Poise", 5)];
    public override List<(string, string)> Localization => new CardLoc("无光骑士", "只能在鞘中打出。造成 {Damage:diff()} 点伤害。此伤害不受蓄势增伤。蓄势 +{Poise}。");

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await Hit(ctx, play);
        await MlynarRuntime.GainPoise(ctx, play.Player.Creature, DynamicVars["Poise"].IntValue);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars["Poise"].UpgradeValueBy(2);
    }
}

public class UnlitGlory() : MlynarCard(3, CardType.Skill, CardRarity.Rare, TargetType.AllEnemies), IDrawSwordCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10, ValueProp.Move), new DynamicVar("True", 8)];
    public override List<(string, string)> Localization => new CardLoc("未照耀的荣光", "进入拔剑（2 回合）。对所有敌人造成 {Damage:diff()} 点伤害两次，再造成 {True} 点真实伤害。已在拔剑中则不能打出。");

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await MlynarRuntime.EnterDrawn(ctx, play.Player, 2, this);
        await HitAll(ctx, play, 2);
        var creature = play.Player.Creature;
        await CreatureCmd.Damage(ctx, creature.CombatState!.HittableEnemies, DynamicVars["True"].BaseValue, ValueProp.Unpowered, creature, this, play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars["True"].UpgradeValueBy(2);
    }
}

public class TitleOfGrandKnight() : MlynarCard(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Poise", 8)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    public override List<(string, string)> Localization => new CardLoc("长骑之名", "蓄势 +{Poise}。消耗。");

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        await MlynarRuntime.GainPoise(ctx, play.Player.Creature, DynamicVars["Poise"].IntValue);

    protected override void OnUpgrade() => DynamicVars["Poise"].UpgradeValueBy(2);
}

public class ObscureWandererCard() : MlynarCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new ScryVar(4)];
    public override List<(string, string)> Localization => new CardLoc("日暮寻路", "预见 {Scry}。你弃掉的每张牌：蓄势 +1。你可以将弃牌堆的 1 张牌放到抽牌堆顶。");

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var result = await ScryCmd.Execute(ctx, play.Player, DynamicVars["Scry"].IntValue);
        if (result.Discarded.Count > 0)
            await MlynarRuntime.GainPoise(ctx, play.Player.Creature, result.Discarded.Count);
        await OfferTopdeck(ctx, play);
    }

    protected override void OnUpgrade() => DynamicVars["Scry"].UpgradeValueBy(1);
}

public class LetterOpener() : MlynarCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Poise", 4)];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    public override List<(string, string)> Localization => new CardLoc("拆信刀", "选择：蓄势 +{Poise}；或消耗手牌中 1 张牌，并升级另 1 张手牌。");

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        var hand = play.Player.PlayerCombatState?.Hand.Cards.Where(c => c != this).ToList() ?? [];
        if (hand.Count < 2)
        {
            await MlynarRuntime.GainPoise(ctx, play.Player.Creature, DynamicVars["Poise"].IntValue);
            return;
        }

        var picked = await CardSelectCmd.FromHand(
            ctx,
            play.Player,
            new CardSelectorPrefs(new LocString("characters", "mlynar_letter_exhaust"), 0, 1),
            c => c != this,
            this);
        var exhaust = picked?.FirstOrDefault();
        if (exhaust == null)
        {
            await MlynarRuntime.GainPoise(ctx, play.Player.Creature, DynamicVars["Poise"].IntValue);
            return;
        }

        await CardCmd.Exhaust(ctx, exhaust);
        var upgrade = await CardSelectCmd.FromHandForUpgrade(ctx, play.Player, this);
        if (upgrade != null)
            CardCmd.Upgrade(upgrade, CardPreviewStyle.HorizontalLayout);
    }

    protected override void OnUpgrade() => RemoveKeyword(CardKeyword.Exhaust);
}

public class VastRoar() : MlynarCard(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(16, ValueProp.Move), new DynamicVar("Thorns", 4)];
    public override List<(string, string)> Localization => new CardLoc("苍茫怒号", "获得 {Block:diff()} 点格挡。本场战斗在鞘中时，你每次受到攻击伤害，对来源造成 {Thorns} 点真实伤害。");

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        Block(play);
        await PowerCmd.Apply<VastRoarPower>(ctx, play.Player.Creature, DynamicVars["Thorns"].BaseValue, play.Player.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(4);
        DynamicVars["Thorns"].UpgradeValueBy(2);
    }
}

public class NearlLongNight() : MlynarCard(2, CardType.Skill, CardRarity.Rare, TargetType.AllEnemies)
{
    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new DamageVar(12, ValueProp.Move),
        new BlockVar(8, ValueProp.Move),
        new DynamicVar("SheathBlock", 16),
        new DynamicVar("Poise", 2)
    ];

    public override List<(string, string)> Localization => new CardLoc(
        "长夜临光",
        "若在拔剑中：对所有敌人造成 {Damage:diff()} 点伤害，获得 {Block:diff()} 点格挡。若在鞘中：获得 {SheathBlock} 点格挡，蓄势 +{Poise}，本回合不能进入拔剑。");

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        if (MlynarRuntime.IsDrawn(play.Player.Creature))
        {
            await HitAll(ctx, play);
            Block(play);
            return;
        }

        GainBlock(play, DynamicVars["SheathBlock"].BaseValue);
        await MlynarRuntime.GainPoise(ctx, play.Player.Creature, DynamicVars["Poise"].IntValue);
        await MlynarRuntime.ForbidDrawThisTurn(ctx, play.Player.Creature);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(12);
        DynamicVars.Block.UpgradeValueBy(2);
        DynamicVars["SheathBlock"].UpgradeValueBy(4);
        DynamicVars["Poise"].UpgradeValueBy(1);
    }
}

public class ManInTheSheathCard() : MlynarCard(1, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Poise", 2)];
    public override List<(string, string)> Localization => new CardLoc("鞘中人", "每回合开始时，蓄势 +{Poise}。");

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        await PowerCmd.Apply<ManInSheathPower>(ctx, play.Player.Creature, DynamicVars["Poise"].IntValue, play.Player.Creature, this);

    protected override void OnUpgrade() => AddKeyword(CardKeyword.Innate);
}

public class FearNoDarkCard() : MlynarCard(2, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(10, ValueProp.Move)];
    public override List<(string, string)> Localization => new CardLoc("不畏苦暗", "每当你退出拔剑，获得 {Block:diff()} 点格挡。");

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        await PowerCmd.Apply<FearNoDarkPower>(ctx, play.Player.Creature, DynamicVars.Block.IntValue, play.Player.Creature, this);

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3);
}

public class GoldenWrathCard() : MlynarCard(3, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    public override List<(string, string)> Localization => new CardLoc("金色的愠怒", "你在鞘中的每次攻击对目标施加等量数值的愠怒。进入拔剑时，所有敌人的愠怒结算为等量真实伤害，然后清零。");

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) =>
        await PowerCmd.Apply<GoldenWrathPower>(ctx, play.Player.Creature, 1, play.Player.Creature, this);

    protected override void OnUpgrade() => EnergyCost.UpgradeBy(-1);
}
