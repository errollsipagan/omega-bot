using OmegaBotWrapper.Enum;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;

namespace OmegaBotWrapper
{
    /// <summary>
    /// Shared default bot action behavior used by wrappers, sample bots, and compatibility layers.
    /// </summary>
    public static class BotActionUtilities
    {
        private static readonly int[] ZoneMasks = { Zones.z0, Zones.z1, Zones.z2, Zones.z3, Zones.z4, Zones.z5, Zones.z6 };

        public static BattleCmdResponse DefaultBattle(BattleCmdData state)
        {
            var candidate = state.AttackableCards
                .Where(card => !card.Attacked)
                .OrderByDescending(card => card.Attack)
                .FirstOrDefault();

            if (candidate.ID > 0)
                return candidate.DeclareAttack();

            return state.CanMainPhaseTwo
                ? new BattleCmdResponse(BattleAction.ToMainPhaseTwo)
                : new BattleCmdResponse(BattleAction.ToEndPhase);
        }

        public static IdleCmdResponse DefaultIdle(IdleCmdData state)
        {
            if (state.SummonableCards.Any())
                return state.SummonableCards.First().NormalSummon();

            return state.CanBattlePhase
                ? new IdleCmdResponse(MainAction.ToBattlePhase)
                : new IdleCmdResponse(MainAction.ToEndPhase);
        }

        public static int DefaultChain(SelectChainData state) => state.Forced ? 0 : -1;

        public static IList<byte> BuildSequentialSelection(int minimum)
        {
            byte[] selection = new byte[minimum];
            for (int i = 0; i < selection.Length; i++)
                selection[i] = (byte)i;
            return selection;
        }

        public static IList<byte> BuildSequentialListSelection(int minimum)
        {
            List<byte> selection = new();
            for (byte i = 0; i < minimum; i++)
                selection.Add(i);
            return selection;
        }

        public static IList<byte>? SelectSum(SelectSumData state)
        {
            int min = state.Minimum;
            int max = state.Maximum;
            var cards = state.SelectableCards;
            int sum = state.Sum;
            int comboSize = (min <= 1) ? 2 : min;

            while (comboSize <= max && comboSize <= cards.Length)
            {
                foreach (var combo in cards.GetCombinations(comboSize))
                {
                    int s1 = 0;
                    int s2 = 0;
                    foreach (var card in combo)
                    {
                        s1 += card.OpParam1;
                        s2 += card.OpParam2 != 0 ? card.OpParam2 : card.OpParam1;
                    }

                    if (s1 == sum || s2 == sum || (!state.MustBeExactValue && (s1 >= sum || s2 >= sum)))
                        return combo.Select(card => (byte)Array.IndexOf(cards, card)).ToList();
                }

                comboSize++;
            }

            return null;
        }

        public static IList<short> SelectCounter(SelectCounterData state)
        {
            List<short> response = new();
            short need = state.RequiredQuantity;

            for (int i = 0; i < state.Counters.Length && need > 0; i++)
            {
                short amount = state.Counters[i];
                if (amount > need)
                {
                    response.Add(need);
                    need = 0;
                    break;
                }

                response.Add(amount);
                need -= amount;
            }

            return response;
        }

        public static byte FindFirstAvailableZone(int filter)
        {
            for (byte seq = 0; seq < ZoneMasks.Length; seq++)
                if (Zones.IsAvailable(ZoneMasks[seq], filter))
                    return seq;

            return 0;
        }

        public static int BuildZoneResponse(byte player, byte location, byte seq) =>
            BitConverter.ToInt32(new byte[] { player, location, seq, 0 });

        public static int SelectZone(byte player, byte location, int filter) =>
            BuildZoneResponse(player, location, FindFirstAvailableZone(filter));

        public static IList<CardAttribute> SelectAttributes(AnnounceAttributeData state)
        {
            List<CardAttribute> attributes = new();
            foreach (CardAttribute attribute in System.Enum.GetValues(typeof(CardAttribute)))
                if (state.SelectableOptions.Contains((byte)attribute))
                    attributes.Add(attribute);

            Random rng = new();
            return attributes.OrderBy(_ => rng.Next()).Take(state.RequiredCount).ToList();
        }

        public static IList<CardRace> SelectRaces(AnnounceRaceData state)
        {
            List<CardRace> races = new();
            foreach (CardRace race in System.Enum.GetValues(typeof(CardRace)))
                if (state.SelectableOptions.Contains((int)race))
                    races.Add(race);

            Random rng = new();
            return races.OrderBy(_ => rng.Next()).Take(state.RequiredCount).ToList();
        }

        public static int SelectFirstNumber(AnnounceNumberData state) =>
            state.SelectableOptions.FirstOrDefault();

        public static int SelectRandomAnnouncedCard(DBCardList cards, bool preferAlias = false)
        {
            if (cards.Cards == null || cards.Cards.Length == 0)
                return 0;

            var chosen = cards.Cards[new Random().Next(0, cards.Cards.Length)];
            return preferAlias && chosen.Alias != 0 ? chosen.Alias : chosen.ID;
        }

        public static byte SelectRandomRps() => (byte)new Random().Next(1, 4);

        public static CardPosition SelectPosition(SelectPositionData state, CardPosition preferredFallback = CardPosition.FaceUpAttack)
        {
            byte[] positions = state.Positions ?? Array.Empty<byte>();
            if (positions.Length == 0)
                return preferredFallback;

            return (CardPosition)positions.FirstOrDefault((byte)preferredFallback);
        }

        public static IList<byte> IdentitySort(SortCardData state)
        {
            List<byte> response = new();
            for (byte i = 0; i < state.SelectableCards.Length; i++)
                response.Add(i);
            return response;
        }
    }
}
