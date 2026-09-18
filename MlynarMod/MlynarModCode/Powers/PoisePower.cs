using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MlynarMod.MlynarModCode.Cards;
using MlynarMod.MlynarModCode.Core;
using MlynarMod.MlynarModCode.Extensions;

namespace MlynarMod.MlynarModCode.Powers;

public class PoisePower : MlynarPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public int Points;

    public override int DisplayAmount => Points;
    public override string CustomPackedIconPath => "poisepower.png".PowerImagePath();
    public override string CustomBigIconPath => "poisepower.png".BigPowerImagePath();

    public override List<(string, string)> Localization => new PowerLoc(
        "蓄势",
        "最多 8 点。鞘中每回合开始 +1。进入拔剑时把当前蓄势兑成窗口伤害并清零。",
        "最多 8 点。鞘中每回合开始 +1。进入拔剑时把当前蓄势兑成窗口伤害并清零。");

    public bool AttackedThisTurn;
    public bool DamagedThisWindow;
    public bool KilledThisWindow;
    public bool CannotDrawThisTurn;
    public bool FirstDrawExtraTurn;
    public bool NextDrawExtraTurn;
    public bool NextDrawNoReset;
    public bool PendingNoResetOnKill;
    public bool PendingAllowEarlySheathe;
    public bool ForceNoResetOnce;
    public bool FearNoDark;
    public int FearNoDarkBlock = 8;
    public int NextSheatheKeepPoints;
    public int NextDrawExtraDamage;
    public int ExtraWindowFlat;
    public int ExtraSheathStartPoise;
    public int BlockOnSheathe;
    public int DrawOnSheathe;
    public int EnergyOnSheathe;
    public int BalloonStacks;
    public int BalloonPerStack = 4;
    public bool BalloonAsBlock;
    public int WrathPerSheathAttack;
    public int SitPoise;
    public int SitPoiseThisTurn;
    public int SitBlock;
    public int NextTurnBonusPoise;
    public int IncomingReduceThisTurn;
    public int NewspaperPoise;
    public bool NewspaperUsedThisTurn;
    public int HitPoise;
    public int WandererDamage;
    public int WandererCrowdDamage;
    public int WandererReduce;
    public int DrawnAttackBonus;
    public int Thorns;
    public int SheathThorns;
    public bool Taunt;
    public bool NoStrengthFocus;
    public void SetPoints(int value)
    {
        var next = Math.Clamp(value, 0, MlynarRuntime.PoiseCap);
        if (next == Points)
            return;
        Points = next;
        InvokeDisplayAmountChanged();
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext ctx, Player player)
    {
        if (player.Creature != Owner) return;
        AttackedThisTurn = false;
        NewspaperUsedThisTurn = false;
        CannotDrawThisTurn = false;
        IncomingReduceThisTurn = 0;
        if (!MlynarRuntime.IsDrawn(Owner))
            await MlynarRuntime.GainPoise(ctx, Owner, 1 + ExtraSheathStartPoise + NextTurnBonusPoise);
        NextTurnBonusPoise = 0;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext ctx, CombatSide side, IEnumerable<Creature> creatures)
    {
        if (Owner == null || !creatures.Contains(Owner)) return;

        if (!MlynarRuntime.IsDrawn(Owner) && !AttackedThisTurn)
        {
            var sit = SitPoise + SitPoiseThisTurn;
            SitPoiseThisTurn = 0;
            if (sit > 0)
                await MlynarRuntime.GainPoise(ctx, Owner, sit);
            if (SitBlock > 0)
                CreatureCmd.GainBlock(Owner, SitBlock, default, null, false);
            if (BalloonPerStack > 0 && Owner.HasPower<TenYearBalloonPower>())
                BalloonStacks += 1;
        }

        if (MlynarRuntime.IsDrawn(Owner))
        {
            var drawn = Owner.GetPower<DrawnPower>();
            await PowerCmd.ModifyAmount(ctx, drawn, -1, Owner, null);
            if (drawn.Amount <= 0)
                await MlynarRuntime.Sheathe(ctx, Owner);
        }
    }

    public override Task AfterCardPlayed(PlayerChoiceContext ctx, CardPlay cardPlay)
    {
        if (cardPlay.Player.Creature == Owner && cardPlay.Card.Type == CardType.Attack)
            AttackedThisTurn = true;
        return Task.CompletedTask;
    }

    public override Task AfterDamageReceived(PlayerChoiceContext ctx, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? card)
    {
        if (target != Owner) return Task.CompletedTask;
        if (result.UnblockedDamage <= 0) return Task.CompletedTask;
        if (MlynarRuntime.IsDrawn(Owner))
            DamagedThisWindow = true;
        if (Owner.HasPower<RoadRemainsPower>() && !Owner.GetPower<RoadRemainsPower>().Used
            && Owner.CurrentHp * 2 <= Owner.MaxHp)
        {
            Owner.GetPower<RoadRemainsPower>().Used = true;
            CannotDrawThisTurn = true;
            _ = MlynarRuntime.SetPoise(ctx, Owner, MlynarRuntime.PoiseCap);
        }
        if (HitPoise > 0)
            _ = MlynarRuntime.GainPoise(ctx, Owner, HitPoise);
        var thorns = Thorns + (MlynarRuntime.IsDrawn(Owner) ? 0 : SheathThorns);
        if (thorns > 0 && dealer != null && dealer != Owner)
            _ = CreatureCmd.Damage(ctx, dealer, thorns, ValueProp.Unpowered, Owner);
        return Task.CompletedTask;
    }

    public override Task AfterAttack(PlayerChoiceContext ctx, AttackCommand attack)
    {
        if (attack.Attacker != Owner) return Task.CompletedTask;
        var hits = attack.Results.SelectMany(r => r).ToList();
        if (hits.Any(r => r.WasTargetKilled))
            KilledThisWindow = true;

        if (!MlynarRuntime.IsDrawn(Owner) && WrathPerSheathAttack > 0 && attack.CardPlay?.Card.Type == CardType.Attack)
        {
            foreach (var result in hits)
            {
                if (result.Receiver == null || result.Receiver == Owner) continue;
                _ = PowerCmd.Apply<WrathMarkPower>(ctx, result.Receiver, WrathPerSheathAttack, Owner, attack.CardPlay?.Card);
            }
        }

        return Task.CompletedTask;
    }

    public override decimal ModifyDamageAdditive(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (Owner == null) return 0;
        if (dealer != null && dealer != Owner) return 0;
        if (!props.IsPoweredAttack()) return 0;

        var bonus = MlynarRuntime.AttackBonus(Owner, cardSource);
        if (bonus != 0 && cardPlay == null)
            MlynarRuntime.PreviewBonusApplied = true;
        return bonus;
    }

    public override decimal ModifyDamageMultiplicative(
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (target == Owner && IncomingReduceThisTurn > 0 && dealer != Owner)
        {
            var reduced = Math.Max(0, (int)amount - IncomingReduceThisTurn);
            return amount == 0 ? 1m : reduced / amount;
        }

        if (target != Owner || WandererReduce <= 0) return 1m;
        var crowd = Owner.CombatState?.HittableEnemies.Count() ?? 0;
        if (crowd < 3) return 1m;
        var wandererReduced = Math.Max(0, (int)amount - WandererReduce);
        return amount == 0 ? 1m : wandererReduced / amount;
    }
}
