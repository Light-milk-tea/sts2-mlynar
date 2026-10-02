using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
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
        "最多 10 层。拔剑期间，每层使攻击伤害 +10%。主动退出时清零并抽 3 张牌；回合结束自动退出时清零，下回合抽 3 张牌。",
        "最多 10 层。拔剑期间，每层使攻击伤害 +10%。主动退出时清零并抽 3 张牌；回合结束自动退出时清零，下回合抽 3 张牌。");

    public bool AttackedThisTurn;
    public bool CannotDrawThisTurn;
    public bool SheatheResolvedThisEnd;
    public int EndTurnPoiseIfNoAttack;
    public int EndTurnDamage;
    public int EndTurnDamageIfNoAttack;
    public int HoldPoise;

    public void SetPoints(int value)
    {
        var next = Math.Clamp(value, 0, MlynarRuntime.PoiseCap);
        if (next == Points)
            return;
        Points = next;
        InvokeDisplayAmountChanged();
    }

    public override Task AfterPlayerTurnStart(PlayerChoiceContext ctx, Player player)
    {
        if (player.Creature != Owner) return Task.CompletedTask;
        AttackedThisTurn = false;
        CannotDrawThisTurn = false;
        SheatheResolvedThisEnd = false;
        return Task.CompletedTask;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext ctx, CombatSide side, IEnumerable<Creature> creatures)
    {
        if (Owner == null || !creatures.Contains(Owner)) return;

        if (MlynarRuntime.IsDrawn(Owner))
        {
            var drawn = Owner.GetPower<DrawnPower>()!;
            if (drawn.Amount <= 1)
                await MlynarRuntime.Sheathe(ctx, Owner, auto: true);
            else
                await PowerCmd.ModifyAmount(ctx, drawn, -1, Owner, null);
        }

        SheatheResolvedThisEnd = true;

        if (!AttackedThisTurn && EndTurnPoiseIfNoAttack > 0)
            await MlynarRuntime.GainPoise(ctx, Owner, EndTurnPoiseIfNoAttack);
        if (HoldPoise > 0)
        {
            await MlynarRuntime.GainPoise(ctx, Owner, HoldPoise);
            HoldPoise = 0;
        }
        if (!AttackedThisTurn && EndTurnDamageIfNoAttack > 0)
            await CreatureCmd.Damage(ctx, Owner, EndTurnDamageIfNoAttack, ValueProp.Move, Owner);
        if (EndTurnDamage > 0)
            await CreatureCmd.Damage(ctx, Owner, EndTurnDamage, ValueProp.Move, Owner);

        EndTurnPoiseIfNoAttack = 0;
        EndTurnDamage = 0;
        EndTurnDamageIfNoAttack = 0;
    }

    public override Task AfterCardPlayed(PlayerChoiceContext ctx, CardPlay cardPlay)
    {
        if (cardPlay.Player.Creature == Owner && cardPlay.Card.Type == CardType.Attack)
            AttackedThisTurn = true;
        return Task.CompletedTask;
    }

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (Owner == null || dealer != Owner || cardSource == null) return 1m;
        if (!props.IsPoweredAttack()) return 1m;
        if (cardSource is IIgnorePoise) return 1m;

        var drawn = MlynarRuntime.IsDrawn(Owner);
        if (!drawn)
        {
            if (cardSource is not IDrawSwordCard || CannotDrawThisTurn)
                return 1m;
        }

        var poise = Points;
        if (poise <= 0) return 1m;
        return 1m + poise * 0.1m;
    }
}
