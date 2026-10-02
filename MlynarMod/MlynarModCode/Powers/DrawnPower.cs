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

    public override int DisplayAmount => Amount;
    public override string CustomPackedIconPath => "drawnpower.png".PowerImagePath();
    public override string CustomBigIconPath => "drawnpower.png".BigPowerImagePath();

    public override List<(string, string)> Localization => new PowerLoc(
        "拔剑",
        "已拔剑，层数是剩余回合。攻击伤害每层蓄势 +10%。进入时获得 1 点能量。回合结束层数 -1，到 0 时退出。",
        "已拔剑，层数是剩余回合。攻击伤害每层蓄势 +10%。进入时获得 1 点能量。回合结束层数 -1，到 0 时退出。");

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
