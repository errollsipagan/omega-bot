using OmegaBotWrapper.Enum;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;

namespace OmegaBotWrapper
{
    /// <summary>
    /// Validates bot responses before they are serialized into packets sent to Omega.
    /// Invalid responses are corrected to the safest legal value and logged.
    /// </summary>
    internal static class BotResponseValidator
    {
        // ---- SelectBattleCmd -----------------------------------------------

        /// <summary>
        /// Validates a BattleCmd response.
        /// Fallback: ToEndPhase (never risks sending an invalid attacker index).
        /// </summary>
        public static BattleCmdResponse ValidateBattleCmd(BattleCmdResponse response, BattleCmdData state, int botId)
        {
            int actionVal = (int)response.Action;
            if (actionVal < 0 || actionVal > 3)
            {
                BotLogger.Warn(botId, $"[SelectBattleCmd] Invalid Action={actionVal}, expected 0-3. Falling back to ToEndPhase.");
                return BattleAction.ToEndPhase;
            }

            if (response.Action == BattleAction.Attack &&
                (state.AttackableCards == null || state.AttackableCards.Length == 0))
            {
                BotLogger.Warn(botId, $"[SelectBattleCmd] Attack chosen but AttackableCards is empty. Falling back to ToEndPhase.");
                return BattleAction.ToEndPhase;
            }

            if (response.Action == BattleAction.ToMainPhaseTwo && !state.CanMainPhaseTwo)
            {
                BotLogger.Warn(botId, $"[SelectBattleCmd] ToMainPhaseTwo chosen but CanMainPhaseTwo=false. Falling back to ToEndPhase.");
                return BattleAction.ToEndPhase;
            }

            return response;
        }

        // ---- SelectIdleCmd ------------------------------------------------

        /// <summary>
        /// Validates an IdleCmd response.
        /// Fallback: ToEndPhase.
        /// </summary>
        public static IdleCmdResponse ValidateIdleCmd(IdleCmdResponse response, IdleCmdData state, int botId)
        {
            int actionVal = (int)response.Action;
            if (actionVal < 0 || actionVal > 8)
            {
                BotLogger.Warn(botId, $"[SelectIdleCmd] Invalid Action={actionVal}, expected 0-8. Falling back to ToEndPhase.");
                return MainAction.ToEndPhase;
            }

            if (response.Action == MainAction.ToBattlePhase && !state.CanBattlePhase)
            {
                BotLogger.Warn(botId, $"[SelectIdleCmd] ToBattlePhase chosen but CanBattlePhase=false. Falling back to ToEndPhase.");
                return MainAction.ToEndPhase;
            }

            return response;
        }

        // ---- SelectCard / SelectUnselectCard / SelectTribute / SelectSum ---

        /// <summary>
        /// Validates a card-selection response (used by SelectCard, SelectUnselectCard, SelectTribute, SelectSum).
        /// Corrects count to satisfy [minimum, maximum] and removes out-of-bounds indexes.
        /// Fallback: indexes 0..minimum-1.
        /// </summary>
        public static IList<byte> ValidateCardSelection(
            IList<byte>? response,
            int minimum, int maximum, int selectableCount,
            string stateName, int botId)
        {
            response ??= Array.Empty<byte>();

            // Remove out-of-bounds indexes
            var valid = response.Where(idx => idx < selectableCount).ToList();
            if (valid.Count != response.Count)
                BotLogger.Warn(botId, $"[{stateName}] Removed {response.Count - valid.Count} out-of-bounds index(es) (selectableCount={selectableCount}). Original: [{string.Join(",", response)}]");

            // Clamp to maximum
            if (valid.Count > maximum)
            {
                BotLogger.Warn(botId, $"[{stateName}] Selection count {valid.Count} > maximum {maximum}. Truncating.");
                valid = valid.Take(maximum).ToList();
            }

            // Pad to minimum using first unused indexes
            if (valid.Count < minimum)
            {
                BotLogger.Warn(botId, $"[{stateName}] Selection count {valid.Count} < minimum {minimum}. Padding with sequential indexes.");
                for (byte i = 0; i < selectableCount && valid.Count < minimum; i++)
                    if (!valid.Contains(i))
                        valid.Add(i);
            }

            return valid;
        }

