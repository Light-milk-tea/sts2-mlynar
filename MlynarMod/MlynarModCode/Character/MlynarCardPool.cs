using BaseLib.Abstracts;
using Godot;
using MlynarMod.MlynarModCode.Extensions;

namespace MlynarMod.MlynarModCode.Character;

public class MlynarCardPool : CustomCardPoolModel
{
    public override string Title => Mlynar.CharacterId;
    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();
    public override float H => 0.12f;
    public override float S => 0.72f;
    public override float V => 0.82f;
    public override Color DeckEntryCardColor => Mlynar.Color;
    public override bool IsColorless => false;
}
