using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MlynarMod.MlynarModCode.Character;
using MlynarMod.MlynarModCode.Core;
using MlynarMod.MlynarModCode.Extensions;

namespace MlynarMod.MlynarModCode.Cards;

public interface IDrawSwordCard;

public interface ISheathOnly;

public interface IIgnorePoise;

[Pool(typeof(MlynarCardPool))]
public abstract class MlynarCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    CustomCardModel(cost, type, rarity, target)
{
    public override string CustomPortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();
    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
    public override string BetaPortraitPath => $"beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    protected decimal Dmg => DynamicVars.Damage?.BaseValue ?? 0;
    protected decimal Blk => DynamicVars.Block?.BaseValue ?? 0;

    protected Task Hit(PlayerChoiceContext ctx, CardPlay play, int times = 1) =>
        DamageCmd.Attack(Dmg).FromCard(this, play).Targeting(play.Target!).WithHitCount(times).Execute(ctx);

    protected Task HitAll(PlayerChoiceContext ctx, CardPlay play, int times = 1) =>
        DamageCmd.Attack(Dmg).FromCard(this, play).TargetingAllOpponents(play.Player.Creature.CombatState!).WithHitCount(times).Execute(ctx);

    protected Task HitRandom(PlayerChoiceContext ctx, CardPlay play, int times) =>
        DamageCmd.Attack(Dmg).FromCard(this, play).TargetingRandomOpponents(play.Player.Creature.CombatState!, true).WithHitCount(times).Execute(ctx);

    protected Task HitAmount(PlayerChoiceContext ctx, CardPlay play, decimal amount) =>
        DamageCmd.Attack(amount).FromCard(this, play).Targeting(play.Target!).Execute(ctx);

    protected void Block(CardPlay play) =>
        CreatureCmd.GainBlock(play.Player.Creature, Blk, default, play, false);

    protected void GainBlock(CardPlay play, decimal amount) =>
        CreatureCmd.GainBlock(play.Player.Creature, amount, default, play, false);

    protected async Task DiscardOne(PlayerChoiceContext ctx, CardPlay play)
    {
        var hand = play.Player.PlayerCombatState?.Hand;
        if (hand == null || !hand.Cards.Any()) return;
        await CardSelectCmd.FromHandForDiscard(ctx, play.Player, new CardSelectorPrefs(new LocString("characters", "mlynar_discard_one"), 1), _ => true, this);
    }

    protected async Task OfferTopdeck(PlayerChoiceContext ctx, CardPlay play)
    {
        var state = play.Player.PlayerCombatState;
        if (state == null || state.DiscardPile.IsEmpty) return;
        var selected = await CardSelectCmd.FromCombatPile(
            ctx,
            state.DiscardPile,
            play.Player,
            new CardSelectorPrefs(new LocString("characters", "mlynar_top"), 0, 1));
        var card = selected?.FirstOrDefault();
        if (card != null)
            await CardPileCmd.Add(card, state.DrawPile, CardPilePosition.Top);
    }

    protected override bool IsPlayable
    {
        get
        {
            var creature = Owner?.Creature;
            if (creature != null)
            {
                if (this is IDrawSwordCard && (MlynarRuntime.IsDrawn(creature) || MlynarRuntime.CannotDraw(creature)))
                    return false;
                if (this is ISheathOnly && MlynarRuntime.IsDrawn(creature))
                    return false;
            }

            return base.IsPlayable;
        }
    }
}
