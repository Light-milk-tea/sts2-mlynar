using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MlynarMod.MlynarModCode.Core;

namespace MlynarMod.MlynarModCode.Cards;

/// <summary>
/// 对齐斥罪 <c>TrialDamageVar</c>：在原版 Strength/易伤预览之后，把蓄势加成写进 PreviewValue。
/// 牌面必须用 <c>{Damage:diff()}</c> 才会显示这个预览值。
/// </summary>
public class MlynarDamageVar : DamageVar
{
    public MlynarDamageVar(decimal damage, ValueProp props)
        : base(damage, props)
    {
    }

    public MlynarDamageVar(string name, decimal damage, ValueProp props)
        : base(name, damage, props)
    {
    }

    public override void UpdateCardPreview(CardModel card, CardPreviewMode previewMode, Creature? target, bool runGlobalHooks)
    {
        base.UpdateCardPreview(card, previewMode, target, runGlobalHooks);
        if (!runGlobalHooks || !Props.IsPoweredAttack())
            return;

        if (MlynarRuntime.PreviewBonusApplied)
            return;

        var bonus = MlynarRuntime.AttackBonus(card);
        if (bonus == 0)
            return;

        PreviewValue += bonus;
        MlynarRuntime.PreviewBonusApplied = true;
    }
}