        // ---- SelectChain --------------------------------------------------

        /// <summary>
        /// Validates a chain selection.
        /// -1 is only valid when state.Forced == false.
        /// Positive index must be within SelectableCards range.
        /// Fallback: 0 (first chain) when forced, -1 when not.
        /// </summary>
        public static int ValidateSelectChain(int response, SelectChainData state, int botId)
        {
            if (response == -1)
            {
                if (state.Forced)
                {
                    BotLogger.Warn(botId, $"[SelectChain] Returned -1 (no chain) but selection is Forced. Falling back to 0.");
                    return 0;
                }
                return -1;
            }

            int cardCount = state.SelectableCards?.Length ?? 0;
            if (response < 0 || response >= cardCount)
            {
                BotLogger.Warn(botId, $"[SelectChain] Index={response} out of range [0,{cardCount - 1}]. Falling back to {(state.Forced ? 0 : -1)}.");
                return state.Forced ? 0 : -1;
            }

            return response;
        }

        // ---- SelectCounter ------------------------------------------------

        /// <summary>
        /// Validates counter amounts.
        /// Each amount must be 0..Counters[i]; total must equal RequiredQuantity.
        /// Fallback: redistribute from front to satisfy total.
        /// </summary>
        public static IList<short> ValidateSelectCounter(IList<short> response, SelectCounterData state, int botId)
        {
            var counters = state.Counters ?? Array.Empty<short>();
            var result = response?.ToList() ?? new List<short>();

            // Pad or trim to match Counters length
            while (result.Count < counters.Length) result.Add(0);
            if (result.Count > counters.Length)
            {
                BotLogger.Warn(botId, $"[SelectCounter] Response count {result.Count} != Counters length {counters.Length}. Trimming.");
                result = result.Take(counters.Length).ToList();
            }

            // Clamp each amount to what that card actually has
            bool clamped = false;
            for (int i = 0; i < result.Count; i++)
            {
                if (result[i] < 0) { result[i] = 0; clamped = true; }
                else if (result[i] > counters[i]) { result[i] = counters[i]; clamped = true; }
            }
            if (clamped)
                BotLogger.Warn(botId, $"[SelectCounter] Clamped one or more amounts to available counter limits.");

            // Check total
            short total = (short)result.Sum(x => x);
            if (total == state.RequiredQuantity)
                return result;

            // Redistribute to hit RequiredQuantity
            BotLogger.Warn(botId, $"[SelectCounter] Total={total} != RequiredQuantity={state.RequiredQuantity}. Redistributing.");
            var fixed_ = Enumerable.Repeat((short)0, counters.Length).ToList();
            short need = state.RequiredQuantity;
            for (int i = 0; i < counters.Length && need > 0; i++)
            {
                short take = (short)Math.Min(counters[i], need);
                fixed_[i] = take;
                need -= take;
            }
            if (need > 0)
                BotLogger.Warn(botId, $"[SelectCounter] Not enough total counters available ({counters.Sum(x => x)}) to satisfy RequiredQuantity={state.RequiredQuantity}.");
            return fixed_;
        }

        // ---- SelectDisfield / SelectPlace ---------------------------------

