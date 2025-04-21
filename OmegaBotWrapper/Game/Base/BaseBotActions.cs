using OmegaBotWrapper.Enum;
using OmegaBotWrapper.Game.AI.Actions;
using OmegaBotWrapper.Game.Field;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;
using OmegaBotWrapper.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#pragma warning disable 1998

namespace OmegaBotWrapper.Game.Base
{
    /// <summary>
    /// Abstract base class implementing IBotActions, providing default bot action implementations that can be overridden.
    /// </summary>
    public abstract class BaseBotActions : IBotActions
    {
        /// <summary>
        /// Wrapper instance which this instructions are linked to
        /// </summary>
        public DuelBotWrapper? BotWrapper { get; set; }

        /// <summary>
        /// Name of the deck to be used
        /// </summary>
        public string? DeckName { get; set; }

        /// <summary>
        /// Deck code to be used
        /// </summary>
        public string? DeckHash { get; set; }

        // Armazena o índice do defensor selecionado para uso em OnSelectCard
        private byte? _selectedDefenderIndex;

        /// <summary>
        /// Registers all the actions the bot should take during specific states
        /// </summary>
        /// <param name="bot">Bot instance which actions are being registered to</param>
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

        /// <summary>
        /// Action to be taken at battle phase, should return the action and its index (or 0 if not applicable)
        /// </summary>
        public virtual async Task<BattleCmdResponse> OnBattleCmd(BattleCmdData state)
        {
            ConsoleUtils.PrintColored("OnBattleCmd - Optimized Smart Attack Logic with Defender Index Tracking");

            await GameField.Initialize();

            // Gets the BOT (Enemy) monsters that can attack, sorted by attack (highest first)
            var attackableCards = state.AttackableCards
                .Where(card => !card.Attacked && !card.IsDisabled()) // Ignores cards that have already attacked or are disabled
                .OrderByDescending(card => card.Attack) // Prioritizes monsters with higher ATK
                .ToList();

            if (!attackableCards.Any())
            {
                ConsoleUtils.PrintColored("No attackable cards available.");
                _selectedDefenderIndex = null; // Resets the selected defender
                if (state.CanMainPhaseTwo)
                    return BattleAction.ToMainPhaseTwo;
                return BattleAction.ToEndPhase;
            }

            // Obtains player's (Own) monsters in the MonsterZone, with original indexes
            var opponentMonsters = (await GameField.GetCardsInLocation(CardLocation.MonsterZone, controller: 0))
                .Select((card, index) => new { Card = card, OriginalIndex = index })
                .Where(x => !x.Card.IsDisabled()) // Ignora monstros desabilitados
                .OrderByDescending(x => x.Card.IsDefense() ? x.Card.Defense : x.Card.Attack) // Prioritizes higher DEF or ATK
                .ToList();

            // Check direct attack
            if (!opponentMonsters.Any())
            {
                var directAttacker = attackableCards.FirstOrDefault(card => card.CanDirectAttack);
                if (directAttacker.ID > 0 && directAttacker.ID > 0 && state.AttackableCards.TryGetByID(directAttacker.ID, out DynamicCard _, out int index))
                {
                    ConsoleUtils.PrintColored($"Direct Attack with Card ID: {directAttacker.ID}, Attack: {directAttacker.Attack}, Index: {index}");
                    _selectedDefenderIndex = null; // No defender for direct attack
                    return new BattleCmdResponse(BattleAction.Attack, (ushort)index);
                }
                ConsoleUtils.PrintColored("No direct attack possible.");
                _selectedDefenderIndex = null;
                if (state.CanMainPhaseTwo)
                    return BattleAction.ToMainPhaseTwo;
                return BattleAction.ToEndPhase;
            }

            // Select the attacker with the highest attack
            var bestAttacker = attackableCards.First(); // First in the ordered list (highest attack)
            int attackerPower = bestAttacker.GetRealPower();
            ConsoleUtils.PrintColored($"Selected best attacker: ID {bestAttacker.ID}, Power {attackerPower}");

            // Try to attack the defender with the highest defense (or face-down)
            foreach (var target in opponentMonsters)
            {
                var defender = target.Card;
                bool isFaceUp = defender.IsFaceup();
                int defenderPower = defender.GetRealPower();
                int defenderIndex = target.OriginalIndex;

                if (!await new DefaultActions().OnPreBattleBetween(bestAttacker, defender))
                    continue;

                // Check if the attack is advantageous
                if (isFaceUp && attackerPower > defenderPower)
                {
                    if (state.AttackableCards.TryGetByID(bestAttacker.ID, out DynamicCard _, out int index))
                    {
                        ConsoleUtils.PrintColored($"Attacking face-up monster (ID: {defender.ID}, Power: {defenderPower}, Field Index: {defenderIndex}) with Card ID: {bestAttacker.ID}, Power: {attackerPower}, Attacker Index: {index}");
                        _selectedDefenderIndex = (byte)defenderIndex; // store index defender
                        return state.AttackableCards[(ushort)index].DeclareAttack();
                        //return new BattleCmdResponse(BattleAction.Attack, (ushort)index);
                    }
                }
                else if (!isFaceUp && defender.IsDefense())
                {
                    if (state.AttackableCards.TryGetByID(bestAttacker.ID, out DynamicCard _, out int index))
                    {
                        ConsoleUtils.PrintColored($"Attacking face-down monster (ID: {defender.ID}, Field Index: {defenderIndex}) with Card ID: {bestAttacker.ID}, Power: {attackerPower}, Attacker Index: {index}");
                        _selectedDefenderIndex = (byte)defenderIndex; // store index defender
                        return new BattleCmdResponse(BattleAction.Attack, (ushort)index);
                    }
                }
            }

            // If there are no advantageous targets, move to Main Phase 2 or end the turn.
            ConsoleUtils.PrintColored("No advantageous attack targets found.");
            _selectedDefenderIndex = null; // Resets the selected defender
            if (state.CanMainPhaseTwo)
                return BattleAction.ToMainPhaseTwo;
            return BattleAction.ToEndPhase;
        }

