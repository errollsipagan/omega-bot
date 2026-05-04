using OmegaBotWrapper.Enum;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;
#pragma warning disable 1998
namespace OmegaBotWrapper
{
    /// <summary>
    /// Interface which can be implemented with all the callbacks which the bot will run
    /// </summary>
    public interface IBotActions
    {
        /// <summary>
        /// Wrapper instance which this instructions are linked to
        /// </summary>
        DuelBotWrapper? BotWrapper { get; set; }
        /// <summary>
        /// Name of the deck to be used
        /// </summary>
        string? DeckName { get; set; }
        /// <summary>
        /// Deck code to be used
        /// </summary>
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
            bot.OnSelectTributeAction = OnSelectTribute;
            bot.OnSelectSumAction = OnSelectSum;
            bot.OnAnnounceAttributeAction = OnAnnounceAttribute;
            bot.OnAnnounceRaceAction = OnAnnounceRace;
            bot.OnAnnounceNumberAction = OnAnnounceNumber;
            bot.OnAnnounceCardAction = OnAnnounceCard;
            bot.OnAnnounceCardFilterAction = OnAnnounceCardFilter;
            bot.OnSelectUnselectCardAction = OnSelectUnselectCard;
            bot.OnSelectCounterAction = OnSelectCounter;
            bot.OnSelectDisfieldAction = OnSelectDisfield;
            bot.OnSelectPlaceAction = OnSelectPlace;
            bot.OnSelectEffectYnAction = OnSelectEffectYn;
            bot.OnSelectYesNoAction = OnSelectYesNo;
            bot.OnSelectOptionAction = OnSelectOption;
            bot.OnSelectRockPaperScissors = OnSelectRockPaperScissors;
            bot.OnSelectPositionAction = OnSelectPosition;
            bot.OnSortCardAction = OnSortCard;
        }
        /// <summary>
        /// Action to be taken at battle phase, should return the action and its index (or 0 if not applicable)
        /// </summary>
        async Task<BattleCmdResponse> OnBattleCmd(BattleCmdData state)
        {
            return BotActionUtilities.DefaultBattle(state);
        }
        /// <summary>
        /// Action to be taken at main phase, should return the action and its index (or 0 if not applicable)
        /// </summary>
        async Task<IdleCmdResponse> OnIdleCmd(IdleCmdData state)
        {
            return BotActionUtilities.DefaultIdle(state);
        }
        /// <summary>
        /// Action to be taken when selecting something to chain, should return the index of the selected chain to activate or, if allowed, -1 for none
        /// </summary>
        async Task<int> OnSelectChain(SelectChainData state)
        {
            return BotActionUtilities.DefaultChain(state);
        }
        /// <summary>
        /// Action to be taken when selecting tribute(s), should return the indexes that was selected from the selectable options
        /// </summary>
        async Task<IList<byte>?> OnSelectCard(SelectCardData state)
        {
            return BotActionUtilities.BuildSequentialSelection(state.Minimum);
        }
        /// <summary>
        /// Action to be taken when selecting tribute(s), should return the indexes that was selected from the selectable options
        /// </summary>
        async Task<IList<byte>?> OnSelectTribute(SelectTributeData state)
        {
            return BotActionUtilities.BuildSequentialListSelection(state.Minimum);
        }
        /// <summary>
        /// Action to be taken when selecting a sum (eg. synchro/ritual), should return the indexes that was selected from the selectable options
        /// </summary>
        async Task<IList<byte>?> OnSelectSum(SelectSumData state)
        {
            return BotActionUtilities.SelectSum(state);
        }
        /// <summary>
        /// Action to be taken when announcing attribute(s), should return the attribute(s) selected
        /// </summary>
        async Task<IList<CardAttribute>> OnAnnounceAttribute(AnnounceAttributeData state)
        {
            return BotActionUtilities.SelectAttributes(state);
        }
        /// <summary>
        /// Action to be taken when announcing race(s), should return the race(s) selected
        /// </summary>
        async Task<IList<CardRace>> OnAnnounceRace(AnnounceRaceData state)
        {
            return BotActionUtilities.SelectRaces(state);
        }
        /// <summary>
        /// Action to be taken when announcing a number, should return the index of the number selected from the selectable options
        /// </summary>
        async Task<int> OnAnnounceNumber(AnnounceNumberData state)
        {
            return BotActionUtilities.SelectFirstNumber(state);
        }
        /// <summary>
        /// Action to be taken when announcing a card, should return the card id that was selected
        /// </summary>
        async Task<int> OnAnnounceCard(AnnounceCardData state)
        {
            var filters = state.AppliableFilters;
            var cards = await DuelBotWrapper.GetMatchingCards(filters);
            return BotActionUtilities.SelectRandomAnnouncedCard(cards);
        }
        /// <summary>
        /// Action to be taken when announcing a card with filter, should return the card id that was selected
        /// </summary>
        async Task<int> OnAnnounceCardFilter(AnnounceCardFilterData state)
        {
            var filters = state.AppliableFilters;
            var cards = await DuelBotWrapper.GetMatchingCards(filters);
            return BotActionUtilities.SelectRandomAnnouncedCard(cards);
        }
        /// <summary>
        /// Action to be taken when selecting/unselecting card(s), should return the indexes that was selected from the selectable options
        /// </summary>
        async Task<IList<byte>?> OnSelectUnselectCard(SelectUnselectCardData state)
        {
            return BotActionUtilities.BuildSequentialListSelection(state.Minimum);
        }
        /// <summary>
        /// Action to be taken when selecting counter(s), should return the amount of counters picked from each of the indexes that was present on the selectable options
        /// </summary>
        async Task<IList<short>> OnSelectCounter(SelectCounterData state)
        {
            return BotActionUtilities.SelectCounter(state);
        }
        /// <summary>
        /// Action to be taken when selecting field zone for disabling, should return the integer representing the zone selected (use the 'Zones' class helper for values)
        /// </summary>
        async Task<int> OnSelectDisfield(SelectDisfieldData state)
        {
            return BotActionUtilities.SelectZone(state.Player, (byte)state.Location, state.Filter);
        }
        /// <summary>
        /// Action to be taken when selecting field zone, should return the integer representing the zone selected (use the 'Zones' class helper for values)
        /// </summary>
        async Task<int> OnSelectPlace(SelectPlaceData state)
        {
            return BotActionUtilities.SelectZone(state.Player, (byte)state.Location, state.Filter);
        }
        /// <summary>
        /// Action to be taken when selecting yes or no for an effect, should return true or false
        /// </summary>
        async Task<bool> OnSelectEffectYn(SelectEffectYnData state)
        {
            return false;
        }
        /// <summary>
        /// Action to be taken when selecting yes or no, should return true or false
        /// </summary>
        async Task<bool> OnSelectYesNo(SelectYesNoData state)
        {
            return false;
        }
        /// <summary>
        /// Action to be taken when selecting an option, should return the index of the selected option
        /// </summary>
        async Task<int> OnSelectOption(SelectOptionData state)
        {
            return 0;
        }
        /// <summary>
        /// Action to be taken when selecting a rock paper scissors option, should return 1 = scissors, 2 = rock, 3 = paper 
        /// </summary>
        async Task<byte> OnSelectRockPaperScissors()
        {
            return BotActionUtilities.SelectRandomRps();
        }
        /// <summary>
        /// Action to be taken when selecting position, should return the position selected
        /// </summary>
        async Task<CardPosition> OnSelectPosition(SelectPositionData state)
        {
            return BotActionUtilities.SelectPosition(state);
        }
        /// <summary>
        /// Action to be taken when sorting cards, should return the indexes of the selectable cards in the desired order
        /// </summary>
        async Task<IList<byte>> OnSortCard(SortCardData state)
        {
            return BotActionUtilities.IdentitySort(state);
        }
    }
}
