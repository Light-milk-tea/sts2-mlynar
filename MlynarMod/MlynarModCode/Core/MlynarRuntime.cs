using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using MlynarMod.MlynarModCode.Powers;

namespace MlynarMod.MlynarModCode.Core;

public static class MlynarRuntime
{
    public const int PoiseCap = 10;
    public const int ExitDrawCount = 3;

    public static int GetPoise(Creature creature) =>
        creature.HasPower<PoisePower>() ? creature.GetPower<PoisePower>()!.Points : 0;

    public static bool IsDrawn(Creature creature) => creature.HasPower<DrawnPower>();

    public static bool CannotDraw(Creature creature) =>
        creature.HasPower<PoisePower>() && Brain(creature).CannotDrawThisTurn;

    public static PoisePower Brain(Creature creature) => creature.GetPower<PoisePower>()!;

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

    public static async Task AddEndTurnPoiseIfNoAttack(PlayerChoiceContext ctx, Creature creature, int amount)
    {
        if (amount == 0) return;
        await EnsureBrain(ctx, creature);
        Brain(creature).EndTurnPoiseIfNoAttack += amount;
    }

    public static async Task AddEndTurnDamage(PlayerChoiceContext ctx, Creature creature, int amount)
    {
        if (amount == 0) return;
        await EnsureBrain(ctx, creature);
        Brain(creature).EndTurnDamage += amount;
    }

    public static async Task AddEndTurnDamageIfNoAttack(PlayerChoiceContext ctx, Creature creature, int amount)
    {
        if (amount == 0) return;
        await EnsureBrain(ctx, creature);
        Brain(creature).EndTurnDamageIfNoAttack += amount;
    }

    public static async Task ForbidDrawThisTurn(PlayerChoiceContext ctx, Creature creature)
    {
        await EnsureBrain(ctx, creature);
        Brain(creature).CannotDrawThisTurn = true;
    }

    public static async Task EnterDrawn(PlayerChoiceContext ctx, Player player, int turns, CardModel? source)
    {
        var creature = player.Creature;
        await EnsureBrain(ctx, creature);
        if (IsDrawn(creature) || Brain(creature).CannotDrawThisTurn) return;

        await PlayerCmd.GainEnergy(1, player);
        await PowerCmd.Apply<DrawnPower>(ctx, creature, turns, creature, source);
        await DetonateWrath(ctx, creature);
    }

    public static async Task Sheathe(PlayerChoiceContext ctx, Creature creature, bool auto, bool keepPoise = false)
    {
        if (!IsDrawn(creature)) return;
        var drawn = creature.GetPower<DrawnPower>();
        if (drawn == null) return;

        await PowerCmd.Remove(drawn);

        if (!keepPoise)
            await SetPoise(ctx, creature, 0);

        if (creature.HasPower<FearNoDarkPower>() && creature.GetPowerAmount<FearNoDarkPower>() > 0)
            await CreatureCmd.GainBlock(creature, creature.GetPowerAmount<FearNoDarkPower>(), default, null, false);

        if (keepPoise) return;

        var player = creature.Player;
        if (player == null) return;
        if (auto)
            await PowerCmd.Apply<DrawCardsNextTurnPower>(ctx, creature, ExitDrawCount, creature, null);
        else
            await CardPileCmd.Draw(ctx, ExitDrawCount, player);
    }

    public static async Task DetonateWrath(PlayerChoiceContext ctx, Creature owner)
    {
        if (owner.CombatState == null) return;
        foreach (var enemy in owner.CombatState.HittableEnemies.ToArray())
        {
            if (!enemy.HasPower<WrathMarkPower>()) continue;
            var stacks = enemy.GetPowerAmount<WrathMarkPower>();
            await PowerCmd.Remove(enemy.GetPower<WrathMarkPower>()!);
            if (stacks > 0)
                await CreatureCmd.Damage(ctx, enemy, stacks, ValueProp.Unpowered, owner);
        }
    }

    public static bool IsEliteOrBoss(Creature? creature) =>
        creature?.CombatState?.Encounter?.RoomType is RoomType.Elite or RoomType.Boss;
}