        /// <summary>
        /// Action to be taken at main phase, should return the action and its index (or 0 if not applicable)
        /// </summary>
        public virtual async Task<IdleCmdResponse> OnIdleCmd(IdleCmdData state)
        {
            ConsoleUtils.PrintColored("OnIdleCmd");

            if (state.SummonableCards.Any())
                return state.SummonableCards.First().NormalSummon();
            if (state.CanBattlePhase)
                return MainAction.ToBattlePhase;
            return MainAction.ToEndPhase;
        }

        /// <summary>
        /// Action to be taken when selecting something to chain, should return the index of the selected chain to activate or, if allowed, -1 for none
        /// </summary>
        public virtual async Task<int> OnSelectChain(SelectChainData state)
        {
            ConsoleUtils.PrintColored("OnSelectChain");

            if (!state.Forced)
                return -1;
            return 0;
        }

        /// <summary>
        /// Action to be taken when selecting card(s), should return the indexes that were selected from the selectable options
        /// </summary>
        public virtual async Task<IList<byte>?> OnSelectCard(SelectCardData state)
        {
            if (state.SelectionHint.Equals((int)DHint.ATTACKTARGET))
            {
                ConsoleUtils.PrintColored("OnSelectCard - Selecting Defender");

                if (_selectedDefenderIndex.HasValue)
                {
                    // Checks whether defenderIndex is valid and meets selection requirements
                    if (state.Minimum <= 1 && state.Maximum >= 1)
                    {
                        ConsoleUtils.PrintColored($"Selecting defender with index: {_selectedDefenderIndex.Value}");
                        return new List<byte> { _selectedDefenderIndex.Value };
                    }
                }
            }

            if (state.SelectionHint.Equals((int)DHint.DESTROY))
            {
                ConsoleUtils.PrintColored("OnSelectCard - Selecting Card to DESTROY");

                // Filter cards where Owner = 1 (Player) because using Player Vision
                var botCards = state.SelectableCards.Select((card, index) => new { Card = card, Index = index })
                                                    .Where(x => x.Card.Owner == 1)
                                                    .Take(state.Minimum)
                                                    .Select(x => (byte)x.Index).ToArray();
                // Respect Minimum (or Maximum if needed)
                if (botCards.Any())
                {
                    ConsoleUtils.PrintColored($"Selected {botCards.Length} bot-owned cards for destruction.");
                    return botCards;
                }
            }

            // Default logic if defenderIndex is not valid or not available
            ConsoleUtils.PrintColored("No specific defender index available, using default selection.");
            byte[] selection = new byte[state.Minimum];
            for (int i = 0; i < selection.Length; i++)
                selection[i] = (byte)i;
            return selection;
        }

        /// <summary>
        /// Action to be taken when selecting tribute(s), should return the indexes that were selected from the selectable options
        /// </summary>
        public virtual async Task<IList<byte>?> OnSelectTribute(SelectTributeData state)
        {
            ConsoleUtils.PrintColored("OnSelectTribute");

            List<byte> resp = new();
            for (byte i = 0; i < state.Minimum; i++)
                resp.Add(i);
            return resp;
        }

