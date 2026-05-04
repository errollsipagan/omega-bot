using OmegaBotWrapper.Enum;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;

namespace OmegaBotWrapper.Heuristics
{
    /// <summary>
    /// Default heuristic brain. It keeps the current bot intentionally conservative,
    /// while exposing a richer candidate/score pipeline for future deck-specific AI.
    /// </summary>
    public class DefaultBotBrain : IBotBrain
    {
        private readonly ICardEvaluator _cardEvaluator;
        private readonly IBoardEvaluator _boardEvaluator;
        private readonly IActionScorer _actionScorer;

        public DefaultBotBrain(
            ICardEvaluator? cardEvaluator = null,
            IBoardEvaluator? boardEvaluator = null,
            IActionScorer? actionScorer = null)
        {
            _cardEvaluator = cardEvaluator ?? new DefaultCardEvaluator();
            _boardEvaluator = boardEvaluator ?? new DefaultBoardEvaluator();
            _actionScorer = actionScorer ?? new DefaultActionScorer(_cardEvaluator, _boardEvaluator);
        }

        public virtual Task<BattleCmdResponse> OnBattleCmdAsync(DecisionContext<BattleCmdData> context, CancellationToken ct = default)
        {
            List<CandidateAction> candidates = new();
            DynamicCard[] attackableCards = context.StateData.AttackableCards ?? Array.Empty<DynamicCard>();

            for (int i = 0; i < attackableCards.Length; i++)
            {
                var card = attackableCards[i];
                if (card.Attacked || !_cardEvaluator.IsActionable(card))
                    continue;

                var tags = new List<string>();
                if (card.CanDirectAttack)
                    tags.Add("direct-attack");
                if (_boardEvaluator.CanDetectLethal(context.Field, card))
                    tags.Add("lethal");

                candidates.Add(new CandidateAction
                {
                    State = BotState.SelectBattleCmd,
                    Kind = CandidateActionKind.Attack,
                    Response = CandidateResponse.Create(card.DeclareAttack()),
                    SourceCard = card,
                    ActionIndex = card.ActionIndex.Length > (int)BattleAction.Attack ? card.ActionIndex[(int)BattleAction.Attack] : null,
                    Order = i,
                    Tags = tags
                });
            }

            if (context.StateData.CanMainPhaseTwo)
            {
                candidates.Add(new CandidateAction
                {
                    State = BotState.SelectBattleCmd,
                    Kind = CandidateActionKind.ToMainPhaseTwo,
                    Response = CandidateResponse.Create(new BattleCmdResponse(BattleAction.ToMainPhaseTwo)),
                    Order = int.MaxValue - 1
                });
            }

            if (context.StateData.CanEndPhase)
            {
                candidates.Add(new CandidateAction
                {
                    State = BotState.SelectBattleCmd,
                    Kind = CandidateActionKind.ToEndPhase,
                    Response = CandidateResponse.Create(new BattleCmdResponse(BattleAction.ToEndPhase)),
                    Order = int.MaxValue
                });
            }

            BattleCmdResponse fallback = context.StateData.CanMainPhaseTwo
                ? new BattleCmdResponse(BattleAction.ToMainPhaseTwo)
                : new BattleCmdResponse(BattleAction.ToEndPhase);

            return Task.FromResult(ChooseBest(context, candidates, fallback));
        }

        public virtual Task<IdleCmdResponse> OnIdleCmdAsync(DecisionContext<IdleCmdData> context, CancellationToken ct = default)
        {
            List<CandidateAction> candidates = new();
            DynamicCard[] summonableCards = context.StateData.SummonableCards ?? Array.Empty<DynamicCard>();

            for (int i = 0; i < summonableCards.Length; i++)
            {
                var card = summonableCards[i];
                if (!_cardEvaluator.IsActionable(card))
                    continue;

                candidates.Add(new CandidateAction
                {
                    State = BotState.SelectIdleCmd,
                    Kind = CandidateActionKind.Summon,
                    Response = CandidateResponse.Create(card.NormalSummon()),
                    SourceCard = card,
                    ActionIndex = card.ActionIndex.Length > (int)MainAction.Summon ? card.ActionIndex[(int)MainAction.Summon] : null,
                    Order = i
                });
            }

            if (context.StateData.CanBattlePhase)
            {
                candidates.Add(new CandidateAction
                {
                    State = BotState.SelectIdleCmd,
                    Kind = CandidateActionKind.ToBattlePhase,
                    Response = CandidateResponse.Create(new IdleCmdResponse(MainAction.ToBattlePhase)),
                    Order = int.MaxValue - 1
                });
            }

            if (context.StateData.CanEndPhase)
            {
                candidates.Add(new CandidateAction
                {
                    State = BotState.SelectIdleCmd,
                    Kind = CandidateActionKind.ToEndPhase,
                    Response = CandidateResponse.Create(new IdleCmdResponse(MainAction.ToEndPhase)),
                    Order = int.MaxValue
                });
            }

            IdleCmdResponse fallback = context.StateData.CanBattlePhase
                ? new IdleCmdResponse(MainAction.ToBattlePhase)
                : new IdleCmdResponse(MainAction.ToEndPhase);

            return Task.FromResult(ChooseBest(context, candidates, fallback));
        }

