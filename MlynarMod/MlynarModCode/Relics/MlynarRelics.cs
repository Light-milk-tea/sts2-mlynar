using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MlynarMod.MlynarModCode.Core;

namespace MlynarMod.MlynarModCode.Relics;

public class UnsharpenedSword : MlynarRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;
    public override List<(string, string)> Localization => new RelicLoc(
        "未开刃的家传剑",
        "每场战斗开始时，获得 2 层蓄势和 3 层荆棘。",
        "还没开刃，但还是那把剑。");

    private bool _applied;

    public override Task BeforeCombatStart()
    {
        _applied = false;
        return Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStartEarly(PlayerChoiceContext ctx, Player player)
    {
        if (player != Owner || _applied) return;
        _applied = true;
        await PowerCmd.Apply<ThornsPower>(ctx, Owner.Creature, 3, Owner.Creature, null);
        await MlynarRuntime.GainPoise(ctx, Owner.Creature, 2);
    }
}