        /// <summary>
        /// Action to be taken when selecting a sum (e.g., synchro/ritual), should return the indexes that were selected from the selectable options
        /// </summary>
        public virtual async Task<IList<byte>?> OnSelectSum(SelectSumData state)
        {
            ConsoleUtils.PrintColored("OnSelectSum");

            int sum = state.Sum;
            int min = state.Minimum;
            int max = state.Maximum;
            var cards = state.SelectableCards;

            // Check if a single card satisfies the sum
            for (int i = 0; i < cards.Length; i++)
            {
                var card = cards[i];
                int param1 = card.OpParam1;
                int param2 = card.OpParam2 != 0 ? card.OpParam2 : card.OpParam1;

                // If a single card has param1 or param2 equal to sum and min <= 1
                if ((param1 == sum || param2 == sum) && min <= 1)
                {
                    return new List<byte> { (byte)i };
                }
            }

            // Check combinations of more cards if necessary
            int start = Math.Max(1, min); // Start with the minimum number of cards
            for (int count = start; count <= max && count <= cards.Length; count++)
            {
                var combos = cards.GetCombinations(count);
                foreach (var combo in combos)
                {
                    int sumParam1 = 0;
                    int sumParam2 = 0;
                    foreach (var card in combo)
                    {
                        sumParam1 += card.OpParam1;
                        sumParam2 += card.OpParam2 != 0 ? card.OpParam2 : card.OpParam1;
                    }

                    // Check if the sum is exactly equal to sum
                    if (sumParam1 == sum || sumParam2 == sum)
                    {
                        return combo.Select(c => (byte)Array.IndexOf(state.SelectableCards, c)).ToList();
                    }

                    // If it doesn't need to be exact, check if it is greater than or equal to
                    if (!state.MustBeExactValue && (sumParam1 >= sum || sumParam2 >= sum))
                    {
                        return combo.Select(c => (byte)Array.IndexOf(state.SelectableCards, c)).ToList();
                    }
                }
            }

            return null; // Can't find any combination
        }

        /// <summary>
        /// Action to be taken when announcing attribute(s), should return the attribute(s) selected
        /// </summary>
        public virtual async Task<IList<CardAttribute>> OnAnnounceAttribute(AnnounceAttributeData state)
        {
            ConsoleUtils.PrintColored("OnAnnounceAttribute");

            List<CardAttribute> cas = new();
            foreach (CardAttribute ca in System.Enum.GetValues(typeof(CardAttribute)))
                if (state.SelectableOptions.Contains((byte)ca))
                    cas.Add(ca);
            Random rng = new();
            return cas.OrderBy(x => rng.Next()).Take(state.RequiredCount).ToList();
        }

        /// <summary>
        /// Action to be taken when announcing race(s), should return the race(s) selected
        /// </summary>
        public virtual async Task<IList<CardRace>> OnAnnounceRace(AnnounceRaceData state)
        {
            ConsoleUtils.PrintColored("OnAnnounceRace");

            List<CardRace> cas = new();
            foreach (CardRace ca in System.Enum.GetValues(typeof(CardRace)))
                if (state.SelectableOptions.Contains((byte)ca))
                    cas.Add(ca);
            Random rng = new();
            return cas.OrderBy(x => rng.Next()).Take(state.RequiredCount).ToList();
        }

        /// <summary>
        /// Action to be taken when announcing a number, should return the index of the number selected from the selectable options
        /// </summary>
        public virtual async Task<int> OnAnnounceNumber(AnnounceNumberData state)
        {
            ConsoleUtils.PrintColored("OnAnnounceNumber");

            return state.SelectableOptions[0];
        }

        /// <summary>
        /// Action to be taken when announcing a card, should return the card id that was selected
        /// </summary>
        public virtual async Task<int> OnAnnounceCard(AnnounceCardData state)
        {
            ConsoleUtils.PrintColored("OnAnnounceCard");

            var filters = state.AppliableFilters;
            var cards = await DuelBotWrapper.GetMatchingCards(filters);
            Random rng = new();
            return cards.Cards.ElementAt(rng.Next(0, cards.Cards.Length)).ID;
        }

        /// <summary>
        /// Action to be taken when announcing a card with filter, should return the card id that was selected
        /// </summary>
        public virtual async Task<int> OnAnnounceCardFilter(AnnounceCardFilterData state)
        {
            ConsoleUtils.PrintColored("OnAnnounceCardFilter");

            var filters = state.AppliableFilters;
            var cards = await DuelBotWrapper.GetMatchingCards(filters);
            Random rng = new();
            return cards.Cards.ElementAt(rng.Next(0, cards.Cards.Length)).ID;
        }

