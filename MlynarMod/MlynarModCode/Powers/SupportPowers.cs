using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MlynarMod.MlynarModCode.Core;

namespace MlynarMod.MlynarModCode.Powers;

public class WrathMarkPower : MlynarPower
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "愠怒",
        "进入拔剑时结算为等量真实伤害，然后清零。",
        "进入拔剑时结算为等量真实伤害，然后清零。");
}

public class WandererPower : MlynarPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public int Bonus;
    public int Crowd = 4;
    public int Reduce = 2;

    public override int DisplayAmount => Bonus;

    public void Add(int bonus, int crowd, int reduce)
    {
        Bonus += bonus;
        Crowd += crowd;
        Reduce += reduce;
        InvokeDisplayAmountChanged();
    }

    public override List<(string, string)> Localization => new PowerLoc(
        "游侠",
        "攻击伤害增加。场上敌人不少于 3 名时加得更多，并减少受到的伤害。",
        "攻击伤害增加。场上敌人不少于 3 名时加得更多，并减少受到的伤害。");

    public override decimal ModifyDamageAdditive(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (Owner == null || dealer != Owner || Bonus <= 0) return 0;
        if (cardSource == null || cardSource.Type != CardType.Attack) return 0;
        if (!props.IsPoweredAttack()) return 0;
        var enemies = Owner.CombatState?.HittableEnemies.Count() ?? 0;
        return enemies >= 3 ? Crowd : Bonus;
    }

    public override decimal ModifyDamageMultiplicative(
        Creature? target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        CardPlay? cardPlay)
    {
        if (Owner == null || target != Owner || Reduce <= 0) return 1m;
        var enemies = Owner.CombatState?.HittableEnemies.Count() ?? 0;
        if (enemies < 3) return 1m;
        var reduced = Math.Max(0, (int)amount - Reduce);
        return amount == 0 ? 1m : reduced / amount;
    }
}

public class ManInSheathPower : MlynarPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "鞘中人",
        "每回合开始时，获得层数点蓄势。",
        "每回合开始时，获得层数点蓄势。");

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext ctx, Player player)
    {
        if (player.Creature != Owner || Amount <= 0) return;
        await MlynarRuntime.GainPoise(ctx, Owner, Amount);
    }
}

public class FearNoDarkPower : MlynarPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "不畏苦暗",
        "每当你退出拔剑，获得层数点格挡。",
        "每当你退出拔剑，获得层数点格挡。");
}

public class GoldenWrathPower : MlynarPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override List<(string, string)> Localization => new PowerLoc(
        "金色的愠怒",
        "鞘中的每次攻击对目标施加等同于该次伤害的愠怒。进入拔剑时，愠怒结算为真实伤害。",
        "鞘中的每次攻击对目标施加等同于该次伤害的愠怒。进入拔剑时，愠怒结算为真实伤害。");

    public override async Task AfterAttack(PlayerChoiceContext ctx, AttackCommand attack)
    {
        if (Owner == null || attack.Attacker != Owner) return;
        if (MlynarRuntime.IsDrawn(Owner)) return;
        if (attack.CardPlay?.Card.Type != CardType.Attack) return;

        var hits = attack.Results.SelectMany(r => r).ToList();
        foreach (var result in hits)
        {
            if (result.Receiver == null || result.Receiver == Owner) continue;
            var stacks = (int)result.TotalDamage;
            if (stacks <= 0) continue;
            await PowerCmd.Apply<WrathMarkPower>(ctx, result.Receiver, stacks, Owner, attack.CardPlay?.Card);
        }
    }
}

public class VastRoarPower : MlynarPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override List<(string, string)> Localization => new PowerLoc(
        "苍茫怒号",
        "本场在鞘中时，每当你受到攻击伤害，对来源造成层数点真实伤害。",
        "本场在鞘中时，每当你受到攻击伤害，对来源造成层数点真实伤害。");

    public override async Task BeforeDamageReceived(
        PlayerChoiceContext choiceContext,
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        if (Owner == null || target != Owner || Amount <= 0) return;
        if (dealer == null || dealer == Owner) return;
        if (!props.IsPoweredAttack()) return;
        if (MlynarRuntime.IsDrawn(Owner)) return;
        await CreatureCmd.Damage(choiceContext, dealer, Amount, ValueProp.Unpowered, Owner);
    }
}
