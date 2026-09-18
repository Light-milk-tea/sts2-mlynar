using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Relics;
using MlynarMod.MlynarModCode.Character;
using MlynarMod.MlynarModCode.Extensions;

namespace MlynarMod.MlynarModCode.Relics;

[Pool(typeof(MlynarRelicPool))]
public abstract class MlynarRelic : CustomRelicModel
{
    public override string PackedIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".RelicImagePath();
    protected override string PackedIconOutlinePath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}_outline.png".RelicImagePath();
    protected override string BigIconPath => $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigRelicImagePath();
    public abstract override RelicRarity Rarity { get; }
    public override bool ShouldReceiveCombatHooks => true;
}
