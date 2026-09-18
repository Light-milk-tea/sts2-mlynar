using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MlynarMod.MlynarModCode.Character;
using MlynarMod.MlynarModCode.Core;
using MlynarMod.MlynarModCode.Extensions;

namespace MlynarMod.MlynarModCode.Cards;

public interface IDrawSwordCard;

public interface IHalfWindowBonus;

[Pool(typeof(MlynarCardPool))]
public abstract class MlynarCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    CustomCardModel(cost, type, rarity, target)
{
    public override string CustomPortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();
    public override string PortraitPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
    public override string BetaPortraitPath => $"beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();

    protected decimal Dmg => DynamicVars.Damage?.BaseValue ?? 0;
    protected decimal Blk => DynamicVars.Block?.BaseValue ?? 0;

    protected Task Hit(PlayerChoiceContext ctx, CardPlay play, Creature? target = null) =>
        CommonActions.CardAttack(this, play, target ?? play.Target, Dmg, DynamicVars.Damage!.Props).Execute(ctx);

    protected Task HitAll(PlayerChoiceContext ctx, CardPlay play) =>
        CommonActions.CardAttack(this, play).Execute(ctx);

    protected void Block(CardPlay play) =>
        CreatureCmd.GainBlock(play.Player.Creature, Blk, default, play, false);

    protected override bool IsPlayable =>
        !(this is IDrawSwordCard && Owner?.Creature != null && MlynarRuntime.IsDrawn(Owner.Creature))
        && base.IsPlayable;
}