        public virtual Task<int> OnSelectChainAsync(DecisionContext<SelectChainData> context, CancellationToken ct = default)
        {
            List<CandidateAction> candidates = new();
            DynamicCard[] selectableCards = context.StateData.SelectableCards ?? Array.Empty<DynamicCard>();
            int[] descriptions = context.StateData.Descriptions ?? Array.Empty<int>();

            for (int i = 0; i < selectableCards.Length; i++)
            {
                var card = selectableCards[i];
                candidates.Add(new CandidateAction
                {
                    State = BotState.SelectChain,
                    Kind = CandidateActionKind.ActivateChain,
                    Response = CandidateResponse.Create(i),
                    SourceCard = card,
                    Description = descriptions.Length > i ? descriptions[i] : null,
                    IsForced = context.StateData.Forced,
                    Order = i
                });
            }

            if (!context.StateData.Forced)
            {
                candidates.Add(new CandidateAction
                {
                    State = BotState.SelectChain,
                    Kind = CandidateActionKind.PassChain,
                    Response = CandidateResponse.Create(-1),
                    Order = int.MaxValue
                });
            }

            int fallback = context.StateData.Forced ? 0 : -1;
            return Task.FromResult(ChooseBest(context, candidates, fallback));
        }

        public virtual Task<IList<byte>?> OnSelectCardAsync(DecisionContext<SelectCardData> context, CancellationToken ct = default)
        {
            if (context.StateData.SelectionHint == (int)DHint.ATTACKTARGET &&
                context.Field.HasValue &&
                context.Field.Value.OwnSide.HasBattlingMonster)
            {
                DynamicCard attacker = context.Field.Value.OwnSide.BattlingMonster;
                var bestTarget = (context.StateData.SelectableCards ?? Array.Empty<DynamicCard>())
                    .Select((card, index) => new
                    {
                        Index = index,
                        Score = _boardEvaluator.EvaluateBattleTarget(attacker, card, _cardEvaluator)
                    })
                    .OrderByDescending(option => option.Score)
                    .ThenBy(option => option.Index)
                    .FirstOrDefault();

                if (bestTarget != null)
                    return Task.FromResult<IList<byte>?>(new[] { (byte)bestTarget.Index });
            }

            if (context.StateData.SelectionHint is (int)DHint.TARGET or (int)DHint.DESTROY or (int)DHint.REMOVE)
            {
                byte[] bestTargets = (context.StateData.SelectableCards ?? Array.Empty<DynamicCard>())
                    .Select((card, index) => new
                    {
                        Index = index,
                        Score = _boardEvaluator.EvaluateRemovalTarget(card, _cardEvaluator)
                    })
                    .OrderByDescending(option => option.Score)
                    .ThenBy(option => option.Index)
                    .Take(context.StateData.Minimum)
                    .Select(option => (byte)option.Index)
                    .ToArray();

                if (bestTargets.Length == context.StateData.Minimum)
                    return Task.FromResult<IList<byte>?>(bestTargets);
            }

            byte[] selection = new byte[context.StateData.Minimum];
            for (int i = 0; i < selection.Length; i++)
                selection[i] = (byte)i;
            return Task.FromResult<IList<byte>?>(selection);
        }

        public virtual Task<IList<byte>?> OnSelectTributeAsync(DecisionContext<SelectTributeData> context, CancellationToken ct = default)
        {
            return Task.FromResult<IList<byte>?>(BotActionUtilities.BuildSequentialListSelection(context.StateData.Minimum));
        }

        public virtual Task<IList<byte>?> OnSelectSumAsync(DecisionContext<SelectSumData> context, CancellationToken ct = default)
            => Task.FromResult(BotActionUtilities.SelectSum(context.StateData));

