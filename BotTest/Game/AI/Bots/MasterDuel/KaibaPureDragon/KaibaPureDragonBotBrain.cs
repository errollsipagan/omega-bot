using BotTest.Game.AI.Brains;
using OmegaBotWrapper;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.Heuristics;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;

namespace BotTest.Game.AI.Bots.MasterDuel.KaibaPureDragon;

public sealed class KaibaPureDragonBotBrain : KaibaBotBrain
{
    private enum KaibaAction
    {
        None,
        FluteSummon,
        KaiserTribute
    }

    public KaibaPureDragonBotBrain(
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

        var hand = await GetBotHandCardsAsync();
        var monsters = await GetBotMonsterCardsAsync();
        var state = context.StateData;
        var summonable = (state.SummonableCards ?? Array.Empty<DynamicCard>())
            .OrderByDescending(card => card.Attack)
            .Where(card => CanNormalSummon(card, monsters))
            .ToArray();
        bool hasFaceUpLordOfD = monsters.Any(card =>
            card.IsCode(KaibaPureDragonCardId.LordOfD) && card.IsFaceup());
        bool hasLordOfDOnField = monsters.Any(card =>
            card.IsCode(KaibaPureDragonCardId.LordOfD));
        bool hasFluteInHand = hand.Any(card =>
            card.IsCode(KaibaPureDragonCardId.TheFluteOfSummoningDragon));
        bool hasKaiserOnField = monsters.Any(card =>
            card.IsCode(KaibaPureDragonCardId.KaiserSeaHorse));
        var bewCard = hand.FirstOrDefault(card =>
            card.IsCode(KaibaPureDragonCardId.BlueEyesWhiteDragon));
        var hasBEWDragonInHand = hand.Any(card =>
            card.IsCode(KaibaPureDragonCardId.BlueEyesWhiteDragon));
        bool hasHighLevelDragonInHand = hand.Any(IsHighLevelDragon);
        var hasSummonable = summonable.Count() > 0;
        var specializedMoves = new List<KaibaScoredMove>();

        if (hasFaceUpLordOfD &&
            hasHighLevelDragonInHand &&
            TryGetActivable(context.StateData, KaibaPureDragonCardId.TheFluteOfSummoningDragon, out var flute))
        {
            specializedMoves.Add(new KaibaScoredMove(
                "Activate The Flute of Summoning Dragon",
                flute,
                flute.MainActivateFromIndex(),
                500,
                () => SetPendingAction(
                    PendingActionKind.Reveal,
                    flute.ID,
                    MainAction.Activate,
                    contextCode: (int)KaibaAction.FluteSummon)));
        }

        if (!hasLordOfDOnField &&
            hasFluteInHand &&
            hasHighLevelDragonInHand)
        {
            if (TryGetSummonable(context.StateData, KaibaPureDragonCardId.LordOfD, out var lordOfD))
            {
                specializedMoves.Add(new KaibaScoredMove(
                    "Normal Summon Lord of D. for combo",
                    lordOfD,
                    lordOfD.NormalSummon(),
                    200));
            }
        }

        if (!hasKaiserOnField && hasHighLevelDragonInHand)
        {
            if (TryGetSummonable(context.StateData, KaibaPureDragonCardId.KaiserSeaHorse, out var kaiser))
            {
                specializedMoves.Add(new KaibaScoredMove(
                    "Normal Kaiser Sea Horse for BEW combo",
                    kaiser,
                    kaiser.NormalSummon(),
                    (int)CardScore.Excellent));
            }
        }

        if (hasKaiserOnField && hasBEWDragonInHand && hasSummonable)
        {
            specializedMoves.Add(new KaibaScoredMove(
                "Summon BEW using Kaiser Sea Horse",
                bewCard,
                bewCard.NormalSummon(),
                (int)CardScore.Perfect,
                () => SetPendingAction(
                    PendingActionKind.TargetSelection,
                    KaibaCardId.BlueEyesWhiteDragon,
                    MainAction.Summon,
                    contextCode: (int)KaibaAction.KaiserTribute)
                ));
        }

        var filteredState = FilterLordOfDSet(context.StateData);
        if (!hasFluteInHand || !hasHighLevelDragonInHand)
            filteredState = FilterLordOfDSummon(filteredState);

        var caveDragon = (filteredState.SummonableCards ?? Array.Empty<DynamicCard>())
            .FirstOrDefault(card => card.IsCode(KaibaPureDragonCardId.CaveDragon));
        bool controlsDragon = monsters.Any(card => card.HasRace(CardRace.Dragon));
        if (caveDragon.ID > 0)
        {
            specializedMoves.Add(new KaibaScoredMove(
                controlsDragon
                    ? "Normal Summon Cave Dragon with Dragon controlled"
                    : "Normal Summon Cave Dragon without Dragon (illegal)",
                caveDragon,
                caveDragon.NormalSummon(),
                controlsDragon ? 200 : -500,
                IsLegal: controlsDragon));
            if (!controlsDragon)
            {
                filteredState.SummonableCards = (filteredState.SummonableCards ?? Array.Empty<DynamicCard>())
                    .Where(card => !card.IsCode(KaibaPureDragonCardId.CaveDragon))
                    .ToArray();
            }
        }
        else if (!controlsDragon)
        {
            var caveDragonInHand = hand.FirstOrDefault(card =>
                card.IsCode(KaibaPureDragonCardId.CaveDragon));
            if (caveDragonInHand.ID > 0)
            {
                specializedMoves.Add(new KaibaScoredMove(
                    "Normal Summon Cave Dragon without Dragon (illegal)",
                    caveDragonInHand,
                    new IdleCmdResponse(MainAction.Summon),
                    -500,
                    IsLegal: false));
            }
        }

        return await SelectScoredIdleMoveAsync(
            context with { StateData = filteredState },
            ct,
            specializedMoves);
    }

