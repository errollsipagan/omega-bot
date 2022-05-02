using OmegaBotWrapper;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.Struct;
#pragma warning disable 1998
namespace BotTest
{
    /// <summary>
    /// Sample bot that implements the bot actions interface which simply ends their turn, say no to chains and select the least amount of cards when asked to
    /// (eg. At end phase when has more cards in hand than the hand size limit)
    /// </summary>
    public class Actions : IBotActions
    {
        public string? DeckName { get; set; }
        public string? DeckHash { get; set; }

        /// <summary>
        /// Goes to end phase when on battle phase
        /// </summary>
        /// <param name="state">State of battle phase</param>
        /// <returns>Go to end phase command</returns>
        public async Task<Tuple<BattleAction, ushort>> OnBattleCmd(BattleCmdState state)
        {
            return new(BattleAction.ToEndPhase, 0);
        }
        /// <summary>
        /// Goes to end phase when on main phase
        /// </summary>
        /// <param name="state">State of main phase</param>
        /// <returns>Go to end phase command</returns>
        public async Task<Tuple<MainAction, ushort>> OnIdleCmd(IdleCmdState state)
        {
            return new(MainAction.ToEndPhase, 0);
        }
        /// <summary>
        /// Says no to any chain request
        /// </summary>
        /// <param name="state">State of the chain request</param>
        /// <returns>Answer no to any chain request</returns>
        public async Task<int> OnSelectChain(SelectChainState state)
        {
            return -1;
        }
        /// <summary>
        /// Select the least mandatory amount of cards in order when asked to pick cards with no distinction
        /// </summary>
        /// <param name="state">State of the select card request</param>
        /// <returns>Least amount of required cards in order they was received</returns>
        public async Task<IList<byte>?> OnSelectCard(SelectCardState state)
        {
            if (state.CurrentStateData.Cancelable) return null;
            byte[] selection = new byte[state.CurrentStateData.Minimum];
            for (int i = 0; i < selection.Length; i++)
                selection[i] = (byte)i;
            return selection;
        }

        public Task<IList<byte>?> OnSelectTribute(SelectTributeState state)
        {
            throw new NotImplementedException();
        }

        public Task<IList<byte>?> OnSelectSum(SelectSumState state)
        {
            throw new NotImplementedException();
        }

        public Task<IList<CardAttribute>> OnAnnounceAttribute(AnnounceAttributeState state)
        {
            throw new NotImplementedException();
        }

        public Task<IList<CardRace>> OnAnnounceRace(AnnounceRaceState state)
        {
            throw new NotImplementedException();
        }

        public Task<int> OnAnnounceNumber(AnnounceNumberState state)
        {
            throw new NotImplementedException();
        }

        public Task<int> OnAnnounceCard(AnnounceCardState state)
        {
            throw new NotImplementedException();
        }

        public Task<int> OnAnnounceCardFilter(AnnounceCardFilterState state)
        {
            throw new NotImplementedException();
        }

        public Task<IList<byte>?> OnSelectUnselectCard(SelectUnselectCardState state)
        {
            throw new NotImplementedException();
        }

        public Task<IList<short>> OnSelectCounter(SelectCounterState state)
        {
            throw new NotImplementedException();
        }

        public Task<int> OnSelectDisfield(SelectDisfieldState state)
        {
            throw new NotImplementedException();
        }

        public Task<int> OnSelectPlace(SelectPlaceState state)
        {
            throw new NotImplementedException();
        }

        public Task<bool> OnSelectEffectYn(SelectEffectYnState state)
        {
            throw new NotImplementedException();
        }

        public Task<bool> OnSelectYesNo(SelectYesNoState state)
        {
            throw new NotImplementedException();
        }

        public Task<int> OnSelectOption(SelectOptionState state)
        {
            throw new NotImplementedException();
        }

        public Task<byte> OnSelectRockPaperScissors()
        {
            throw new NotImplementedException();
        }

        public Task<CardPosition> OnSelectPosition(SelectPositionState state)
        {
            throw new NotImplementedException();
        }

        public Task<IList<byte>> OnSortCard(SortCardState state)
        {
            throw new NotImplementedException();
        }
    }
}
