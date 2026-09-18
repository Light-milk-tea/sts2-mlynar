using BaseLib.Abstracts;
using Godot;
using MlynarMod.MlynarModCode.Extensions;

namespace MlynarMod.MlynarModCode.Character;

public class MlynarRelicPool : CustomRelicPoolModel
{
    public override Color LabOutlineColor => Mlynar.Color;
    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
}
