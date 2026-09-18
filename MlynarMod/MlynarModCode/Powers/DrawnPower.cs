using BaseLib.Abstracts;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using MlynarMod.MlynarModCode.Extensions;

namespace MlynarMod.MlynarModCode.Powers;

public class DrawnPower : MlynarPower
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public int WindowBonus;
    public int StoredPoise;
    public bool NoResetOnKill;
    public bool AllowEarlySheathe;
    public bool NoResetThisWindow;

    public override int DisplayAmount => Amount;
    public override string CustomPackedIconPath => "drawnpower.png".PowerImagePath();
    public override string CustomBigIconPath => "drawnpower.png".BigPowerImagePath();

    public override List<(string, string)> Localization => new PowerLoc(
        "拔剑",
        "已拔剑。攻击额外造成进入拔剑时的蓄势。回合结束时层数 -1，到 0 时收剑。",
        "已拔剑。攻击额外造成进入拔剑时的蓄势。回合结束时层数 -1，到 0 时收剑。");

    public override async Task AfterApplied(Creature? applier, CardModel? cardSource)
    {
        if (Owner is not { IsDead: false })
            return;
        await CreatureCmd.TriggerAnim(Owner, "Drawn", 0.35f);
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        if (oldOwner is not { IsDead: false })
            return;
        await CreatureCmd.TriggerAnim(oldOwner, "Sheathe", 0.35f);
    }
}
