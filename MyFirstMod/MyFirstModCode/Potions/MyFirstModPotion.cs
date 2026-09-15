using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using MyFirstMod.MyFirstModCode.Character;
using MyFirstMod.MyFirstModCode.Extensions;

namespace MyFirstMod.MyFirstModCode.Potions;

[Pool(typeof(TrialistPotionPool))]
public abstract class MyFirstModPotion : CustomPotionModel
{
    public override string? CustomPackedImagePath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PotionImagePath();

    public override string? CustomPackedOutlinePath =>
        $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".PotionOutlineImagePath();
}
