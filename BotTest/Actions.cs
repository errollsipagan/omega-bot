using BotTest.Game.Field;
using OmegaBotWrapper;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;

namespace BotTest
{
    /// <summary>
    /// Sample bot that implements the bot actions interface with GameField integration
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
        /// GameField instance to access field information (agora um campo privado)
        /// </summary>
        private GameField _gameField;

        public Actions()
        {
            // Inicialização temporária, será substituída em RegisterActions
            _gameField = null!;
        }

        /// <summary>
        /// Registers all the actions the bot should take during specific states
        /// </summary>
        /// <param name="bot">Bot instance which actions are being registered to</param>
        public void RegisterActions(DuelBotWrapper bot)
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
        /// Action to be taken at battle phase, should return the action and its index
        /// </summary>
        async Task<BattleCmdResponse> OnBattleCmd(BattleCmdData state)
        {
            if (state.AttackableCards.Any())
            {
                var enemyMonsters = await _gameField.GetBotCardsInZone(CardLocation.MonsterZone, isOwnSide: false);
                if (enemyMonsters.Any())
                {
                    var weakestEnemy = enemyMonsters.OrderBy(card => card.GetRealPower()).First();
                    var bestAttacker = state.AttackableCards
                        .Where(card => card.GetRealPower() > weakestEnemy.GetRealPower())
                        .OrderByDescending(card => card.GetRealPower())
                        .FirstOrDefault();

                    if (bestAttacker.ID != 0)
                    {
                        return bestAttacker.DeclareAttack();
                    }
                }
                else
                {
                    var strongestAttacker = state.AttackableCards
                        .OrderByDescending(card => card.GetRealPower())
                        .FirstOrDefault();
                    if (strongestAttacker.ID != 0 && strongestAttacker.CanDirectAttack)
                    {
                        return strongestAttacker.DeclareAttack();
                    }
                }
            }

            if (state.CanMainPhaseTwo)
                return BattleAction.ToMainPhaseTwo;
            return BattleAction.ToEndPhase;
        }

        /// <summary>
        /// Action to be taken at main phase, should return the action and its index
        /// </summary>
        async Task<IdleCmdResponse> OnIdleCmd(IdleCmdData state)
        {
            var enemyMonsterCount = await _gameField.GetBotZoneCardCount(CardLocation.MonsterZone, isOwnSide: false);
            var enemySpellTrapCount = await _gameField.GetBotZoneCardCount(CardLocation.SpellZone, isOwnSide: false);

            if (state.SummonableCards.Any())
            {
                //if (enemyMonsterCount == 0 || enemySpellTrapCount == 0)
                //{
                var strongestSummon = state.SummonableCards
                    .OrderByDescending(card => card.Attack)
                    .First();
                return strongestSummon.NormalSummon();
                //}
            }

            if (state.ActivableCards.Any())
            {
                var activableCard = state.ActivableCards.First();
                return activableCard.MainActivateFromIndex(0);
            }

            if (state.CanBattlePhase)
                return MainAction.ToBattlePhase;
            return MainAction.ToEndPhase;
        }

        // Métodos restantes mantidos como na implementação original
        async Task<int> OnSelectChain(SelectChainData state)
        {
            if (!state.Forced)
                return -1;
            return 0;
        }

        async Task<IList<byte>?> OnSelectCard(SelectCardData state)
        {
            byte[] selection = new byte[state.Minimum];
            for (int i = 0; i < selection.Length; i++)
                selection[i] = (byte)i;
            return selection;
        }

        async Task<IList<byte>?> OnSelectTribute(SelectTributeData state)
        {
            List<byte> resp = new();
            for (byte i = 0; i < state.Minimum; i++)
                resp.Add(i);
            return resp;
        }

