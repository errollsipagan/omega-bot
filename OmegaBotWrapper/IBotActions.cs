using OmegaBotWrapper.Enum;
using OmegaBotWrapper.Struct;

namespace OmegaBotWrapper
{
    /// <summary>
    /// Interface which can be implemented with all the callbacks which the bot will run
    /// </summary>
    public interface IBotActions
    {
        string? DeckName { get; set; }
        string? DeckHash { get; set; }
        /// <summary>
        /// Registers all the actions the bot should take during specific states
        /// </summary>
        /// <param name="bot">Bot instance which actions are being registered to</param>
        public void RegisterActions(DuelBotWrapper bot)
        {
            bot.OnBattleCmdAction = OnBattleCmd;
            bot.OnIdleCmdAction = OnIdleCmd;
            bot.OnSelectChainAction = OnSelectChain;
            bot.OnSelectCardAction = OnSelectCard;
            bot.OnSelectTributeAction += OnSelectTribute;
            bot.OnSelectSumAction += OnSelectSum;
            bot.OnAnnounceAttributeAction += OnAnnounceAttribute;
            bot.OnAnnounceRaceAction += OnAnnounceRace;
            bot.OnAnnounceNumberAction += OnAnnounceNumber;
            bot.OnAnnounceCardAction += OnAnnounceCard;
            bot.OnAnnounceCardFilterAction += OnAnnounceCardFilter;
            bot.OnSelectUnselectCardAction += OnSelectUnselectCard;
            bot.OnSelectCounterAction += OnSelectCounter;
            bot.OnSelectDisfieldAction += OnSelectDisfield;
            bot.OnSelectPlaceAction += OnSelectPlace;
            bot.OnSelectEffectYnAction += OnSelectEffectYn;
            bot.OnSelectYesNoAction += OnSelectYesNo;
            bot.OnSelectOptionAction += OnSelectOption;
            bot.OnSelectRockPaperScissors += OnSelectRockPaperScissors;
            bot.OnSelectPositionAction += OnSelectPosition;
            bot.OnSortCardAction += OnSortCard;
        }
        /// <summary>
        /// Action to be taken at battle phase, should return the action and its index (or 0 if not applicable)
        /// </summary>
        Task<Tuple<BattleAction, ushort>> OnBattleCmd(BattleCmdState state);
        /// <summary>
        /// Action to be taken at main phase, should return the action and its index (or 0 if not applicable)
        /// </summary>
        Task<Tuple<MainAction, ushort>> OnIdleCmd(IdleCmdState state);
        /// <summary>
        /// Action to be taken when selecting something to chain, should return the index of the selected chain to activate or, if allowed, -1 for none
        /// </summary>
        Task<int> OnSelectChain(SelectChainState state);
        /// <summary>
        /// Action to be taken when selecting tribute(s), should return the indexes that was selected from the selectable options
        /// </summary>
        Task<IList<byte>?> OnSelectCard(SelectCardState state);
        /// <summary>
        /// Action to be taken when selecting tribute(s), should return the indexes that was selected from the selectable options
        /// </summary>
        Task<IList<byte>?> OnSelectTribute(SelectTributeState state);
        /// <summary>
        /// Action to be taken when selecting a sum (eg. synchro/ritual), should return the indexes that was selected from the selectable options
        /// </summary>
        Task<IList<byte>?> OnSelectSum(SelectSumState state);
        // <summary>
        /// Action to be taken when announcing attribute(s), should return the attribute(s) selected
        /// </summary>
        Task<IList<CardAttribute>> OnAnnounceAttribute(AnnounceAttributeState state);
        /// <summary>
        /// Action to be taken when announcing race(s), should return the race(s) selected
        /// </summary>
        Task<IList<CardRace>> OnAnnounceRace(AnnounceRaceState state);
        /// <summary>
        /// Action to be taken when announcing a number, should return the index of the number selected from the selectable options
        /// </summary>
        Task<int> OnAnnounceNumber(AnnounceNumberState state);
        /// <summary>
        /// Action to be taken when announcing a card, should return the card id that was selected
        /// </summary>
        Task<int> OnAnnounceCard(AnnounceCardState state);
        /// <summary>
        /// Action to be taken when announcing a card with filter, should return the card id that was selected
        /// </summary>
        Task<int> OnAnnounceCardFilter(AnnounceCardFilterState state);
        /// <summary>
        /// Action to be taken when selecting/unselecting card(s), should return the indexes that was selected from the selectable options
        /// </summary>
        Task<IList<byte>?> OnSelectUnselectCard(SelectUnselectCardState state);
        /// <summary>
        /// Action to be taken when selecting counter(s), should return the amount of counters picked from each of the indexes that was present on the selectable options
        /// </summary>
        Task<IList<short>> OnSelectCounter(SelectCounterState state);
        /// <summary>
        /// Action to be taken when selecting field zone for disabling, should return the integer representing the zone selected (use the 'Zones' class helper for values)
        /// </summary>
        Task<int> OnSelectDisfield(SelectDisfieldState state);
        /// <summary>
        /// Action to be taken when selecting field zone, should return the integer representing the zone selected (use the 'Zones' class helper for values)
        /// </summary>
        Task<int> OnSelectPlace(SelectPlaceState state);
        /// <summary>
        /// Action to be taken when selecting yes or no for an effect, should return true or false
        /// </summary>
        Task<bool> OnSelectEffectYn(SelectEffectYnState state);
        /// <summary>
        /// Action to be taken when selecting yes or no, should return true or false
        /// </summary>
        Task<bool> OnSelectYesNo(SelectYesNoState state);
        /// <summary>
        /// Action to be taken when selecting an option, should return the index of the selected option
        /// </summary>
        Task<int> OnSelectOption(SelectOptionState state);
        /// <summary>
        /// Action to be taken when selecting a rock paper scissors option, should return 1 = scissors, 2 = rock, 3 = paper 
        /// </summary>
        Task<byte> OnSelectRockPaperScissors();
        /// <summary>
        /// Action to be taken when selecting position, should return the position selected
        /// </summary>
        Task<CardPosition> OnSelectPosition(SelectPositionState state);
        /// <summary>
        /// Action to be taken when sorting cards, should return the indexes of the selectable cards in the desired order
        /// </summary>
        Task<IList<byte>> OnSortCard(SortCardState state);
    }
}
