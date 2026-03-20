using BotTest.Game.Field;
using BotTest.Utils;
using OmegaBotWrapper;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;

#pragma warning disable 1998
#pragma warning disable 8602

namespace BotTest.Game.Base
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

        /// <summary>
        /// GameField instance to access field information
        /// </summary>
        private GameField _gameField;

        public BaseBotActions()
        {
            // Temporary initialization, will be replaced in RegisterActions
            _gameField = null!;
        }

        // Store the selected defender identity for ATTACKTARGET selection.
        private DynamicCard? _selectedDefenderCard;

        /// <summary>
        /// Registers all the actions the bot should take during specific states
        /// </summary>
        /// <param name="bot">Bot instance which actions are being registered to</param>
        public virtual void RegisterActions(DuelBotWrapper bot)
        {
            _gameField = new GameField(bot);

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
            ConsoleUtils.PrintColored("OnBattleCmd - Generic attack evaluation");

            var attackableCards = state.AttackableCards
                .Where(card => !card.Attacked && !card.IsDisabled())
                .OrderByDescending(card => card.Attack)
                .ToList();

            if (!attackableCards.Any())
            {
                ConsoleUtils.PrintColored("No attackable cards available.");
                _selectedDefenderCard = null;
                return state.CanMainPhaseTwo ? BattleAction.ToMainPhaseTwo : BattleAction.ToEndPhase;
            }

            var opponentMonsters = (await _gameField.GetPlayerCardsInZone(CardLocation.MonsterZone))
                .Select((card, index) => new { Card = card, OriginalIndex = index })
                .Where(x => !x.Card.IsDisabled())
                .ToList();

            if (!opponentMonsters.Any())
            {
                var directAttacker = attackableCards
                    .Where(card => card.CanDirectAttack)
                    .OrderByDescending(card => card.Attack)
                    .FirstOrDefault();

                if (directAttacker.ID > 0 && state.AttackableCards.TryGetByID(directAttacker.ID, out DynamicCard _, out int index))
                {
                    ConsoleUtils.PrintColored($"Direct Attack with Card ID: {directAttacker.ID}, Attack: {directAttacker.Attack}, Index: {index}");
                    _selectedDefenderCard = null;
                    return state.AttackableCards[index].DeclareAttack();
                }

                ConsoleUtils.PrintColored("No direct attack possible.");
                _selectedDefenderCard = null;
                return state.CanMainPhaseTwo ? BattleAction.ToMainPhaseTwo : BattleAction.ToEndPhase;
            }

            var bestBattle = attackableCards
                .SelectMany(attacker => opponentMonsters.Select(target => new
                {
                    Attacker = attacker,
                    Target = target.Card,
                    TargetIndex = target.OriginalIndex,
                    AttackerPower = attacker.GetRealPower(),
                    TargetPower = target.Card.GetRealPower(),
                    Score = GetAttackScore(attacker, target.Card)
                }))
                .Where(option => option.Score > 0)
                .OrderByDescending(option => option.Score)
                .ThenByDescending(option => option.AttackerPower)
                .FirstOrDefault();

            if (bestBattle != null &&
                state.AttackableCards.TryGetByID(bestBattle.Attacker.ID, out DynamicCard _, out int attackerIndex))
            {
                ConsoleUtils.PrintColored($"Attacking target ID {bestBattle.Target.ID} with attacker ID {bestBattle.Attacker.ID}");
                _selectedDefenderCard = bestBattle.Target;
                return state.AttackableCards[attackerIndex].DeclareAttack();
            }

            var facedownDefenseTarget = opponentMonsters
                .Where(target => !target.Card.IsFaceup() && target.Card.IsDefense())
                .OrderBy(target => target.Card.CurrentCoordinates.Sequence)
                .FirstOrDefault();

            if (facedownDefenseTarget != null)
            {
                var opportunisticAttacker = attackableCards
                    .OrderByDescending(card => card.Attack)
                    .FirstOrDefault();

                if (opportunisticAttacker.ID > 0 &&
                    state.AttackableCards.TryGetByID(opportunisticAttacker.ID, out DynamicCard _, out int opportunisticAttackerIndex))
                {
                    ConsoleUtils.PrintColored($"Taking the opportunity to attack facedown defense target ID {facedownDefenseTarget.Card.ID} with attacker ID {opportunisticAttacker.ID}");
                    _selectedDefenderCard = facedownDefenseTarget.Card;
                    return state.AttackableCards[opportunisticAttackerIndex].DeclareAttack();
                }
            }

            ConsoleUtils.PrintColored("No advantageous attack targets found.");
            _selectedDefenderCard = null;
            return state.CanMainPhaseTwo ? BattleAction.ToMainPhaseTwo : BattleAction.ToEndPhase;
        }

        /// <summary>
        /// Action to be taken at main phase, should return the action and its index (or 0 if not applicable)
        /// </summary>
        public virtual async Task<IdleCmdResponse> OnIdleCmd(IdleCmdData state)
        {
            ConsoleUtils.PrintColored("[BASE BOT ACTIONS] OnIdleCmd");

            var opponentMonsters = (await _gameField.GetPlayerCardsInZone(CardLocation.MonsterZone))
                .Where(card => !card.IsDisabled())
                .ToList();

            var repositionToDefense = state.ReposableCards
                .Where(card => card.IsFaceup() && card.IsAttack() && ShouldRepositionToDefense(card, opponentMonsters))
                .OrderBy(card => card.Attack)
                .FirstOrDefault();

            if (repositionToDefense.ID > 0)
            {
                ConsoleUtils.PrintColored($"Repositioning to defense: {repositionToDefense.ID}");
                return repositionToDefense.Reposition();
            }

            var repositionToAttack = state.ReposableCards
                .Where(card => card.IsDefense() && ShouldRepositionToAttack(card, opponentMonsters))
                .OrderByDescending(card => card.Attack)
                .FirstOrDefault();

            if (repositionToAttack.ID > 0)
            {
                ConsoleUtils.PrintColored($"Repositioning to attack: {repositionToAttack.ID}");
                return repositionToAttack.Reposition();
            }

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
            ConsoleUtils.PrintColored($"[BASE BOT ACTIONS] [OnSelectCard] => {(DHint)state.SelectionHint}");

            if (state.SelectionHint.Equals((int)DHint.TARGET))
            {
                var botCards = state.SelectableCards
                                    .Select((card, index) => new { Card = card, Index = index })
                                    .OrderByDescending(x => x.Card.Attack) // Order by best Attack
                                    .Take(state.Minimum)
                                    .Select(x => (byte)x.Index)
                                    .ToArray();
                if (botCards.Any())
                {
                    ConsoleUtils.PrintColored($"Selected {botCards.Length} cards for TARGET.");
                    return botCards;
                }
            }

            if (state.SelectionHint.Equals((int)DHint.ATTACKTARGET))
            {
                ConsoleUtils.PrintColored("OnSelectCard - Selecting Defender");

                if (_selectedDefenderCard.HasValue)
                {
                    var selectedDefender = _selectedDefenderCard.Value;
                    if (state.SelectableCards.TryGetByIDWithZone(selectedDefender.ID, selectedDefender.CurrentCoordinates.Sequence, out _, out int selectedIndex))
                    {
                        ConsoleUtils.PrintColored($"Selecting defender by card code and sequence: {selectedDefender.ID} / {selectedDefender.CurrentCoordinates.Sequence}");
                        return new List<byte> { (byte)selectedIndex };
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
                    ConsoleUtils.PrintColored($"Selected {botCards.Length} cards for DESTROY.");
                    return botCards;
                }
            }

            if (state.SelectionHint.Equals((int)DHint.REMOVE))
            {
                ConsoleUtils.PrintColored("OnSelectCard - Selecting Card to REMOVE");

                // Filter cards where Owner = 1 (Player) because using Player Vision
                var botCards = state.SelectableCards.Select((card, index) => new { Card = card, Index = index })
                                                    .Where(x => x.Card.Owner == 1)
                                                    .Take(state.Minimum)
                                                    .Select(x => (byte)x.Index).ToArray();
                // Respect Minimum (or Maximum if needed)
                if (botCards.Any())
                {
                    ConsoleUtils.PrintColored($"Selected {botCards.Length} cards for REMOVE.");
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

        private static bool ShouldRepositionToAttack(DynamicCard card, IEnumerable<DynamicCard> opponentMonsters)
        {
            var opponents = opponentMonsters.ToList();
            if (!opponents.Any())
                return true;

            return opponents.Any(opponent => GetAttackScore(card, opponent) > 0);
        }

        private static bool ShouldRepositionToDefense(DynamicCard card, IEnumerable<DynamicCard> opponentMonsters)
        {
            var strongestOpponentAttack = opponentMonsters
                .Where(opponent => opponent.IsFaceup() && opponent.IsAttack())
                .Select(opponent => opponent.Attack)
                .DefaultIfEmpty(0)
                .Max();

            return strongestOpponentAttack > card.Attack;
        }

        private static int GetAttackScore(DynamicCard attacker, DynamicCard defender)
        {
            int attackerPower = attacker.Attack;
            int defenderPower = defender.GetRealPower();

            if (attackerPower <= defenderPower)
                return 0;

            int score = 10_000 + defenderPower;

            if (defender.IsFaceup() && defender.IsAttack())
                score += 2_000;
            else if (defender.IsDefense())
                score += 1_000;

            score += attackerPower - defenderPower;
            return score;
        }

        /// <summary>
        /// Action to be taken when selecting tribute(s), should return the indexes that were selected from the selectable options
        /// </summary>
        public virtual async Task<IList<byte>?> OnSelectTribute(SelectTributeData state)
        {
            ConsoleUtils.PrintColored("[BASE BOT ACTIONS] OnSelectTribute");

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
            ConsoleUtils.PrintColored("[BASE BOT ACTIONS] OnSelectSum");

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
            ConsoleUtils.PrintColored("[BASE BOT ACTIONS] OnAnnounceAttribute");

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
            ConsoleUtils.PrintColored("[BASE BOT ACTIONS] OnAnnounceRace");

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
            ConsoleUtils.PrintColored("[BASE BOT ACTIONS] OnAnnounceNumber");

            return state.SelectableOptions[0];
        }

        /// <summary>
        /// Action to be taken when announcing a card, should return the card id that was selected
        /// </summary>
        public virtual async Task<int> OnAnnounceCard(AnnounceCardData state)
        {
            ConsoleUtils.PrintColored("[BASE BOT ACTIONS] OnAnnounceCard");
            var filters = state.AppliableFilters;
            var cards = await DuelBotWrapper.GetMatchingCards(filters);
            Random rng = new();
            var announcedCard = cards.Cards.ElementAt(rng.Next(0, cards.Cards.Length));
            return announcedCard.Alias != 0 ? announcedCard.Alias : announcedCard.ID;
        }

        /// <summary>
        /// Action to be taken when announcing a card with filter, should return the card id that was selected
        /// </summary>
        public virtual async Task<int> OnAnnounceCardFilter(AnnounceCardFilterData state)
        {
            ConsoleUtils.PrintColored("[BASE BOT ACTIONS] OnAnnounceCardFilter");

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
            ConsoleUtils.PrintColored("[BASE BOT ACTIONS] OnSelectUnselectCard");

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
            ConsoleUtils.PrintColored("[BASE BOT ACTIONS] OnSelectCounter");

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
            ConsoleUtils.PrintColored("[BASE BOT ACTIONS] OnSelectDisfield");

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
            ConsoleUtils.PrintColored("[BASE BOT ACTIONS] OnSelectPlace");

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
            ConsoleUtils.PrintColored("[BASE BOT ACTIONS] OnSelectEffectYn");

            return false;
        }

        /// <summary>
        /// Action to be taken when selecting yes or no, should return true or false
        /// </summary>
        public virtual async Task<bool> OnSelectYesNo(SelectYesNoData state)
        {
            ConsoleUtils.PrintColored("[BASE BOT ACTIONS] OnSelectYesNo");

            return false;
        }

        /// <summary>
        /// Action to be taken when selecting an option, should return the index of the selected option
        /// </summary>
        public virtual async Task<int> OnSelectOption(SelectOptionData state)
        {
            ConsoleUtils.PrintColored("[BASE BOT ACTIONS] OnSelectOption");

            return 0;
        }

        /// <summary>
        /// Action to be taken when selecting a rock paper scissors option, should return 1 = scissors, 2 = rock, 3 = paper 
        /// </summary>
        public virtual async Task<byte> OnSelectRockPaperScissors()
        {
            ConsoleUtils.PrintColored("[BASE BOT ACTIONS] OnSelectRockPaperScissors");

            return (byte)new Random().Next(1, 4);
        }

        /// <summary>
        /// Action to be taken when selecting position, should return the position selected
        /// </summary>
        public virtual async Task<CardPosition> OnSelectPosition(SelectPositionData state)
        {
            ConsoleUtils.PrintColored("[BASE BOT ACTIONS] OnSelectPosition");

            return (CardPosition)state.Positions[0];
        }

        /// <summary>
        /// Action to be taken when sorting cards, should return the indexes of the selectable cards in the desired order
        /// </summary>
        public virtual async Task<IList<byte>> OnSortCard(SortCardData state)
        {
            ConsoleUtils.PrintColored("[BASE BOT ACTIONS] OnSortCard");

            List<byte> resp = new();
            for (byte i = 0; i < state.SelectableCards.Length; ++i)
                resp.Add(i);
            return resp;
        }

        /// <summary>
        /// Returns the current phase of the duel based on the value of BotWrapper.CurrentStateHeader.CurrentPhase
        /// </summary>
        /// <returns>A value from the DuelPhase enum representing the current phase.</returns>
        public DuelPhase GetCurrentDuelPhase()
        {
            ConsoleUtils.PrintColored($"Current Duel Phase: {(DuelPhase)BotWrapper.CurrentStateHeader.CurrentPhase}");
            return (DuelPhase)BotWrapper.CurrentStateHeader.CurrentPhase;
        }

        /// <summary>
        /// Checks the current state of the bot from the CurrentStateCode.
        /// </summary>
        /// <returns>Value of the BotState enum representing the current state of the bot.</returns>
        public BotState GetCurrentBotState()
        {
            ConsoleUtils.PrintColored($"Current State: {(DuelPhase)BotWrapper.CurrentStateHeader.CurrentPhase}");
            return (BotState)BotWrapper.CurrentStateHeader.CurrentStateCode;
        }

        /// <summary>
        /// Gets the name of the bot's current state.
        /// </summary>
        /// <returns>String with the name of the bot's current state.</returns>
        public string GetCurrentStateName()
        {
            ConsoleUtils.PrintColored($"Current State Name: {(DuelPhase)BotWrapper.CurrentStateHeader.CurrentPhase}");
            return BotWrapper.CurrentStateHeader.CurrentStateName;
        }

        /// <summary>
        /// Returns the current player of the duel based on the value of BotWrapper.CurrentStateHeader.CurrentPlayer
        /// </summary>
        /// <returns>A value from the DuelPhase enum representing the current phase.</returns>
        public int GetCurrentPlayer()
        {
            ConsoleUtils.PrintColored($"Current Player: {BotWrapper.CurrentStateHeader.CurrentPlayer}");
            return BotWrapper.CurrentStateHeader.CurrentPlayer;
        }
    }
}
