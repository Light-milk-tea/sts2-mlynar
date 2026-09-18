using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using MlynarMod.MlynarModCode.Cards;
using MlynarMod.MlynarModCode.Powers;

namespace MlynarMod.MlynarModCode.Core;

public static class MlynarRuntime
{
    public const int PoiseCap = 8;

    public static int GetPoise(Creature creature) =>
        creature.HasPower<PoisePower>() ? creature.GetPower<PoisePower>().Points : 0;

    public static bool IsDrawn(Creature creature) => creature.HasPower<DrawnPower>();

    public static int WindowBonus(Creature creature) =>
        creature.HasPower<DrawnPower>() ? creature.GetPower<DrawnPower>().WindowBonus : 0;

    [ThreadStatic]
    public static bool PreviewBonusApplied;

    public static decimal AttackBonus(CardModel card) =>
        card.Owner?.Creature == null ? 0 : AttackBonus(card.Owner.Creature, card);

    public static decimal AttackBonus(Creature creature, CardModel? card)
    {
        var drawn = IsDrawn(creature);
        var willDraw = !drawn && card is IDrawSwordCard;
        if (!drawn && !willDraw) return 0;
        if (willDraw && creature.HasPower<PoisePower>() && Brain(creature).CannotDrawThisTurn)
            return 0;

        decimal bonus = drawn ? WindowBonus(creature) : GetPoise(creature);
        if (!creature.HasPower<PoisePower>())
            return card is IHalfWindowBonus ? Math.Ceiling(bonus / 2m) : bonus;

        var brain = Brain(creature);
        if (!drawn)
            bonus += brain.ExtraWindowFlat + brain.NextDrawExtraDamage;
        bonus += brain.DrawnAttackBonus;
        if (card is IHalfWindowBonus)
            bonus = Math.Ceiling(bonus / 2m);
        if (brain.WandererDamage > 0 && (card == null || card.Type == CardType.Attack))
        {
            var enemies = creature.CombatState?.HittableEnemies.Count() ?? 0;
            bonus += enemies >= 3 ? brain.WandererCrowdDamage : brain.WandererDamage;
        }

        return bonus;
    }

    public static PoisePower Brain(Creature creature) => creature.GetPower<PoisePower>();

    public static async Task EnsureBrain(PlayerChoiceContext ctx, Creature creature)
    {
        if (creature.HasPower<PoisePower>()) return;
        await PowerCmd.Apply<PoisePower>(ctx, creature, 1, creature, null);
        creature.GetPower<PoisePower>()?.SetPoints(0);
    }

    public static async Task GainPoise(PlayerChoiceContext ctx, Creature creature, int delta)
    {
        if (delta == 0) return;
        await EnsureBrain(ctx, creature);
        var power = creature.GetPower<PoisePower>();
        if (power == null) return;
        power.SetPoints(power.Points + delta);
    }

    public static async Task SetPoise(PlayerChoiceContext ctx, Creature creature, int value)
    {
        await EnsureBrain(ctx, creature);
        creature.GetPower<PoisePower>()?.SetPoints(value);
    }

    public static async Task EnterDrawn(PlayerChoiceContext ctx, Player player, int turns, CardModel? source)
    {
        var creature = player.Creature;
        await EnsureBrain(ctx, creature);
        if (IsDrawn(creature)) return;

        var brain = Brain(creature);
        if (brain.CannotDrawThisTurn) return;

        var stored = GetPoise(creature);
        var bonus = stored + brain.ExtraWindowFlat;
        if (brain.NextDrawExtraDamage != 0)
            bonus += brain.NextDrawExtraDamage;

        if (brain.FirstDrawExtraTurn)
        {
            turns += 1;
            brain.FirstDrawExtraTurn = false;
        }

        if (brain.NextDrawExtraTurn)
        {
            turns += 1;
            brain.NextDrawExtraTurn = false;
        }

        var noReset = brain.NextDrawNoReset;
        brain.NextDrawNoReset = false;
        brain.NextDrawExtraDamage = 0;

        await PowerCmd.Apply<DrawnPower>(ctx, creature, turns, creature, source);
        var drawn = creature.GetPower<DrawnPower>();
        drawn.WindowBonus = bonus;
        drawn.StoredPoise = stored;
        await SetPoise(ctx, creature, 0);
        drawn.NoResetOnKill = brain.PendingNoResetOnKill;
        drawn.AllowEarlySheathe = brain.PendingAllowEarlySheathe;
        drawn.NoResetThisWindow = noReset;
        brain.PendingNoResetOnKill = false;
        brain.PendingAllowEarlySheathe = false;
        brain.DamagedThisWindow = false;
        brain.KilledThisWindow = false;
        brain.AttackedThisTurn = false;

        if (brain.BalloonStacks > 0)
        {
            var pile = brain.BalloonAsBlock
                ? brain.BalloonStacks * brain.BalloonPerStack
                : brain.BalloonStacks * brain.BalloonPerStack;
            if (brain.BalloonAsBlock)
                CreatureCmd.GainBlock(creature, pile, default, null, false);
            else
                await CreatureCmd.Damage(ctx, creature.CombatState!.HittableEnemies, pile, ValueProp.Unpowered, creature);
            brain.BalloonStacks = 0;
        }

        await DetonateWrath(ctx, creature);
    }

    public static async Task Sheathe(PlayerChoiceContext ctx, Creature creature)
    {
        if (!IsDrawn(creature)) return;
        var brain = Brain(creature);
        var drawn = creature.GetPower<DrawnPower>();

        var noReset = drawn.NoResetThisWindow
                      || (drawn.NoResetOnKill && brain.KilledThisWindow)
                      || brain.ForceNoResetOnce;
        if (brain.ForceNoResetOnce)
            brain.ForceNoResetOnce = false;

        if (noReset)
        {
            await SetPoise(ctx, creature, drawn.StoredPoise);
        }
        else if (brain.FearNoDark && (brain.DamagedThisWindow || !brain.KilledThisWindow))
        {
            await SetPoise(ctx, creature, drawn.StoredPoise / 2);
            CreatureCmd.GainBlock(creature, brain.FearNoDarkBlock, default, null, false);
        }
        else
        {
            await SetPoise(ctx, creature, brain.NextSheatheKeepPoints);
        }

        brain.NextSheatheKeepPoints = 0;
        await PowerCmd.Remove(drawn);

        if (brain.BlockOnSheathe > 0)
            CreatureCmd.GainBlock(creature, brain.BlockOnSheathe, default, null, false);
        if (brain.DrawOnSheathe > 0)
            await CardPileCmd.Draw(ctx, brain.DrawOnSheathe, creature.Player!);
        if (brain.EnergyOnSheathe > 0)
            await PowerCmd.Apply<EnergyNextTurnPower>(ctx, creature, brain.EnergyOnSheathe, creature, null);
    }

    private static async Task DetonateWrath(PlayerChoiceContext ctx, Creature owner)
    {
        if (owner.CombatState == null) return;
        foreach (var enemy in owner.CombatState.HittableEnemies.ToArray())
        {
            if (!enemy.HasPower<WrathMarkPower>()) continue;
            var stacks = enemy.GetPowerAmount<WrathMarkPower>();
            await PowerCmd.Remove(enemy.GetPower<WrathMarkPower>());
            if (stacks > 0)
                await CreatureCmd.Damage(ctx, enemy, stacks, ValueProp.Unpowered, owner);
        }
    }

    public static bool IsEliteOrBoss(Creature? creature) =>
        creature?.CombatState?.Encounter?.RoomType is RoomType.Elite or RoomType.Boss;
}