        async Task<IList<byte>?> OnSelectSum(SelectSumData state)
        {
            int s1, s2;
            int min = state.Minimum;
            int max = state.Maximum;
            var cards = state.SelectableCards;
            int sum = state.Sum;
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
                    if (s1 == sum || s2 == sum || (!state.MustBeExactValue && (s1 >= sum || s2 >= sum)))
                    {
                        return combo.Select(c => (byte)Array.IndexOf(state.SelectableCards, c)).ToList();
                    }
                }
                i++;
            }
            return null;
        }

        async Task<IList<CardAttribute>> OnAnnounceAttribute(AnnounceAttributeData state)
        {
            List<CardAttribute> cas = new();
            foreach (CardAttribute ca in Enum.GetValues(typeof(CardAttribute)))
                if (state.SelectableOptions.Contains((byte)ca))
                    cas.Add(ca);
            Random rng = new();
            return cas.OrderBy(x => rng.Next()).Take(state.RequiredCount).ToList();
        }

        async Task<IList<CardRace>> OnAnnounceRace(AnnounceRaceData state)
        {
            List<CardRace> cas = new();
            foreach (CardRace ca in Enum.GetValues(typeof(CardRace)))
                if (state.SelectableOptions.Contains((byte)ca))
                    cas.Add(ca);
            Random rng = new();
            return cas.OrderBy(x => rng.Next()).Take(state.RequiredCount).ToList();
        }

        async Task<int> OnAnnounceNumber(AnnounceNumberData state)
        {
            return state.SelectableOptions[0];
        }

        async Task<int> OnAnnounceCard(AnnounceCardData state)
        {
            var filters = state.AppliableFilters;
            var cards = await DuelBotWrapper.GetMatchingCards(filters);
            Random rng = new();
            return cards.Cards.ElementAt(rng.Next(0, cards.Cards.Length)).ID;
        }

        async Task<int> OnAnnounceCardFilter(AnnounceCardFilterData state)
        {
            var filters = state.AppliableFilters;
            var cards = await DuelBotWrapper.GetMatchingCards(filters);
            Random rng = new();
            return cards.Cards.ElementAt(rng.Next(0, cards.Cards.Length)).ID;
        }

        async Task<IList<byte>?> OnSelectUnselectCard(SelectUnselectCardData state)
        {
            List<byte> selection = new();
            for (int i = 0; i < state.Minimum; i++)
                selection.Add((byte)i);
            return selection;
        }

        async Task<IList<short>> OnSelectCounter(SelectCounterData state)
        {
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

        async Task<int> OnSelectDisfield(SelectDisfieldData state)
        {
            int[] zones = new int[] { Zones.z0, Zones.z1, Zones.z2, Zones.z3, Zones.z4, Zones.z5, Zones.z6 };
            byte seq;
            for (seq = 0; seq < 7; ++seq)
                if (Zones.IsAvailable(zones[seq], state.Filter))
                    break;
            return BitConverter.ToInt32(new byte[] { state.Player, (byte)state.Location, seq, 0 });
        }

        async Task<int> OnSelectPlace(SelectPlaceData state)
        {
            int[] zones = new int[] { Zones.z0, Zones.z1, Zones.z2, Zones.z3, Zones.z4, Zones.z5, Zones.z6 };
            byte seq;
            for (seq = 0; seq < 7; ++seq)
                if (Zones.IsAvailable(zones[seq], state.Filter))
                    break;
            return BitConverter.ToInt32(new byte[] { state.Player, (byte)state.Location, seq, 0 });
        }

        async Task<bool> OnSelectEffectYn(SelectEffectYnData state)
        {
            return false;
        }

        async Task<bool> OnSelectYesNo(SelectYesNoData state)
        {
            return false;
        }

        async Task<int> OnSelectOption(SelectOptionData state)
        {
            return 0;
        }

        async Task<byte> OnSelectRockPaperScissors()
        {
            return (byte)new Random().Next(1, 4);
        }

        async Task<CardPosition> OnSelectPosition(SelectPositionData state)
        {
            return (CardPosition)state.Positions[0];
        }

        async Task<IList<byte>> OnSortCard(SortCardData state)
        {
            List<byte> resp = new();
            for (byte i = 0; i < state.SelectableCards.Length; ++i)
                resp.Add(i);
            return resp;
        }
    }
}