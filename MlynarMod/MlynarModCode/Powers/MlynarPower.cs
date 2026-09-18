using BaseLib.Abstracts;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Powers;
using MlynarMod.MlynarModCode.Extensions;

namespace MlynarMod.MlynarModCode.Powers;

public abstract class MlynarPower : CustomPowerModel
{
    public override string CustomPackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PowerImagePath();
    public override string CustomBigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigPowerImagePath();
    public abstract override PowerType Type { get; }
    public abstract override PowerStackType StackType { get; }
    public override bool ShouldReceiveCombatHooks => true;
}