        /// <summary>
        /// Action to be taken when selecting/unselecting card(s), should return the indexes that were selected from the selectable options
        /// </summary>
        public virtual async Task<IList<byte>?> OnSelectUnselectCard(SelectUnselectCardData state)
        {
            ConsoleUtils.PrintColored("OnSelectUnselectCard");

            byte[] selection = new byte[state.Minimum];

            for (int i = 0; i < selection.Length; i++)
                selection[i] = (byte)i;
            return selection;

            /*List<byte> selection = new();
            for (int i = 0; i < state.Minimum; i++)
                selection.Add((byte)i);
            return selection;*/
        }

        /// <summary>
        /// Action to be taken when selecting counter(s), should return the amount of counters picked from each of the indexes that were present on the selectable options
        /// </summary>
        public virtual async Task<IList<short>> OnSelectCounter(SelectCounterData state)
        {
            ConsoleUtils.PrintColored("OnSelectCounter");

            List<short> resp = new();
            short need = state.RequiredQuantity;
            byte i = 0;
            while (need > 0)
            {
                var amt = state.Counters[i];
                if (amt > need)
                {
                    resp.Add(need);
                    break;
                }
                resp.Add(amt);
                need -= amt;
                i++;
            }
            return resp;
        }

        /// <summary>
        /// Action to be taken when selecting field zone for disabling, should return the integer representing the zone selected (use the 'Zones' class helper for values)
        /// </summary>
        public virtual async Task<int> OnSelectDisfield(SelectDisfieldData state)
        {
            ConsoleUtils.PrintColored("OnSelectDisfield");

            int[] zones = new int[] { Zones.z0, Zones.z1, Zones.z2, Zones.z3, Zones.z4, Zones.z5, Zones.z6 };
            byte seq;
            for (seq = 0; seq < 7; ++seq)
                if (Zones.IsAvailable(zones[seq], state.Filter))
                    break;
            return BitConverter.ToInt32(new byte[] { state.Player, (byte)state.Location, seq, 0 });
        }

        /// <summary>
        /// Action to be taken when selecting field zone, should return the integer representing the zone selected (use the 'Zones' class helper for values)
        /// </summary>
        public virtual async Task<int> OnSelectPlace(SelectPlaceData state)
        {
            ConsoleUtils.PrintColored("OnSelectPlace");

            int[] zones = new int[] { Zones.z0, Zones.z1, Zones.z2, Zones.z3, Zones.z4, Zones.z5, Zones.z6 };
            byte seq;
            for (seq = 0; seq < 7; ++seq)
                if (Zones.IsAvailable(zones[seq], state.Filter))
                    break;
            return BitConverter.ToInt32(new byte[] { state.Player, (byte)state.Location, seq, 0 });
        }

        /// <summary>
        /// Action to be taken when selecting yes or no for an effect, should return true or false
        /// </summary>
        public virtual async Task<bool> OnSelectEffectYn(SelectEffectYnData state)
        {
            ConsoleUtils.PrintColored("OnSelectEffectYn");

            return false;
        }

        /// <summary>
        /// Action to be taken when selecting yes or no, should return true or false
        /// </summary>
        public virtual async Task<bool> OnSelectYesNo(SelectYesNoData state)
        {
            ConsoleUtils.PrintColored("OnSelectYesNo");

            return false;
        }

        /// <summary>
        /// Action to be taken when selecting an option, should return the index of the selected option
        /// </summary>
        public virtual async Task<int> OnSelectOption(SelectOptionData state)
        {
            ConsoleUtils.PrintColored("OnSelectOption");

            return 0;
        }

        /// <summary>
        /// Action to be taken when selecting a rock paper scissors option, should return 1 = scissors, 2 = rock, 3 = paper 
        /// </summary>
        public virtual async Task<byte> OnSelectRockPaperScissors()
        {
            ConsoleUtils.PrintColored("OnSelectRockPaperScissors");

            return (byte)new Random().Next(1, 4);
        }

        /// <summary>
        /// Action to be taken when selecting position, should return the position selected
        /// </summary>
        public virtual async Task<CardPosition> OnSelectPosition(SelectPositionData state)
        {
            ConsoleUtils.PrintColored("OnSelectPosition");

            return (CardPosition)state.Positions[0];
        }

        /// <summary>
        /// Action to be taken when sorting cards, should return the indexes of the selectable cards in the desired order
        /// </summary>
        public virtual async Task<IList<byte>> OnSortCard(SortCardData state)
        {
            ConsoleUtils.PrintColored("OnSortCard");

            List<byte> resp = new();
            for (byte i = 0; i < state.SelectableCards.Length; ++i)
                resp.Add(i);
            return resp;
        }
    }
}