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
                    Response = card.DeclareAttack(),
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
                    Response = new BattleCmdResponse(BattleAction.ToMainPhaseTwo),
                    Order = int.MaxValue - 1
                });
            }

            if (context.StateData.CanEndPhase)
            {
                candidates.Add(new CandidateAction
                {
                    State = BotState.SelectBattleCmd,
                    Kind = CandidateActionKind.ToEndPhase,
                    Response = new BattleCmdResponse(BattleAction.ToEndPhase),
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
                    Response = card.NormalSummon(),
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
                    Response = new IdleCmdResponse(MainAction.ToBattlePhase),
                    Order = int.MaxValue - 1
                });
            }

            if (context.StateData.CanEndPhase)
            {
                candidates.Add(new CandidateAction
                {
                    State = BotState.SelectIdleCmd,
                    Kind = CandidateActionKind.ToEndPhase,
                    Response = new IdleCmdResponse(MainAction.ToEndPhase),
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
                    Response = i,
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
                    Response = -1,
                    Order = int.MaxValue
                });
            }

            int fallback = context.StateData.Forced ? 0 : -1;
            return Task.FromResult(ChooseBest(context, candidates, fallback));
        }

        public virtual Task<IList<byte>?> OnSelectCardAsync(DecisionContext<SelectCardData> context, CancellationToken ct = default)
        {
            byte[] selection = new byte[context.StateData.Minimum];
            for (int i = 0; i < selection.Length; i++)
                selection[i] = (byte)i;
            return Task.FromResult<IList<byte>?>(selection);
        }

        public virtual Task<IList<byte>?> OnSelectTributeAsync(DecisionContext<SelectTributeData> context, CancellationToken ct = default)
        {
            List<byte> response = new();
            for (byte i = 0; i < context.StateData.Minimum; i++)
                response.Add(i);
            return Task.FromResult<IList<byte>?>(response);
        }

        public virtual Task<IList<byte>?> OnSelectSumAsync(DecisionContext<SelectSumData> context, CancellationToken ct = default)
        {
            int min = context.StateData.Minimum;
            int max = context.StateData.Maximum;
            var cards = context.StateData.SelectableCards ?? Array.Empty<DynamicCard>();
            int sum = context.StateData.Sum;
            int comboSize = (min <= 1) ? 2 : min;

            while (comboSize <= max && comboSize <= cards.Length)
            {
                foreach (var combo in cards.GetCombinations(comboSize))
                {
                    int s1 = 0;
                    int s2 = 0;
                    foreach (var card in combo)
                    {
                        s1 += card.OpParam1;
                        s2 += card.OpParam2 != 0 ? card.OpParam2 : card.OpParam1;
                    }

                    if (s1 == sum || s2 == sum || (!context.StateData.MustBeExactValue && (s1 >= sum || s2 >= sum)))
                        return Task.FromResult<IList<byte>?>(combo.Select(c => (byte)Array.IndexOf(cards, c)).ToList());
                }

                comboSize++;
            }

            return Task.FromResult<IList<byte>?>(null);
        }

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
            if (cards.Cards == null || cards.Cards.Length == 0)
                return 0;
            return cards.Cards[new Random().Next(0, cards.Cards.Length)].ID;
        }

        public virtual async Task<int> OnAnnounceCardFilterAsync(DecisionContext<AnnounceCardFilterData> context, CancellationToken ct = default)
        {
            var cards = await DuelBotWrapper.GetMatchingCards(context.StateData.AppliableFilters ?? Array.Empty<int>());
            if (cards.Cards == null || cards.Cards.Length == 0)
                return 0;
            return cards.Cards[new Random().Next(0, cards.Cards.Length)].ID;
        }

        public virtual Task<IList<byte>?> OnSelectUnselectCardAsync(DecisionContext<SelectUnselectCardData> context, CancellationToken ct = default)
        {
            List<byte> selection = new();
            for (int i = 0; i < context.StateData.Minimum; i++)
                selection.Add((byte)i);
            return Task.FromResult<IList<byte>?>(selection);
        }

        public virtual Task<IList<short>> OnSelectCounterAsync(DecisionContext<SelectCounterData> context, CancellationToken ct = default)
        {
            List<short> response = new();
            short need = context.StateData.RequiredQuantity;
            short[] counters = context.StateData.Counters ?? Array.Empty<short>();

            for (int i = 0; i < counters.Length && need > 0; i++)
            {
                if (counters[i] > need)
                {
                    response.Add(need);
                    need = 0;
                    break;
                }

                response.Add(counters[i]);
                need -= counters[i];
            }

            return Task.FromResult<IList<short>>(response);
        }

        public virtual Task<int> OnSelectDisfieldAsync(DecisionContext<SelectDisfieldData> context, CancellationToken ct = default)
        {
            byte seq = FindFirstAvailableZone(context.StateData.Filter);
            return Task.FromResult(BuildZoneResponse(context.StateData.Player, (byte)context.StateData.Location, seq));
        }

        public virtual Task<int> OnSelectPlaceAsync(DecisionContext<SelectPlaceData> context, CancellationToken ct = default)
        {
            byte seq = FindFirstAvailableZone(context.StateData.Filter);
            return Task.FromResult(BuildZoneResponse(context.StateData.Player, (byte)context.StateData.Location, seq));
        }

        public virtual Task<bool> OnSelectEffectYnAsync(DecisionContext<SelectEffectYnData> context, CancellationToken ct = default) =>
            Task.FromResult(false);

        public virtual Task<bool> OnSelectYesNoAsync(DecisionContext<SelectYesNoData> context, CancellationToken ct = default) =>
            Task.FromResult(false);

        public virtual Task<int> OnSelectOptionAsync(DecisionContext<SelectOptionData> context, CancellationToken ct = default) =>
            Task.FromResult(0);

        public virtual Task<byte> OnSelectRockPaperScissorsAsync(CancellationToken ct = default) =>
            Task.FromResult((byte)new Random().Next(1, 4));

        public virtual Task<CardPosition> OnSelectPositionAsync(DecisionContext<SelectPositionData> context, CancellationToken ct = default) =>
            Task.FromResult((CardPosition)(context.StateData.Positions ?? Array.Empty<byte>()).FirstOrDefault((byte)CardPosition.FaceUpAttack));

        public virtual Task<IList<byte>> OnSortCardAsync(DecisionContext<SortCardData> context, CancellationToken ct = default)
        {
            List<byte> response = new();
            for (byte i = 0; i < (context.StateData.SelectableCards ?? Array.Empty<DynamicCard>()).Length; i++)
                response.Add(i);
            return Task.FromResult<IList<byte>>(response);
        }

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

        protected static byte FindFirstAvailableZone(int filter)
        {
            int[] zoneMasks = { Zones.z0, Zones.z1, Zones.z2, Zones.z3, Zones.z4, Zones.z5, Zones.z6 };
            for (byte seq = 0; seq < zoneMasks.Length; seq++)
                if (Zones.IsAvailable(zoneMasks[seq], filter))
                    return seq;
            return 0;
        }

        protected static int BuildZoneResponse(byte player, byte location, byte seq) =>
            BitConverter.ToInt32(new byte[] { player, location, seq, 0 });
    }
}
