using OmegaBotWrapper.Enum;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;

namespace OmegaBotWrapper
{
    /// <summary>
    /// Convenience base class for bots that want the wrapper's shared default behavior
    /// without re-implementing every callback.
    /// </summary>
    public abstract class BotActionsBase : IBotActions
    {
        public DuelBotWrapper? BotWrapper { get; set; }

        public string? DeckName { get; set; }

        public string? DeckHash { get; set; }

        public virtual void RegisterActions(DuelBotWrapper bot)
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

        public virtual Task<BattleCmdResponse> OnBattleCmd(BattleCmdData state) =>
            Task.FromResult(BotActionUtilities.DefaultBattle(state));

        public virtual Task<IdleCmdResponse> OnIdleCmd(IdleCmdData state) =>
            Task.FromResult(BotActionUtilities.DefaultIdle(state));

        public virtual Task<int> OnSelectChain(SelectChainData state) =>
            Task.FromResult(BotActionUtilities.DefaultChain(state));

        public virtual Task<IList<byte>?> OnSelectCard(SelectCardData state) =>
            Task.FromResult<IList<byte>?>(BotActionUtilities.BuildSequentialSelection(state.Minimum));

        public virtual Task<IList<byte>?> OnSelectTribute(SelectTributeData state) =>
            Task.FromResult<IList<byte>?>(BotActionUtilities.BuildSequentialListSelection(state.Minimum));

        public virtual Task<IList<byte>?> OnSelectSum(SelectSumData state) =>
            Task.FromResult(BotActionUtilities.SelectSum(state));

        public virtual Task<IList<CardAttribute>> OnAnnounceAttribute(AnnounceAttributeData state) =>
            Task.FromResult(BotActionUtilities.SelectAttributes(state));

        public virtual Task<IList<CardRace>> OnAnnounceRace(AnnounceRaceData state) =>
            Task.FromResult(BotActionUtilities.SelectRaces(state));

        public virtual Task<int> OnAnnounceNumber(AnnounceNumberData state) =>
            Task.FromResult(BotActionUtilities.SelectFirstNumber(state));

        public virtual async Task<int> OnAnnounceCard(AnnounceCardData state)
        {
            var cards = await DuelBotWrapper.GetMatchingCards(state.AppliableFilters);
            return BotActionUtilities.SelectRandomAnnouncedCard(cards);
        }

        public virtual async Task<int> OnAnnounceCardFilter(AnnounceCardFilterData state)
        {
            var cards = await DuelBotWrapper.GetMatchingCards(state.AppliableFilters);
            return BotActionUtilities.SelectRandomAnnouncedCard(cards);
        }

        public virtual Task<IList<byte>?> OnSelectUnselectCard(SelectUnselectCardData state) =>
            Task.FromResult<IList<byte>?>(BotActionUtilities.BuildSequentialListSelection(state.Minimum));

        public virtual Task<IList<short>> OnSelectCounter(SelectCounterData state) =>
            Task.FromResult(BotActionUtilities.SelectCounter(state));

        public virtual Task<int> OnSelectDisfield(SelectDisfieldData state) =>
            Task.FromResult(BotActionUtilities.SelectZone(state.Player, (byte)state.Location, state.Filter));

        public virtual Task<int> OnSelectPlace(SelectPlaceData state) =>
            Task.FromResult(BotActionUtilities.SelectZone(state.Player, (byte)state.Location, state.Filter));

        public virtual Task<bool> OnSelectEffectYn(SelectEffectYnData state) =>
            Task.FromResult(false);

        public virtual Task<bool> OnSelectYesNo(SelectYesNoData state) =>
            Task.FromResult(false);

        public virtual Task<int> OnSelectOption(SelectOptionData state) =>
            Task.FromResult(0);

        public virtual Task<byte> OnSelectRockPaperScissors() =>
            Task.FromResult(BotActionUtilities.SelectRandomRps());

        public virtual Task<CardPosition> OnSelectPosition(SelectPositionData state) =>
            Task.FromResult(BotActionUtilities.SelectPosition(state));

        public virtual Task<IList<byte>> OnSortCard(SortCardData state) =>
            Task.FromResult(BotActionUtilities.IdentitySort(state));
    }
}
