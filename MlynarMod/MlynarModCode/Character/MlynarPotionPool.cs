using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.PotionPools;
using MlynarMod.MlynarModCode.Extensions;

namespace MlynarMod.MlynarModCode.Character;

public class MlynarPotionPool : CustomPotionPoolModel
{
    public override Color LabOutlineColor => Mlynar.Color;
    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();

    protected override IEnumerable<PotionModel> GenerateAllPotions() =>
        ModelDb.PotionPool<IroncladPotionPool>().AllPotions;
}
