using BaseLib.Abstracts;
using BaseLib.Utils.NodeFactories;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MlynarMod.MlynarModCode.Cards;
using MlynarMod.MlynarModCode.Core;
using MlynarMod.MlynarModCode.Extensions;
using MlynarMod.MlynarModCode.Relics;

namespace MlynarMod.MlynarModCode.Character;

public class Mlynar : PlaceholderCharacterModel
{
    public const string CharacterId = "MLYNARMOD-MLYNAR";
    public static readonly Color Color = new("c9a227");

    public override Color NameColor => Color;
    public override CharacterGender Gender => CharacterGender.Masculine;
    public override int StartingHp => 75;

    public override IEnumerable<CardModel> StartingDeck =>
    [
        ModelDb.Card<RangerSwordsmanship>(),
        ModelDb.Card<RangerSwordsmanship>(),
        ModelDb.Card<RangerSwordsmanship>(),
        ModelDb.Card<RangerSwordsmanship>(),
        ModelDb.Card<SelfContained>(),
        ModelDb.Card<SelfContained>(),
        ModelDb.Card<SelfContained>(),
        ModelDb.Card<SelfContained>(),
        ModelDb.Card<KnightSwordsmanship>(),
        ModelDb.Card<UnvoicedFury>()
    ];

    public override IReadOnlyList<RelicModel> StartingRelics =>
    [
        ModelDb.Relic<UnsharpenedSword>()
    ];

    public override CardPoolModel CardPool => ModelDb.CardPool<MlynarCardPool>();
    public override RelicPoolModel RelicPool => ModelDb.RelicPool<MlynarRelicPool>();
    public override PotionPoolModel PotionPool => ModelDb.PotionPool<MlynarPotionPool>();

    public override Control CustomIcon
    {
        get
        {
            var icon = NodeFactory<Control>.CreateFromResource(CustomIconTexturePath);
            icon.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
            return icon;
        }
    }

    public override string CustomVisualPath => "res://MlynarMod/scenes/Mlynar_anim.tscn";
    public override string CustomRestSiteAnimPath => "res://MlynarMod/scenes/Mlynar_rest_site.tscn";
    public override string CustomMerchantAnimPath => "res://MlynarMod/scenes/Mlynar_merchant.tscn";
    public override string CustomCharacterSelectBg => "res://MlynarMod/scenes/Mlynar_bg.tscn";

    public override string CustomIconTexturePath => "character_icon_mlynar.png".CharacterUiPath();
    public override string CustomCharacterSelectIconPath => "char_select_mlynar.png".CharacterUiPath();
    public override string CustomCharacterSelectLockedIconPath => "char_select_mlynar_locked.png".CharacterUiPath();
    public override string CustomMapMarkerPath => "map_marker_mlynar.png".CharacterUiPath();

    public override float AttackAnimDelay => 0.45f;
    public override float CastAnimDelay => 0.5f;

    public override NCreatureVisuals? CreateCustomVisuals()
    {
        return NodeFactory<NCreatureVisuals>.CreateFromScene(CustomVisualPath);
    }

    public override CreatureAnimator? SetupCustomAnimationStates(MegaSprite controller)
    {
        // 鞘中站立 Idle，拔剑用方舟 Skill_1 生命周期；攻击仍播 Loop 以免 Start 一闪而过。
        var sheathedIdle = new AnimState("Idle", true);
        var drawnIdle = new AnimState("Skill_1_Idle", true);
        var dead = new AnimState("Die");
        var drawStart = new AnimState("Skill_1_Start") { NextState = drawnIdle };
        var sheatheEnd = new AnimState("Skill_1_End") { NextState = sheathedIdle };
        var attack = new AnimState("Skill_1_Loop");
        var cast = new AnimState("Skill_3_Loop");

        bool Drawn() => IsControllerDrawn(controller);
        attack.AddNextState(drawnIdle, Drawn);
        attack.AddNextState(sheathedIdle);
        cast.AddNextState(drawnIdle, Drawn);
        cast.AddNextState(sheathedIdle);

        var initial = Drawn() ? drawnIdle : sheathedIdle;
        var animator = new CreatureAnimator(initial, controller);
        animator.AddAnyState("Idle", drawnIdle, Drawn);
        animator.AddAnyState("Idle", sheathedIdle);
        animator.AddAnyState("Drawn", drawStart);
        animator.AddAnyState("Sheathe", sheatheEnd);
        animator.AddAnyState("Attack", attack);
        animator.AddAnyState("Cast", cast);
        animator.AddAnyState("Hit", drawnIdle, Drawn);
        animator.AddAnyState("Hit", sheathedIdle);
        animator.AddAnyState("Dead", dead);
        animator.AddAnyState("Relaxed", sheathedIdle);
        return animator;
    }

    private static bool IsControllerDrawn(MegaSprite controller)
    {
        var creature = ResolveCreature(controller);
        return creature != null && MlynarRuntime.IsDrawn(creature);
    }

    private static Creature? ResolveCreature(MegaSprite controller)
    {
        if (controller.BoundObject is not Node node)
            return null;

        for (var current = node; current != null; current = current.GetParent())
        {
            if (current is NCreature creatureNode)
                return creatureNode.Entity;
        }

        return null;
    }
}
