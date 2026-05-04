using OmegaBotWrapper.Enum;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;

namespace OmegaBotWrapper.Heuristics
{
    /// <summary>
    /// Allows a heuristic brain to plug into the existing IBotActions pipeline.
    /// Existing decks can stay as-is; new decks can opt into IBotBrain directly.
    /// </summary>
    public sealed class BotBrainActionsAdapter : IBotActions
    {
        private readonly IBotBrain _brain;

        public BotBrainActionsAdapter(IBotBrain brain, string? deckName = null, string? deckHash = null)
        {
            _brain = brain;
            DeckName = deckName;
            DeckHash = deckHash;
        }

        public DuelBotWrapper? BotWrapper { get; set; }

        public string? DeckName { get; set; }

        public string? DeckHash { get; set; }

        public Task<BattleCmdResponse> OnBattleCmd(BattleCmdData state) =>
            _brain.OnBattleCmdAsync(CreateContext(BotState.SelectBattleCmd, state, includeFieldSnapshot: true));

        public Task<IdleCmdResponse> OnIdleCmd(IdleCmdData state) =>
            _brain.OnIdleCmdAsync(CreateContext(BotState.SelectIdleCmd, state, includeFieldSnapshot: true));

        public Task<int> OnSelectChain(SelectChainData state) =>
            _brain.OnSelectChainAsync(CreateContext(BotState.SelectChain, state));

        public Task<IList<byte>?> OnSelectCard(SelectCardData state) =>
            _brain.OnSelectCardAsync(CreateContext(BotState.SelectCard, state));

        public Task<IList<byte>?> OnSelectTribute(SelectTributeData state) =>
            _brain.OnSelectTributeAsync(CreateContext(BotState.SelectTribute, state));

        public Task<IList<byte>?> OnSelectSum(SelectSumData state) =>
            _brain.OnSelectSumAsync(CreateContext(BotState.SelectSum, state));

        public Task<IList<CardAttribute>> OnAnnounceAttribute(AnnounceAttributeData state) =>
            _brain.OnAnnounceAttributeAsync(CreateContext(BotState.AnnounceAttribute, state));

        public Task<IList<CardRace>> OnAnnounceRace(AnnounceRaceData state) =>
            _brain.OnAnnounceRaceAsync(CreateContext(BotState.AnnounceRace, state));

        public Task<int> OnAnnounceNumber(AnnounceNumberData state) =>
            _brain.OnAnnounceNumberAsync(CreateContext(BotState.AnnounceNumber, state));

        public Task<int> OnAnnounceCard(AnnounceCardData state) =>
            _brain.OnAnnounceCardAsync(CreateContext(BotState.AnnounceCard, state));

        public Task<int> OnAnnounceCardFilter(AnnounceCardFilterData state) =>
            _brain.OnAnnounceCardFilterAsync(CreateContext(BotState.AnnounceCardFilter, state));

        public Task<IList<byte>?> OnSelectUnselectCard(SelectUnselectCardData state) =>
            _brain.OnSelectUnselectCardAsync(CreateContext(BotState.SelectUnselectCard, state));

        public Task<IList<short>> OnSelectCounter(SelectCounterData state) =>
            _brain.OnSelectCounterAsync(CreateContext(BotState.SelectCounter, state));

        public Task<int> OnSelectDisfield(SelectDisfieldData state) =>
            _brain.OnSelectDisfieldAsync(CreateContext(BotState.SelectDisfield, state));

        public Task<int> OnSelectPlace(SelectPlaceData state) =>
            _brain.OnSelectPlaceAsync(CreateContext(BotState.SelectPlace, state));

        public Task<bool> OnSelectEffectYn(SelectEffectYnData state) =>
            _brain.OnSelectEffectYnAsync(CreateContext(BotState.SelectEffectYn, state));

        public Task<bool> OnSelectYesNo(SelectYesNoData state) =>
            _brain.OnSelectYesNoAsync(CreateContext(BotState.SelectYesNo, state));

        public Task<int> OnSelectOption(SelectOptionData state) =>
            _brain.OnSelectOptionAsync(CreateContext(BotState.SelectOption, state));

        public Task<byte> OnSelectRockPaperScissors() =>
            _brain.OnSelectRockPaperScissorsAsync();

        public Task<CardPosition> OnSelectPosition(SelectPositionData state) =>
            _brain.OnSelectPositionAsync(CreateContext(BotState.SelectPosition, state));

        public Task<IList<byte>> OnSortCard(SortCardData state) =>
            _brain.OnSortCardAsync(CreateContext(BotState.SortCard, state));

        private DecisionContext<TState> CreateContext<TState>(BotState state, TState stateData, bool includeFieldSnapshot = false)
        {
            if (BotWrapper == null)
                throw new InvalidOperationException("BotWrapper must be assigned before invoking brain actions.");

            FieldInfo? field = null;
            if (includeFieldSnapshot)
            {
                try
                {
                    field = BotWrapper.GetBotFieldInfo().GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    BotLogger.Warn(BotWrapper.BotID, $"[{state}] Failed to fetch optional field snapshot for brain evaluation: {ex.Message}");
                }
            }

            return new DecisionContext<TState>
            {
                Bot = BotWrapper,
                State = state,
                Header = BotWrapper.CurrentStateHeader,
                StateData = stateData,
                Field = field
            };
        }
    }
}