        /// <summary>
        /// Validates a zone selection response for SelectDisfield or SelectPlace.
        /// The response is a 4-byte LE int: [controller, location, seq, 0].
        /// Validates that seq (byte 2) is 0-6 and that the corresponding zone
        /// bit is set in Filter.
        /// Fallback: first zone bit set in Filter.
        /// </summary>
        public static int ValidateZoneSelection(int response, int filter, byte expectedPlayer, byte expectedLocation, string stateName, int botId)
        {
            byte[] bytes = BitConverter.GetBytes(response);
            byte player = bytes[0];
            byte location = bytes[1];
            byte seq = bytes[2];

            bool anyIssue = false;

            if (player != expectedPlayer)
            {
                BotLogger.Warn(botId, $"[{stateName}] Response Player={player} != expected {expectedPlayer}. Correcting.");
                bytes[0] = expectedPlayer;
                anyIssue = true;
            }

            if (location != expectedLocation)
            {
                BotLogger.Warn(botId, $"[{stateName}] Response Location=0x{location:X2} != expected 0x{expectedLocation:X2}. Correcting.");
                bytes[1] = expectedLocation;
                anyIssue = true;
            }

            if (seq > 6)
            {
                BotLogger.Warn(botId, $"[{stateName}] Zone seq={seq} is out of range (0-6). Will recompute from Filter=0x{filter:X8}.");
                anyIssue = true;
                seq = 7; // force recompute below
            }
            else if (!IsZoneBitSet(seq, filter))
            {
                BotLogger.Warn(botId, $"[{stateName}] Zone seq={seq} not available in Filter=0x{filter:X8}. Will pick first available.");
                anyIssue = true;
                seq = 7; // force recompute below
            }

            if (anyIssue)
            {
                // Recompute seq from filter
                byte corrected = FindFirstZoneBit(filter);
                if (corrected > 6)
                    BotLogger.Warn(botId, $"[{stateName}] No valid zone bits in Filter=0x{filter:X8}, defaulting to seq=0.");
                bytes[2] = corrected <= 6 ? corrected : (byte)0;
                bytes[3] = 0;
                return BitConverter.ToInt32(bytes, 0);
            }

            return response;
        }

        private static bool IsZoneBitSet(byte seq, int filter) => (filter & (1 << seq)) != 0;

        private static byte FindFirstZoneBit(int filter)
        {
            for (byte seq = 0; seq < 7; seq++)
                if ((filter & (1 << seq)) != 0)
                    return seq;
            return 0;
        }

        // ---- SelectOption -------------------------------------------------

        /// <summary>
        /// Validates a SelectOption index.
        /// Fallback: 0.
        /// </summary>
        public static int ValidateSelectOption(int response, SelectOptionData state, int botId)
        {
            int count = state.Options?.Length ?? 0;
            if (count == 0)
            {
                BotLogger.Warn(botId, $"[SelectOption] Options array is empty. Returning 0.");
                return 0;
            }
            if (response < 0 || response >= count)
            {
                BotLogger.Warn(botId, $"[SelectOption] Index={response} out of range [0,{count - 1}]. Falling back to 0.");
                return 0;
            }
            return response;
        }

        // ---- RockPaperScissors --------------------------------------------

        /// <summary>
        /// Validates RPS choice: must be 1 (scissors), 2 (rock), or 3 (paper).
        /// Fallback: 1.
        /// </summary>
        public static byte ValidateRockPaperScissors(byte response, int botId)
        {
            if (response < 1 || response > 3)
            {
                BotLogger.Warn(botId, $"[RockPaperScissors] Value={response} invalid (expected 1-3). Falling back to 1.");
                return 1;
            }
            return response;
        }

        // ---- SelectPosition -----------------------------------------------

        /// <summary>
        /// Validates a position selection.
        /// Response must be one of the values in state.Positions.
        /// Fallback: first available position.
        /// </summary>
        public static CardPosition ValidateSelectPosition(CardPosition response, SelectPositionData state, int botId)
        {
            var positions = state.Positions;
            if (positions == null || positions.Length == 0)
            {
                BotLogger.Warn(botId, $"[SelectPosition] Positions array is empty. Returning FaceUpAttack.");
                return CardPosition.FaceUpAttack;
            }
            if (!positions.Contains((byte)response))
            {
                BotLogger.Warn(botId, $"[SelectPosition] Position=0x{(byte)response:X2} not in available positions [{string.Join(",", positions.Select(p => $"0x{p:X2}"))}]. Falling back to first.");
                return (CardPosition)positions[0];
            }
            return response;
        }

        // ---- SortCard -----------------------------------------------------