        public virtual Task<IList<CardAttribute>> OnAnnounceAttributeAsync(DecisionContext<AnnounceAttributeData> context, CancellationToken ct = default)
        {
            List<CardAttribute> attributes = new();
            foreach (CardAttribute attribute in System.Enum.GetValues(typeof(CardAttribute)))
                if ((context.StateData.SelectableOptions ?? Array.Empty<byte>()).Contains((byte)attribute))
                    attributes.Add(attribute);

            Random rng = new();
            return Task.FromResult<IList<CardAttribute>>(attributes.OrderBy(_ => rng.Next()).Take(context.StateData.RequiredCount).ToList());
        }

        public virtual Task<IList<CardRace>> OnAnnounceRaceAsync(DecisionContext<AnnounceRaceData> context, CancellationToken ct = default)
        {
            List<CardRace> races = new();
            foreach (CardRace race in System.Enum.GetValues(typeof(CardRace)))
                if ((context.StateData.SelectableOptions ?? Array.Empty<int>()).Contains((int)race))
                    races.Add(race);

            Random rng = new();
            return Task.FromResult<IList<CardRace>>(races.OrderBy(_ => rng.Next()).Take(context.StateData.RequiredCount).ToList());
        }

        public virtual Task<int> OnAnnounceNumberAsync(DecisionContext<AnnounceNumberData> context, CancellationToken ct = default) =>
            Task.FromResult((context.StateData.SelectableOptions ?? Array.Empty<int>()).FirstOrDefault());

        public virtual async Task<int> OnAnnounceCardAsync(DecisionContext<AnnounceCardData> context, CancellationToken ct = default)
        {
            var cards = await DuelBotWrapper.GetMatchingCards(context.StateData.AppliableFilters ?? Array.Empty<int>());
            return BotActionUtilities.SelectRandomAnnouncedCard(cards);
        }

        public virtual async Task<int> OnAnnounceCardFilterAsync(DecisionContext<AnnounceCardFilterData> context, CancellationToken ct = default)
        {
            var cards = await DuelBotWrapper.GetMatchingCards(context.StateData.AppliableFilters ?? Array.Empty<int>());
            return BotActionUtilities.SelectRandomAnnouncedCard(cards);
        }

        public virtual Task<IList<byte>?> OnSelectUnselectCardAsync(DecisionContext<SelectUnselectCardData> context, CancellationToken ct = default)
        {
            return Task.FromResult<IList<byte>?>(BotActionUtilities.BuildSequentialListSelection(context.StateData.Minimum));
        }

        public virtual Task<IList<short>> OnSelectCounterAsync(DecisionContext<SelectCounterData> context, CancellationToken ct = default)
            => Task.FromResult(BotActionUtilities.SelectCounter(context.StateData));

        public virtual Task<int> OnSelectDisfieldAsync(DecisionContext<SelectDisfieldData> context, CancellationToken ct = default)
            => Task.FromResult(BotActionUtilities.SelectZone(context.StateData.Player, (byte)context.StateData.Location, context.StateData.Filter));

        public virtual Task<int> OnSelectPlaceAsync(DecisionContext<SelectPlaceData> context, CancellationToken ct = default)
            => Task.FromResult(BotActionUtilities.SelectZone(context.StateData.Player, (byte)context.StateData.Location, context.StateData.Filter));

        public virtual Task<bool> OnSelectEffectYnAsync(DecisionContext<SelectEffectYnData> context, CancellationToken ct = default) =>
            Task.FromResult(false);

        public virtual Task<bool> OnSelectYesNoAsync(DecisionContext<SelectYesNoData> context, CancellationToken ct = default) =>
            Task.FromResult(false);

        public virtual Task<int> OnSelectOptionAsync(DecisionContext<SelectOptionData> context, CancellationToken ct = default) =>
            Task.FromResult(0);

        public virtual Task<byte> OnSelectRockPaperScissorsAsync(CancellationToken ct = default) =>
            Task.FromResult(BotActionUtilities.SelectRandomRps());

        public virtual Task<CardPosition> OnSelectPositionAsync(DecisionContext<SelectPositionData> context, CancellationToken ct = default) =>
            Task.FromResult(BotActionUtilities.SelectPosition(context.StateData));

        public virtual Task<IList<byte>> OnSortCardAsync(DecisionContext<SortCardData> context, CancellationToken ct = default) =>
            Task.FromResult(BotActionUtilities.IdentitySort(context.StateData));

        protected virtual TResponse ChooseBest<TState, TResponse>(
            DecisionContext<TState> context,
            IEnumerable<CandidateAction> candidates,
            TResponse fallback)
        {
            var best = candidates
                .Select(candidate => _actionScorer.Score(context, candidate))
                .OrderByDescending(result => result.Score)
                .FirstOrDefault();

            return best == null ? fallback : best.Action.GetResponse<TResponse>();
        }

    }
}
