using BaseLib.Abstracts;
using BaseLib.Commands;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MlynarMod.MlynarModCode.Cards;
using MlynarMod.MlynarModCode.Core;

namespace MlynarMod.MlynarModCode.Relics;

public class UnsharpenedSword : MlynarRelic
{
    public override RelicRarity Rarity => RelicRarity.Starter;
    public override List<(string, string)> Localization => new RelicLoc("未开刃的家传剑", "每场战斗开始时，蓄势 +2。", "还没开刃，但还是那把剑。");

    private bool _applied;

    public override Task BeforeCombatStart()
    {
        _applied = false;
        return Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStartEarly(PlayerChoiceContext ctx, Player player)
    {
        if (player != Owner || _applied) return;
        _applied = true;
        await MlynarRuntime.EnsureBrain(ctx, Owner.Creature);
        await MlynarRuntime.GainPoise(ctx, Owner.Creature, 2);
    }
}

public class NewspaperRelic : MlynarRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;
    public override List<(string, string)> Localization => new RelicLoc("垫座的报纸", "每个鞘中回合，你第一次打出非攻击牌时，蓄势 +1。", "垫着看，也算看过了。");

    public override async Task AfterCardPlayed(PlayerChoiceContext ctx, CardPlay cardPlay)
    {
        if (cardPlay.Player != Owner) return;
        if (cardPlay.Card.Type == CardType.Attack) return;
        if (MlynarRuntime.IsDrawn(Owner.Creature)) return;
        await MlynarRuntime.EnsureBrain(ctx, Owner.Creature);
        var brain = MlynarRuntime.Brain(Owner.Creature);
        if (brain.NewspaperUsedThisTurn) return;
        brain.NewspaperUsedThisTurn = true;
        await MlynarRuntime.GainPoise(ctx, Owner.Creature, 1);
    }
}

public class AlwaysAtSide : MlynarRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;
    public override List<(string, string)> Localization => new RelicLoc("始终在身边", "每场战斗开始时，若抽牌堆有拔剑牌，将其中 1 张放到抽牌堆顶。", "人可以走，剑不该丢。");

    public override async Task BeforeCombatStart()
    {
        var draw = Owner.PlayerCombatState?.DrawPile;
        var entry = draw?.Cards.FirstOrDefault(c => c is IDrawSwordCard);
        if (entry == null || draw == null) return;
        await CardPileCmd.Add(entry, draw, CardPilePosition.Top);
    }
}

public class FadedWords : MlynarRelic
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;
    public override List<(string, string)> Localization => new RelicLoc("磨淡的字", "每当你收剑，获得 5 点格挡。", "字淡了，事还在。");

    private bool _applied;

    public override Task BeforeCombatStart()
    {
        _applied = false;
        return Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext ctx, Player player)
    {
        if (player != Owner || _applied) return;
        _applied = true;
        await MlynarRuntime.EnsureBrain(ctx, Owner.Creature);
        MlynarRuntime.Brain(Owner.Creature).BlockOnSheathe += 5;
    }
}

public class HonedSword : MlynarRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;
    public override List<(string, string)> Localization => new RelicLoc("被磨利的剑", "每场战斗第一次收剑时，不重置。", "磨过，就还记得怎么用。");

    private bool _applied;

    public override Task BeforeCombatStart()
    {
        _applied = false;
        return Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext ctx, Player player)
    {
        if (player != Owner || _applied) return;
        _applied = true;
        await MlynarRuntime.EnsureBrain(ctx, Owner.Creature);
        MlynarRuntime.Brain(Owner.Creature).ForceNoResetOnce = true;
    }
}

public class ScoutReport : MlynarRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;
    public override List<(string, string)> Localization => new RelicLoc("侦察报告", "每个回合开始时，预见 1。", "报告看完了，路还是自己走。");

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext ctx, Player player)
    {
        if (player != Owner) return;
        await ScryCmd.Execute(ctx, player, 1);
    }
}

public class AlreadyTrained : MlynarRelic
{
    public override RelicRarity Rarity => RelicRarity.Rare;
    public override List<(string, string)> Localization => new RelicLoc("已然受训", "进入拔剑时，这次拔剑的额外伤害 +2。", "受过训，不代表还想当骑士。");

    private bool _applied;

    public override Task BeforeCombatStart()
    {
        _applied = false;
        return Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext ctx, Player player)
    {
        if (player != Owner || _applied) return;
        _applied = true;
        await MlynarRuntime.EnsureBrain(ctx, Owner.Creature);
        MlynarRuntime.Brain(Owner.Creature).ExtraWindowFlat += 2;
    }
}

public class BlankCommission : MlynarRelic
{
    public override RelicRarity Rarity => RelicRarity.Shop;
    public override List<(string, string)> Localization => new RelicLoc("空白擢升文书", "每场战斗第一次进入拔剑时，持续时间 +1 回合。", "名字空着，剑还在。");

    private bool _applied;

    public override Task BeforeCombatStart()
    {
        _applied = false;
        return Task.CompletedTask;
    }

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext ctx, Player player)
    {
        if (player != Owner || _applied) return;
        _applied = true;
        await MlynarRuntime.EnsureBrain(ctx, Owner.Creature);
        MlynarRuntime.Brain(Owner.Creature).FirstDrawExtraTurn = true;
    }
}
