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
        /// <summary>
        /// Name of the deck to be used
        /// </summary>
        public string? DeckName { get; set; }
        /// <summary>
        /// Deck code to be used
        /// </summary>
        public string? DeckHash { get; set; }

        /// <summary>
        /// Wrapper instance which this instructions are linked to
        /// </summary>
        public DuelBotWrapper? BotWrapper { get; set; }

        /// <summary>
        /// Goes to end phase when on battle phase
        /// </summary>
        /// <param name="state">State of battle phase</param>
        /// <returns>Go to end phase command</returns>
        public async Task<Tuple<BattleAction, ushort>> OnBattleCmd(BattleCmdState state)
        {
            if (state.CurrentStateData.AttackableCards.Any())
                return new(BattleAction.Attack, (ushort)state.CurrentStateData.AttackableCards.First().ActionIndex[(int)BattleAction.Attack]);
            if (state.CurrentStateData.CanMainPhaseTwo)
                return new(BattleAction.ToMainPhaseTwo, 0);
            return new(BattleAction.ToEndPhase, 0);
        }
        /// <summary>
        /// Goes to end phase when on main phase
        /// </summary>
        /// <param name="state">State of main phase</param>
        /// <returns>Go to end phase command</returns>
        public async Task<Tuple<MainAction, ushort>> OnIdleCmd(IdleCmdState state)
        {
            if (state.CurrentStateData.SummonableCards.Any())
                return new(MainAction.Summon, (ushort)state.CurrentStateData.SummonableCards.First().ActionIndex[(int)MainAction.Summon]);
            if (state.CurrentStateData.CanBattlePhase)
                return new(MainAction.ToBattlePhase, 0);
            return new(MainAction.ToEndPhase, 0);
        }
        /// <summary>
        /// Says no to any chain request or first available if forced
        /// </summary>
        /// <param name="state">State of the chain request</param>
        /// <returns>Answer no to any chain request or first available if forced</returns>
        public async Task<int> OnSelectChain(SelectChainState state)
        {
            if (!state.CurrentStateData.Forced)
                return -1;
            return 0;
        }
        /// <summary>
        /// Select the least mandatory amount of cards in order when asked to pick cards with no distinction
        /// </summary>
        /// <param name="state">State of the select card request</param>
        /// <returns>Least amount of required cards in order they was received</returns>
        public async Task<IList<byte>?> OnSelectCard(SelectCardState state)
        {
            byte[] selection = new byte[state.CurrentStateData.Minimum];
            for (int i = 0; i < selection.Length; i++)
                selection[i] = (byte)i;
            return selection;
        }
        /// <summary>
        /// Select the first available cards for tribute
        /// </summary>
        /// <param name="state">Cards state for tribute selection</param>
        /// <returns>Least required amount of tributes in received order</returns>
        public async Task<IList<byte>?> OnSelectTribute(SelectTributeState state)
        {
            List<byte> resp = new();
            for (byte i = 0; i < state.CurrentStateData.Minimum; i++)
                resp.Add(i);
            return resp;
        }
        /// <summary>
        /// Called when has to tribute for synchro or ritual (select sum of values), selected the first possible combination found
        /// </summary>
        /// <param name="state">State data about the sum request</param>
        /// <returns>Indexes of the selected cards for the request</returns>
        public async Task<IList<byte>?> OnSelectSum(SelectSumState state)
        {
            int s1, s2;
            int min = state.CurrentStateData.Minimum;
            int max = state.CurrentStateData.Maximum;
            var cards = state.CurrentStateData.SelectableCards;
            int sum = state.CurrentStateData.Sum;
            int i = (min <= 1) ? 2 : min;
            while (i <= max && i <= cards.Length)
            {
                var combos = cards.GetCombinations(i);
                foreach (var combo in combos)
                {
                    s1 = 0;
                    s2 = 0;
                    foreach (var card in combo)
                    {
                        s1 += card.OpParam1;
                        s2 += (card.OpParam2 != 0) ? card.OpParam2 : card.OpParam1;
                    }
                    if (s1 == sum || s2 == sum || (!state.CurrentStateData.MustBeExactValue && (s1 >= sum || s2 >= sum)))
                    {
                        return combo.Select(c => (byte)Array.IndexOf(state.CurrentStateData.SelectableCards, c)).ToList();
                    }
                }
                i++;
            }
            return null; // Can't find any combination
        }
        /// <summary>
        /// Selectes least required amount of available attributes to pick from at random
        /// </summary>
        /// <param name="state">Attributes data to pick from</param>
        /// <returns>Least required amount of available attributes at random</returns>
        public async Task<IList<CardAttribute>> OnAnnounceAttribute(AnnounceAttributeState state)
        {
            List<CardAttribute> cas = new();
            foreach (CardAttribute ca in Enum.GetValues(typeof(CardAttribute)))
                if (state.CurrentStateData.SelectableOptions.Contains((byte)ca))
                    cas.Add(ca);
            Random rng = new();
            return cas.OrderBy(x => rng.Next()).Take(state.CurrentStateData.RequiredCount).ToList();
        }
        /// <summary>
        /// Selectes least required amount of available races to pick from at random
        /// </summary>
        /// <param name="state">Races data to pick from</param>
        /// <returns>Least required amount of available races at random</returns>
        public async Task<IList<CardRace>> OnAnnounceRace(AnnounceRaceState state)
        {
            List<CardRace> cas = new();
            foreach (CardRace ca in Enum.GetValues(typeof(CardRace)))
                if (state.CurrentStateData.SelectableOptions.Contains((byte)ca))
                    cas.Add(ca);
            Random rng = new();
            return cas.OrderBy(x => rng.Next()).Take(state.CurrentStateData.RequiredCount).ToList();
        }
        /// <summary>
        /// Select first selectable number
        /// </summary>
        /// <param name="state">Numbers to be selected</param>
        /// <returns>The first available number</returns>
        public async Task<int> OnAnnounceNumber(AnnounceNumberState state)
        {
            return state.CurrentStateData.SelectableOptions[0];
        }
        /// <summary>
        /// Select a random card from the possible matching cards to select from
        /// </summary>
        /// <param name="state">Announce card state</param>
        /// <returns>Card ID to announce</returns>
        public async Task<int> OnAnnounceCard(AnnounceCardState state)
        {
            var filters = state.CurrentStateData.AppliableFilters;
            if (BotWrapper == null) return 89631139; // Blue-eyes white dragon
            var cards = await BotWrapper.GetMatchingCards(filters);
            Random rng = new();
            return cards.Cards.ElementAt(rng.Next(0, cards.Cards.Length)).ID;
        }
        /// <summary>
        /// Select a random card from the possible matching cards to select from
        /// </summary>
        /// <param name="state">Announce card state</param>
        /// <returns>Card ID to announce</returns>
        public async Task<int> OnAnnounceCardFilter(AnnounceCardFilterState state)
        {
            var filters = state.CurrentStateData.AppliableFilters;
            if (BotWrapper == null) return 89631139; // Blue-eyes white dragon
            var cards = await BotWrapper.GetMatchingCards(filters);
            Random rng = new();
            return cards.Cards.ElementAt(rng.Next(0, cards.Cards.Length)).ID;
        }
        /// <summary>
        /// Select the least mandatory amount of cards in order when asked to pick cards with no distinction
        /// </summary>
        /// <param name="state">State of the select card request</param>
        /// <returns>Least amount of required cards in order they was received</returns>
        public async Task<IList<byte>?> OnSelectUnselectCard(SelectUnselectCardState state)
        {
            List<byte> selection = new();
            for (int i = 0; i < state.CurrentStateData.Minimum; i++)
                selection[i] = (byte)i;
            return selection;
        }
        /// <summary>
        /// Select the first available counters to be used on the required amount
        /// </summary>
        /// <param name="state">Counters data to be picked from</param>
        /// <returns>First counters available to fulfill the request</returns>
        public async Task<IList<short>> OnSelectCounter(SelectCounterState state)
        {
            List<short> resp = new();
            short need = state.CurrentStateData.RequiredQuantity;
            byte i = 0;
            while (need > 0)
            {
                var amt = state.CurrentStateData.Counters[i];
                if (amt > need)
                {
                    resp.Add(need);
                    break;
                }
                resp.Add(amt);
                need -= amt;
            }
            return resp;
        }
        /// <summary>
        /// Select the first available zone for disfield
        /// </summary>
        /// <param name="state">Disfield state information</param>
        /// <returns>First available zone for disfield</returns>
        public async Task<int> OnSelectDisfield(SelectDisfieldState state)
        {
            int[] zones = new int[] { Zones.z0, Zones.z1, Zones.z2, Zones.z3, Zones.z4, Zones.z5, Zones.z6 };
            byte seq;
            for (seq = 0; seq < 7; ++seq)
                if (Zones.IsAvailable(zones[seq], state.CurrentStateData.Filter))
                    break;
            return BitConverter.ToInt32(new byte[] { state.CurrentStateData.Player, (byte)state.CurrentStateData.Location, seq, 0 });
        }
        /// <summary>
        /// Select the first available zone
        /// </summary>
        /// <param name="state">Place state information</param>
        /// <returns>First available zone</returns>
        public async Task<int> OnSelectPlace(SelectPlaceState state)
        {
            int[] zones = new int[] { Zones.z0, Zones.z1, Zones.z2, Zones.z3, Zones.z4, Zones.z5, Zones.z6 };
            byte seq;
            for (seq = 0; seq < 7; ++seq)
                if (Zones.IsAvailable(zones[seq], state.CurrentStateData.Filter))
                    break;
            return BitConverter.ToInt32(new byte[] { state.CurrentStateData.Player, (byte)state.CurrentStateData.Location, seq, 0 });
        }
        /// <summary>
        /// Always answer NO to effects
        /// </summary>
        /// <param name="state">Effect state data</param>
        /// <returns>Answer NO</returns>

        public async Task<bool> OnSelectEffectYn(SelectEffectYnState state)
        {
            return false;
        }
        /// <summary>
        /// Always answer NO to questions
        /// </summary>
        /// <param name="state">Question data</param>
        /// <returns>Answer NO</returns>
        public async Task<bool> OnSelectYesNo(SelectYesNoState state)
        {
            return false;
        }
        /// <summary>
        /// Always select the first available option
        /// </summary>
        /// <param name="state">Options to pick from</param>
        /// <returns>First pickable option</returns>
        public async Task<int> OnSelectOption(SelectOptionState state)
        {
            return 0;
        }
        /// <summary>
        /// Picks a random between rock, paper and scissors
        /// </summary>
        /// <returns>Random RPS</returns>
        public async Task<byte> OnSelectRockPaperScissors()
        {
            return (byte)new Random().Next(1, 4);
        }
        /// <summary>
        /// Always select the first available position
        /// </summary>
        /// <param name="state">Positions available</param>
        /// <returns>First available position</returns>
        public async Task<CardPosition> OnSelectPosition(SelectPositionState state)
        {
            return (CardPosition)state.CurrentStateData.Positions[0];
        }
        /// <summary>
        /// Sort cards on the same order they came from
        /// </summary>
        /// <param name="state">Data of cards to be sorted</param>
        /// <returns>Cards on the same order they came for sort</returns>
        public async Task<IList<byte>> OnSortCard(SortCardState state)
        {
            List<byte> resp = new();
            for (byte i = 0; i < state.CurrentStateData.SelectableCards.Length; ++i)
                resp.Add(i);
            return resp;
        }
    }
}