        /// <summary>
        /// Validates a sort-card response (must be a permutation of 0..N-1).
        /// Fallback: sequential order 0..N-1.
        /// </summary>
        public static IList<byte> ValidateSortCard(IList<byte> response, SortCardData state, int botId)
        {
            int n = state.SelectableCards?.Length ?? 0;
            var sequential = Enumerable.Range(0, n).Select(i => (byte)i).ToList();

            if (response == null || response.Count != n)
            {
                BotLogger.Warn(botId, $"[SortCard] Response count {response?.Count ?? 0} != expected {n}. Falling back to sequential order.");
                return sequential;
            }

            bool hasOutOfBounds = response.Any(idx => idx >= n);
            bool hasDuplicates = response.Distinct().Count() != n;

            if (hasOutOfBounds || hasDuplicates)
            {
                BotLogger.Warn(botId, $"[SortCard] Response is not a valid permutation of 0-{n - 1} (outOfBounds={hasOutOfBounds}, duplicates={hasDuplicates}). Falling back to sequential.");
                return sequential;
            }

            return response;
        }

        // ---- AnnounceAttribute --------------------------------------------

        /// <summary>
        /// Validates an AnnounceAttribute response.
        /// Count must equal RequiredCount; each attribute must be in SelectableOptions.
        /// Fallback: first RequiredCount options cast to CardAttribute.
        /// </summary>
        public static IList<CardAttribute> ValidateAnnounceAttribute(IList<CardAttribute> response, AnnounceAttributeData state, int botId)
        {
            var options = state.SelectableOptions ?? Array.Empty<byte>();
            int required = state.RequiredCount;

            var valid = response?.Where(a => options.Contains((byte)a)).ToList() ?? new List<CardAttribute>();

            if (valid.Count != required)
            {
                BotLogger.Warn(botId, $"[AnnounceAttribute] Valid count={valid.Count} != RequiredCount={required}. Rebuilding from SelectableOptions.");
                valid = options.Take(required).Select(b => (CardAttribute)b).ToList();
            }

            return valid;
        }

        // ---- AnnounceRace -------------------------------------------------

        /// <summary>
        /// Validates an AnnounceRace response.
        /// Count must equal RequiredCount; each race must be in SelectableOptions.
        /// Fallback: first RequiredCount options cast to CardRace.
        /// </summary>
        public static IList<CardRace> ValidateAnnounceRace(IList<CardRace> response, AnnounceRaceData state, int botId)
        {
            var options = state.SelectableOptions ?? Array.Empty<int>();
            int required = state.RequiredCount;

            var valid = response?.Where(r => options.Contains((int)r)).ToList() ?? new List<CardRace>();

            if (valid.Count != required)
            {
                BotLogger.Warn(botId, $"[AnnounceRace] Valid count={valid.Count} != RequiredCount={required}. Rebuilding from SelectableOptions.");
                valid = options.Take(required).Select(v => (CardRace)v).ToList();
            }

            return valid;
        }

        // ---- AnnounceNumber -----------------------------------------------

        /// <summary>
        /// Validates an AnnounceNumber response (must be one of the selectable option values).
        /// Fallback: first option value.
        /// </summary>
        public static int ValidateAnnounceNumber(int response, AnnounceNumberData state, int botId)
        {
            var options = state.SelectableOptions;
            if (options == null || options.Length == 0)
            {
                BotLogger.Warn(botId, $"[AnnounceNumber] SelectableOptions is empty. Returning 0.");
                return 0;
            }
            if (!options.Contains(response))
            {
                BotLogger.Warn(botId, $"[AnnounceNumber] Value={response} not in SelectableOptions [{string.Join(",", options)}]. Falling back to first.");
                return options[0];
            }
            return response;
        }

        // ---- AnnounceCard / AnnounceCardFilter ----------------------------

        /// <summary>
        /// Validates an AnnounceCard response: card ID must be positive.
        /// Cannot validate against the full DB here; just ensures non-zero.
        /// Fallback: 0 (Omega will likely reject it, but avoids crashing the loop).
        /// </summary>
        public static int ValidateAnnounceCard(int response, string stateName, int botId)
        {
            if (response <= 0)
                BotLogger.Warn(botId, $"[{stateName}] Card ID={response} is not positive. Sending 0 (may be rejected by Omega).");
            return response;
        }
    }
}
