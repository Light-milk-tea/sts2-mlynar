using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MlynarMod.MlynarModCode.Core;

namespace MlynarMod.MlynarModCode.Cards;

public class RangerSwordsmanship() : MlynarCard(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new MlynarDamageVar(6, ValueProp.Move)];
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Strike];
    public override List<(string, string)> Localization => new CardLoc("打击", "造成 {Damage:diff()} 点伤害。");

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play) => await Hit(ctx, play);
    protected override void OnUpgrade() => DynamicVars.Damage.UpgradeValueBy(3);
}

public class SelfContained() : MlynarCard(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5, ValueProp.Move)];
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend];
    public override List<(string, string)> Localization => new CardLoc("防御", "获得 {Block:diff()} 点格挡。");

    protected override Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        Block(play);
        return Task.CompletedTask;
    }

    protected override void OnUpgrade() => DynamicVars.Block.UpgradeValueBy(3);
}

public class HoneSword() : MlynarCard(1, CardType.Skill, CardRarity.Basic, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Poise", 2)];
    public override List<(string, string)> Localization => new CardLoc("磨剑", "蓄势 +{Poise}。抽 1 张牌。");

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await MlynarRuntime.GainPoise(ctx, play.Player.Creature, DynamicVars["Poise"].IntValue);
        await MegaCrit.Sts2.Core.Commands.CardPileCmd.Draw(ctx, 1, play.Player);
    }

    protected override void OnUpgrade() => DynamicVars["Poise"].UpgradeValueBy(1);
}

public class UnvoicedFury() : MlynarCard(2, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy), IDrawSwordCard
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new MlynarDamageVar(9, ValueProp.Move), new BlockVar(6, ValueProp.Move)];
    public override List<(string, string)> Localization => new CardLoc("未声张的怒火", "进入拔剑（1 回合）。造成 {Damage:diff()} 点伤害。获得 {Block:diff()} 点格挡。");

    protected override async Task OnPlay(PlayerChoiceContext ctx, CardPlay play)
    {
        await MlynarRuntime.EnterDrawn(ctx, play.Player, 1, this);
        await Hit(ctx, play);
        Block(play);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
        DynamicVars.Block.UpgradeValueBy(2);
    }
}
