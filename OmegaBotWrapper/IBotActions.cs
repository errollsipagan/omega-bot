using OmegaBotWrapper.Enum;
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
        async Task<Tuple<BattleAction, ushort>> OnBattleCmd(BattleCmdState state)
        {
            if (state.CurrentStateData.AttackableCards.Any())
                return new(BattleAction.Attack, (ushort)state.CurrentStateData.AttackableCards.First().ActionIndex[(int)BattleAction.Attack]);
            if (state.CurrentStateData.CanMainPhaseTwo)
                return new(BattleAction.ToMainPhaseTwo, 0);
            return new(BattleAction.ToEndPhase, 0);
        }
        /// <summary>
        /// Action to be taken at main phase, should return the action and its index (or 0 if not applicable)
        /// </summary>
        async Task<Tuple<MainAction, ushort>> OnIdleCmd(IdleCmdState state)
        {
            if (state.CurrentStateData.SummonableCards.Any())
                return new(MainAction.Summon, (ushort)state.CurrentStateData.SummonableCards.First().ActionIndex[(int)MainAction.Summon]);
            if (state.CurrentStateData.CanBattlePhase)
                return new(MainAction.ToBattlePhase, 0);
            return new(MainAction.ToEndPhase, 0);
        }
        /// <summary>
        /// Action to be taken when selecting something to chain, should return the index of the selected chain to activate or, if allowed, -1 for none
        /// </summary>
        async Task<int> OnSelectChain(SelectChainState state)
        {
            if (!state.CurrentStateData.Forced)
                return -1;
            return 0;
        }
        /// <summary>
        /// Action to be taken when selecting tribute(s), should return the indexes that was selected from the selectable options
        /// </summary>
        async Task<IList<byte>?> OnSelectCard(SelectCardState state)
        {
            byte[] selection = new byte[state.CurrentStateData.Minimum];
            for (int i = 0; i<selection.Length; i++)
                selection[i] = (byte) i;
            return selection;
        }
        /// <summary>
        /// Action to be taken when selecting tribute(s), should return the indexes that was selected from the selectable options
        /// </summary>
        async Task<IList<byte>?> OnSelectTribute(SelectTributeState state)
        {
            List<byte> resp = new();
            for (byte i = 0; i < state.CurrentStateData.Minimum; i++)
                resp.Add(i);
            return resp;
        }
        /// <summary>
        /// Action to be taken when selecting a sum (eg. synchro/ritual), should return the indexes that was selected from the selectable options
        /// </summary>
        async Task<IList<byte>?> OnSelectSum(SelectSumState state)
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
        // <summary>
        /// Action to be taken when announcing attribute(s), should return the attribute(s) selected
        /// </summary>
        async Task<IList<CardAttribute>> OnAnnounceAttribute(AnnounceAttributeState state)
        {
            List<CardAttribute> cas = new();
            foreach (CardAttribute ca in System.Enum.GetValues(typeof(CardAttribute)))
                if (state.CurrentStateData.SelectableOptions.Contains((byte)ca))
                    cas.Add(ca);
            Random rng = new();
            return cas.OrderBy(x => rng.Next()).Take(state.CurrentStateData.RequiredCount).ToList();
        }
        /// <summary>
        /// Action to be taken when announcing race(s), should return the race(s) selected
        /// </summary>
        async Task<IList<CardRace>> OnAnnounceRace(AnnounceRaceState state)
        {
            List<CardRace> cas = new();
            foreach (CardRace ca in System.Enum.GetValues(typeof(CardRace)))
                if (state.CurrentStateData.SelectableOptions.Contains((byte)ca))
                    cas.Add(ca);
            Random rng = new();
            return cas.OrderBy(x => rng.Next()).Take(state.CurrentStateData.RequiredCount).ToList();
        }
        /// <summary>
        /// Action to be taken when announcing a number, should return the index of the number selected from the selectable options
        /// </summary>
        async Task<int> OnAnnounceNumber(AnnounceNumberState state)
        {
            return state.CurrentStateData.SelectableOptions[0];
        }
        /// <summary>
        /// Action to be taken when announcing a card, should return the card id that was selected
        /// </summary>
        async Task<int> OnAnnounceCard(AnnounceCardState state)
        {
            var filters = state.CurrentStateData.AppliableFilters;
            if (BotWrapper == null) return 89631139; // Blue-eyes white dragon
            var cards = await BotWrapper.GetMatchingCards(filters);
            Random rng = new();
            return cards.Cards.ElementAt(rng.Next(0, cards.Cards.Length)).ID;
        }
        /// <summary>
        /// Action to be taken when announcing a card with filter, should return the card id that was selected
        /// </summary>
        async Task<int> OnAnnounceCardFilter(AnnounceCardFilterState state)
        {
            var filters = state.CurrentStateData.AppliableFilters;
            if (BotWrapper == null) return 89631139; // Blue-eyes white dragon
            var cards = await BotWrapper.GetMatchingCards(filters);
            Random rng = new();
            return cards.Cards.ElementAt(rng.Next(0, cards.Cards.Length)).ID;
        }
        /// <summary>
        /// Action to be taken when selecting/unselecting card(s), should return the indexes that was selected from the selectable options
        /// </summary>
        async Task<IList<byte>?> OnSelectUnselectCard(SelectUnselectCardState state)
        {
            List<byte> selection = new();
            for (int i = 0; i < state.CurrentStateData.Minimum; i++)
                selection[i] = (byte)i;
            return selection;
        }
        /// <summary>
        /// Action to be taken when selecting counter(s), should return the amount of counters picked from each of the indexes that was present on the selectable options
        /// </summary>
        async Task<IList<short>> OnSelectCounter(SelectCounterState state)
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
        /// Action to be taken when selecting field zone for disabling, should return the integer representing the zone selected (use the 'Zones' class helper for values)
        /// </summary>
        async Task<int> OnSelectDisfield(SelectDisfieldState state)
        {
            int[] zones = new int[] { Zones.z0, Zones.z1, Zones.z2, Zones.z3, Zones.z4, Zones.z5, Zones.z6 };
            byte seq;
            for (seq = 0; seq < 7; ++seq)
                if (Zones.IsAvailable(zones[seq], state.CurrentStateData.Filter))
                    break;
            return BitConverter.ToInt32(new byte[] { state.CurrentStateData.Player, (byte)state.CurrentStateData.Location, seq, 0 });
        }
        /// <summary>
        /// Action to be taken when selecting field zone, should return the integer representing the zone selected (use the 'Zones' class helper for values)
        /// </summary>
        async Task<int> OnSelectPlace(SelectPlaceState state)
        {
            int[] zones = new int[] { Zones.z0, Zones.z1, Zones.z2, Zones.z3, Zones.z4, Zones.z5, Zones.z6 };
            byte seq;
            for (seq = 0; seq < 7; ++seq)
                if (Zones.IsAvailable(zones[seq], state.CurrentStateData.Filter))
                    break;
            return BitConverter.ToInt32(new byte[] { state.CurrentStateData.Player, (byte)state.CurrentStateData.Location, seq, 0 });
        }
        /// <summary>
        /// Action to be taken when selecting yes or no for an effect, should return true or false
        /// </summary>
        async Task<bool> OnSelectEffectYn(SelectEffectYnState state)
        {
            return false;
        }
        /// <summary>
        /// Action to be taken when selecting yes or no, should return true or false
        /// </summary>
        async Task<bool> OnSelectYesNo(SelectYesNoState state)
        {
            return false;
        }
        /// <summary>
        /// Action to be taken when selecting an option, should return the index of the selected option
        /// </summary>
        async Task<int> OnSelectOption(SelectOptionState state)
        {
            return 0;
        }
        /// <summary>
        /// Action to be taken when selecting a rock paper scissors option, should return 1 = scissors, 2 = rock, 3 = paper 
        /// </summary>
        async Task<byte> OnSelectRockPaperScissors()
        {
            return (byte)new Random().Next(1, 4);
        }
        /// <summary>
        /// Action to be taken when selecting position, should return the position selected
        /// </summary>
        async Task<CardPosition> OnSelectPosition(SelectPositionState state)
        {
            return (CardPosition)state.CurrentStateData.Positions[0];
        }
        /// <summary>
        /// Action to be taken when sorting cards, should return the indexes of the selectable cards in the desired order
        /// </summary>
        async Task<IList<byte>> OnSortCard(SortCardState state)
        {
            List<byte> resp = new();
            for (byte i = 0; i < state.CurrentStateData.SelectableCards.Length; ++i)
                resp.Add(i);
            return resp;
        }
    }
}
