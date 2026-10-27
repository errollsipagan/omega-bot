using BotTest.Game.AI.Brains;
using OmegaBotWrapper;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.Heuristics;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;

namespace BotTest.Game.AI.Bots.MasterDuel.KaibaHeavyFusion;

public sealed class KaibaHeavyFusionBotBrain : KaibaBotBrain
{
    private enum KaibaFusionAction
    {
        None,
        SearchThunderDragon,
        SummonBlueEyesUltimateDragon,
        SummonTwinHeadedThunderDragon
    }

    public KaibaHeavyFusionBotBrain(
        ICardEvaluator? cardEvaluator = null,
        IBoardEvaluator? boardEvaluator = null,
        IActionScorer? actionScorer = null)
        : base(cardEvaluator, boardEvaluator, actionScorer)
    {
    }

    public override async Task<IdleCmdResponse> OnIdleCmdAsync(
        DecisionContext<IdleCmdData> context,
        CancellationToken ct = default)
    {
        await PrepareContextAsync(context, ct);
        var specializedMoves = new List<KaibaScoredMove>();

        if (TryGetActivable(context.StateData, KaibaHeavyFusionCardId.ThunderDragon, out var thunderDragon))
        {
            specializedMoves.Add(new KaibaScoredMove(
                "Activate Thunder Dragon to search",
                thunderDragon,
                thunderDragon.MainActivateFromIndex(),
                200,
                () => SetPendingAction(
                    PendingActionKind.Search,
                    thunderDragon.ID,
                    MainAction.Activate,
                    contextCode: (int)KaibaFusionAction.SearchThunderDragon)));
        }

        var materials = await GetFusionMaterialsAsync();
        var fusionActions = GetFusionLines(materials);
        if (fusionActions.Count > 0 &&
            TryGetActivable(context.StateData, KaibaHeavyFusionCardId.Polymerization, out var polymerization))
        {
            foreach (var fusionAction in fusionActions)
            {
                bool usesSubstitute = fusionAction == KaibaFusionAction.SummonBlueEyesUltimateDragon
                    ? materials.Count(card => card.IsCode(KaibaHeavyFusionCardId.BlueEyesWhiteDragon)) < 3
                    : materials.Count(card => card.IsCode(KaibaHeavyFusionCardId.ThunderDragon)) < 2;
                int fusionScore = fusionAction == KaibaFusionAction.SummonBlueEyesUltimateDragon ? 600 : 350;
                if (usesSubstitute)
                    fusionScore += 150;

                specializedMoves.Add(new KaibaScoredMove(
                    $"Activate Polymerization for {fusionAction}",
                    polymerization,
                    polymerization.MainActivateFromIndex(),
                    fusionScore,
                    () => SetPendingAction(
                        PendingActionKind.MaterialSelection,
                        polymerization.ID,
                        MainAction.Activate,
                        contextCode: (int)fusionAction)));
            }
        }

        if (TryGetMonsterSettable(context.StateData, KaibaHeavyFusionCardId.VersagoTheDestroyer, out var versago))
            specializedMoves.Add(new KaibaScoredMove(
                "Set Versago the Destroyer as defensive fusion substitute",
                versago,
                versago.SetMonster(),
                100));

        if (TryGetMonsterSettable(context.StateData, KaibaHeavyFusionCardId.GoddessWithTheThirdEye, out var goddess))
            specializedMoves.Add(new KaibaScoredMove(
                "Set Goddess with the Third Eye as defensive fusion substitute",
                goddess,
                goddess.SetMonster(),
                100));

        var filteredState = FilterFusionSubstitutesFromSummons(context.StateData);
        return await SelectScoredIdleMoveAsync(
            context with { StateData = filteredState },
            ct,
            specializedMoves);
    }

    public override async Task<bool> OnSelectEffectYnAsync(
        DecisionContext<SelectEffectYnData> context,
        CancellationToken ct = default)
    {
        await PrepareContextAsync(context, ct);

        if (context.StateData.EffectCard.IsCode(KaibaHeavyFusionCardId.ThunderDragon))
            return true;

        return await base.OnSelectEffectYnAsync(context, ct);
    }

    public override async Task<IList<byte>?> OnSelectCardAsync(
        DecisionContext<SelectCardData> context,
        CancellationToken ct = default)
    {
        await PrepareContextAsync(context, ct);

        var selectable = context.StateData.SelectableCards ?? Array.Empty<DynamicCard>();
        if (PendingContextIs((int)KaibaFusionAction.SearchThunderDragon) &&
            context.StateData.SelectionHint == (int)DHint.ATOHAND)
        {
            var selection = selectable
                .Select((card, index) => new { Card = card, Index = index })
                .Where(item => item.Card.IsCode(KaibaHeavyFusionCardId.ThunderDragon))
                .Take(Math.Min(context.StateData.Maximum, selectable.Length))
                .Select(item => (byte)item.Index)
                .ToArray();

            if (selection.Length >= context.StateData.Minimum)
            {
                ClearPendingAction();
                return selection;
            }
        }

        if (context.StateData.SelectionHint == (int)DHint.FMATERIAL)
        {
            var fusion = (KaibaFusionAction)PendingAction.ContextCode;
            var selection = TrySelectFusionMaterials(context.StateData, fusion);
            if (selection != null)
            {
                ClearPendingAction();
                return selection;
            }
        }

        return await base.OnSelectCardAsync(context, ct);
    }

