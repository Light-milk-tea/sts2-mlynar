using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MyFirstMod.MyFirstModCode.Extensions;

namespace MyFirstMod.MyFirstModCode.Character;

public class TrialistRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => Trialist.Color;
    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();

    protected override IEnumerable<RelicModel> GenerateAllRelics() =>
        ModelDb.RelicPool<IroncladRelicPool>().AllRelics;
}
