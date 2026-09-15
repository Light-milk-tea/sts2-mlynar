using BaseLib.Abstracts;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MyFirstMod.MyFirstModCode.Extensions;

namespace MyFirstMod.MyFirstModCode.Character;

public class TrialistCardPool : CustomCardPoolModel
{
    public override string Title => Trialist.CharacterId;

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();

    public override float H => 0.48f;
    public override float S => 0.55f;
    public override float V => 0.95f;

    public override Color DeckEntryCardColor => Trialist.Color;
    public override bool IsColorless => false;

    // 空池会导致战后 CardReward.Populate 抛错，画面停住。
    // 先借用铁甲战士的可掉落卡；之后自己的卡仍可通过 [Pool] 追加进来。
    protected override CardModel[] GenerateAllCards() =>
        ModelDb.CardPool<IroncladCardPool>().AllCards.ToArray();
}