    public override async Task<CardPosition> OnSelectPositionAsync(
        DecisionContext<SelectPositionData> context,
        CancellationToken ct = default)
    {
        await PrepareContextAsync(context, ct);

        if (context.StateData.CardID == KaibaPureDragonCardId.LordOfD &&
            (context.StateData.Positions ?? Array.Empty<byte>()).Contains((byte)CardPosition.FaceUpAttack))
        {
            return CardPosition.FaceUpAttack;
        }

        return await base.OnSelectPositionAsync(context, ct);
    }

    public override async Task<IList<byte>?> OnSelectCardAsync(
        DecisionContext<SelectCardData> context,
        CancellationToken ct = default)
    {
        await PrepareContextAsync(context, ct);

        if (PendingContextIs((int)KaibaAction.FluteSummon) &&
            context.StateData.SelectionHint == (int)DHint.SPSUMMON)
        {
            var selectable = context.StateData.SelectableCards ?? Array.Empty<DynamicCard>();
            int selectionCount = Math.Min(context.StateData.Maximum, selectable.Length);
            var selection = selectable
                .Select((card, index) => new { Card = card, Index = index })
                .Where(item => item.Card.HasRace(CardRace.Dragon))
                .OrderByDescending(item => item.Card.IsCode(KaibaPureDragonCardId.BlueEyesWhiteDragon))
                .ThenByDescending(item => item.Card.Level)
                .ThenByDescending(item => item.Card.Attack)
                .Take(selectionCount)
                .Select(item => (byte)item.Index)
                .ToArray();

            if (selection.Length >= context.StateData.Minimum)
            {
                ClearPendingAction();
                return selection;
            }
        }

        return await base.OnSelectCardAsync(context, ct);
    }

    private static IdleCmdData FilterLordOfDSet(IdleCmdData state)
    {
        state.MonsterSetableCards = (state.MonsterSetableCards ?? Array.Empty<DynamicCard>())
            .Where(card => !card.IsCode(KaibaPureDragonCardId.LordOfD))
            .ToArray();
        return state;
    }

    private static IdleCmdData FilterLordOfDSummon(IdleCmdData state)
    {
        state.SummonableCards = (state.SummonableCards ?? Array.Empty<DynamicCard>())
            .Where(card => !card.IsCode(KaibaPureDragonCardId.LordOfD))
            .ToArray();
        return state;
    }

    private static bool IsHighLevelDragon(DynamicCard card) =>
        card.HasRace(CardRace.Dragon) && card.Level >= 7;
    
    public override async Task<IList<byte>?> OnSelectTributeAsync(
        DecisionContext<SelectTributeData> context,
        CancellationToken ct = default)
    {
        await PrepareContextAsync(context, ct);

        var selectable = context.StateData.SelectableCards ?? Array.Empty<DynamicCard>();
        if (PendingAction.ContextCode == (int)KaibaAction.KaiserTribute)
        {
            var tributes = selectable
                .Select((card, index) => new { Card = card, Index = index })
                .Where(item => item.Card.IsCode(KaibaPureDragonCardId.KaiserSeaHorse))
                .Take(context.StateData.Minimum)
                .Select(item => (byte)item.Index)
                .ToArray();

            if (tributes.Length >= context.StateData.Minimum)
            {
                ClearPendingAction();
                return tributes;
            }

            ClearPendingAction();
        }

        return await base.OnSelectTributeAsync(context, ct);
    }
}