    public override async Task<CardPosition> OnSelectPositionAsync(
        DecisionContext<SelectPositionData> context,
        CancellationToken ct = default)
    {
        await PrepareContextAsync(context, ct);

        if (context.StateData.CardID is KaibaHeavyFusionCardId.BlueEyesUltimateDragon or
            KaibaHeavyFusionCardId.TwinHeadedThunderDragon)
        {
            var positions = context.StateData.Positions ?? Array.Empty<byte>();
            if (positions.Contains((byte)CardPosition.FaceUpAttack))
                return CardPosition.FaceUpAttack;
        }

        return await base.OnSelectPositionAsync(context, ct);
    }

    private async Task<List<DynamicCard>> GetFusionMaterialsAsync()
    {
        var hand = await GetBotHandCardsAsync();
        var monsters = await GetBotMonsterCardsAsync();
        return hand.Concat(monsters).ToList();
    }

    private static List<KaibaFusionAction> GetFusionLines(IEnumerable<DynamicCard> materials)
    {
        var available = materials.ToList();
        var fusionActions = new List<KaibaFusionAction>();
        if (CanSummonBlueEyesUltimateDragon(available))
            fusionActions.Add(KaibaFusionAction.SummonBlueEyesUltimateDragon);

        if (CanSummonTwinHeadedThunderDragon(available))
            fusionActions.Add(KaibaFusionAction.SummonTwinHeadedThunderDragon);

        return fusionActions;
    }

    private static bool CanSummonBlueEyesUltimateDragon(IReadOnlyCollection<DynamicCard> materials)
    {
        int blueEyesCount = materials.Count(card => card.IsCode(KaibaHeavyFusionCardId.BlueEyesWhiteDragon));
        int substituteCount = materials.Count(IsFusionSubstitute);
        return blueEyesCount == 3 || blueEyesCount + substituteCount >= 3;
    }

    private static bool CanSummonTwinHeadedThunderDragon(IReadOnlyCollection<DynamicCard> materials)
    {
        int thunderDragonCount = materials.Count(card => card.IsCode(KaibaHeavyFusionCardId.ThunderDragon));
        int substituteCount = materials.Count(IsFusionSubstitute);
        return thunderDragonCount >= 2 || thunderDragonCount + substituteCount >= 2;
    }

    private static IList<byte>? TrySelectFusionMaterials(SelectCardData state, KaibaFusionAction fusion)
    {
        int requiredCount = fusion switch
        {
            KaibaFusionAction.SummonBlueEyesUltimateDragon => 3,
            KaibaFusionAction.SummonTwinHeadedThunderDragon => 2,
            _ => 0
        };

        if (requiredCount == 0 ||
            requiredCount < state.Minimum ||
            requiredCount > state.Maximum)
        {
            return null;
        }

        var selectable = (state.SelectableCards ?? Array.Empty<DynamicCard>())
            .Select((card, index) => new { Card = card, Index = index })
            .ToList();

        foreach (var combination in selectable.GetCombinations(requiredCount))
        {
            var selected = combination.ToList();
            bool isValid = fusion switch
            {
                KaibaFusionAction.SummonBlueEyesUltimateDragon =>
                    HasBlueEyesUltimateMaterials(selected.Select(item => item.Card)),
                KaibaFusionAction.SummonTwinHeadedThunderDragon =>
                    HasTwinHeadedThunderMaterials(selected.Select(item => item.Card)),
                _ => false
            };

            if (isValid)
                return selected.Select(item => (byte)item.Index).ToArray();
        }

        return null;
    }

    private static bool HasBlueEyesUltimateMaterials(IEnumerable<DynamicCard> materials)
    {
        var selected = materials.ToList();
        int blueEyesCount = selected.Count(card => card.IsCode(KaibaHeavyFusionCardId.BlueEyesWhiteDragon));
        int substituteCount = selected.Count(IsFusionSubstitute);
        return blueEyesCount == 3 || blueEyesCount == 2 && substituteCount == 1;
    }

    private static bool HasTwinHeadedThunderMaterials(IEnumerable<DynamicCard> materials)
    {
        var selected = materials.ToList();
        int thunderDragonCount = selected.Count(card => card.IsCode(KaibaHeavyFusionCardId.ThunderDragon));
        int substituteCount = selected.Count(IsFusionSubstitute);
        return thunderDragonCount == 2 || thunderDragonCount == 1 && substituteCount == 1;
    }

    private static bool IsFusionSubstitute(DynamicCard card) =>
        card.IsCode(
            KaibaHeavyFusionCardId.VersagoTheDestroyer,
            KaibaHeavyFusionCardId.GoddessWithTheThirdEye);

    private static IdleCmdData FilterFusionSubstitutesFromSummons(IdleCmdData state)
    {
        state.SummonableCards = (state.SummonableCards ?? Array.Empty<DynamicCard>())
            .Where(card => !IsFusionSubstitute(card))
            .ToArray();
        return state;
    }
}
