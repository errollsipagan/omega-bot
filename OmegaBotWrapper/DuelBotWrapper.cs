using OmegaBotWrapper.Struct;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.States;

namespace OmegaBotWrapper
{
    /// <summary>
    /// Global wrapper to access the duel bot functionality
    /// </summary>
    public class DuelBotWrapper
    {
        private readonly OmegaApiClient _api;

        /// <summary>
        /// Which bot this instance is connected to
        /// </summary>
        public int BotID { get; private set; }
        /// <summary>
        /// Name of the deck the bot is currently assigned to
        /// </summary>
        public string DeckName { get; private set; }
        /// <summary>
        /// Deck code (hash) of the deck the bot is currently assigned to
        /// </summary>
        public string DeckHash { get; private set; }
        /// <summary>
        /// Creates a new bot wrapper with given deck
        /// </summary>
        public DuelBotWrapper(string deckName, string deckHash, OmegaApiClient? api = null)
        {
            DeckName = deckName;
            DeckHash = deckHash;
            BotID = -1;
            _api = api ?? OmegaApiClient.Default;
        }
        /// <summary>
        /// Creates a new bot wrapper with given set of actions
        /// </summary>
        public DuelBotWrapper(IBotActions botActions, OmegaApiClient? api = null)
        {
            botActions.BotWrapper = this;
            DeckName = botActions.DeckName ?? string.Empty;
            DeckHash = botActions.DeckHash ?? string.Empty;
            BotID = -1;
            _api = api ?? OmegaApiClient.Default;
            botActions.RegisterActions(this);
        }
        /// <summary>
        /// Connects the bot to the given client bot slot
        /// </summary>
        /// <param name="slot">Slot of the bot that this instance will control or -1 for first available</param>
        /// <returns>True if the bot was found and connection was realized or False in case there was no slot available or the given slot was already taken or non-existant</returns>
        public async Task<bool> Connect(int slot = -1, CancellationToken ct = default)
        {
            var botList = await _api.GetBotList(ct);
            if (botList.BotCount <= 0)
                return false;
            if (slot >= 0)
            {
                if (botList.BotCount <= slot || botList.Bots[slot].DeckIndex != 0)
                    return false;
                BotID = botList.Bots[slot].BotID;
            }
            else
            {
                foreach (var bot in botList.Bots)
                {
                    if (bot.DeckIndex != 0)
                        continue;
                    var botState = await _api.GetBotState(bot.BotID, ct);
                    if (botState.CurrentStateHeader.CurrentStateCode != (byte)BotState.WaitingForDeck || botState.CurrentStateData.IsReady)
                        continue;
                    BotID = bot.BotID;
                    break;
                }
            }
            if (BotID < 0)
                return false;
            return await _api.SetBotData(BotID, DeckName, DeckHash.ToSafeB64(), ct);
        }
        /// <summary>
        /// Starts all the bot procedures
        /// </summary>
        /// <returns>Will only return once the bot has finished executing</returns>
        public async Task Start(CancellationToken ct = default)
        {
            try
            {
                while (!ct.IsCancellationRequested && await Heartbeat(ct))
                {
                    await Task.Delay(Const.HEARTBEAT_DELAY, ct);
                }
            }
            catch (OperationCanceledException)
            {
                BotLogger.Log(BotID, "System", "Bot stopped by cancellation.");
            }
            catch (Exception ex)
            {
                BotLogger.Error(BotID, "Heartbeat failed", ex);
            }
            finally
            {
                BotLogger.Log(BotID, "System", "Bot has finished execution");
            }
        }
        /// <summary>
        /// Action to be taken at battle phase, should return the action and its index (or 0 if not applicable)
        /// </summary>
        public Func<BattleCmdData, Task<BattleCmdResponse>>? OnBattleCmdAction { get; set; }
        /// <summary>
        /// Action to be taken at main phase, should return the action and its index (or 0 if not applicable)
        /// </summary>
        public Func<IdleCmdData, Task<IdleCmdResponse>>? OnIdleCmdAction { get; set; }
        /// <summary>
        /// Action to be taken when selecting something to chain, should return the index of the selected chain to activate or, if allowed, -1 for none
        /// </summary>
        public Func<SelectChainData, Task<int>>? OnSelectChainAction { get; set; }
        /// <summary>
        /// Action to be taken when selecting card(s), should return the indexes that was selected from the selectable options
        /// </summary>
        public Func<SelectCardData, Task<IList<byte>?>>? OnSelectCardAction { get; set; }
        /// <summary>
        /// Action to be taken when selecting tribute(s), should return the indexes that was selected from the selectable options
        /// </summary>
        public Func<SelectTributeData, Task<IList<byte>?>>? OnSelectTributeAction { get; set; }
        /// <summary>
        /// Action to be taken when selecting a sum (eg. synchro/ritual), should return the indexes that was selected from the selectable options
        /// </summary>
        public Func<SelectSumData, Task<IList<byte>?>>? OnSelectSumAction { get; set; }
        /// <summary>
        /// Action to be taken when announcing attribute(s), should return the attribute(s) selected
        /// </summary>
        public Func<AnnounceAttributeData, Task<IList<CardAttribute>>>? OnAnnounceAttributeAction { get; set; }
        /// <summary>
        /// Action to be taken when announcing race(s), should return the race(s) selected
        /// </summary>
        public Func<AnnounceRaceData, Task<IList<CardRace>>>? OnAnnounceRaceAction { get; set; }
        /// <summary>
        /// Action to be taken when announcing a number, should return the index of the number selected from the selectable options
        /// </summary>
        public Func<AnnounceNumberData, Task<int>>? OnAnnounceNumberAction { get; set; }
        /// <summary>
        /// Action to be taken when announcing a card, should return the card id that was selected
        /// </summary>
        public Func<AnnounceCardData, Task<int>>? OnAnnounceCardAction { get; set; }
        /// <summary>
        /// Action to be taken when announcing a card with filter, should return the card id that was selected
        /// </summary>
        public Func<AnnounceCardFilterData, Task<int>>? OnAnnounceCardFilterAction { get; set; }
        /// <summary>
        /// Action to be taken when selecting/unselecting card(s), should return the indexes that was selected from the selectable options
        /// </summary>
        public Func<SelectUnselectCardData, Task<IList<byte>?>>? OnSelectUnselectCardAction { get; set; }
        /// <summary>
        /// Action to be taken when selecting counter(s), should return the amount of counters picked from each of the indexes that was present on the selectable options
        /// </summary>
        public Func<SelectCounterData, Task<IList<short>>>? OnSelectCounterAction { get; set; }
        /// <summary>
        /// Action to be taken when selecting field zone for disabling, should return the integer representing the zone selected (use the 'Zones' class helper for values)
        /// </summary>
        public Func<SelectDisfieldData, Task<int>>? OnSelectDisfieldAction { get; set; }
        /// <summary>
        /// Action to be taken when selecting field zone, should return the integer representing the zone selected (use the 'Zones' class helper for values)
        /// </summary>
        public Func<SelectPlaceData, Task<int>>? OnSelectPlaceAction { get; set; }
        /// <summary>
        /// Action to be taken when selecting yes or no for an effect, should return true or false
        /// </summary>
        public Func<SelectEffectYnData, Task<bool>>? OnSelectEffectYnAction { get; set; }
        /// <summary>
        /// Action to be taken when selecting yes or no, should return true or false
        /// </summary>
        public Func<SelectYesNoData, Task<bool>>? OnSelectYesNoAction { get; set; }
        /// <summary>
        /// Action to be taken when selecting an option, should return the index of the selected option
        /// </summary>
        public Func<SelectOptionData, Task<int>>? OnSelectOptionAction { get; set; }
        /// <summary>
        /// Action to be taken when selecting a rock paper scissors option, should return 1 = scissors, 2 = rock, 3 = paper 
        /// </summary>
        public Func<Task<byte>>? OnSelectRockPaperScissors { get; set; }
        /// <summary>
        /// Action to be taken when selecting position, should return the position selected
        /// </summary>
        public Func<SelectPositionData, Task<CardPosition>>? OnSelectPositionAction { get; set; }
        /// <summary>
        /// Action to be taken when sorting cards, should return the indexes of the selectable cards in the desired order
        /// </summary>
        public Func<SortCardData, Task<IList<byte>>>? OnSortCardAction { get; set; }
        public BotStateHeader CurrentStateHeader;
        /// <summary>
        /// Main loop of actions from the bot
        /// </summary>
        /// <returns>Will return True everytime the bot finishes one loop without error, or False otherwise</returns>
        private async Task<bool> Heartbeat(CancellationToken ct = default)
        {
            CurrentStateHeader = await _api.GetBotStateHeader(BotID, ct);
            using MemoryStream ms = new();
            using BinaryWriter pkt = new(ms);
            var state = (BotState)CurrentStateHeader.CurrentStateCode;
            switch (state)
            {
                case BotState.SelectBattleCmd:
                    if (OnBattleCmdAction == null)
                    {
                        BotLogger.Warn(BotID, $"[{state}] No handler registered, skipping.");
                        return true;
                    }
                    var battleState = await _api.GetBotStateAs<BattleCmdState>(BotID, ct);
                    var battleResponse = await OnBattleCmdAction(battleState.CurrentStateData);
                    pkt.Write((ushort)battleResponse.Action);
                    pkt.Write(battleResponse.Index);
                    break;
                case BotState.SelectIdleCmd:
                    if (OnIdleCmdAction == null)
                    {
                        BotLogger.Warn(BotID, $"[{state}] No handler registered, skipping.");
                        return true;
                    }
                    var idleState = await _api.GetBotStateAs<IdleCmdState>(BotID, ct);
                    var idleResponse = await OnIdleCmdAction(idleState.CurrentStateData);
                    pkt.Write((ushort)idleResponse.Action);
                    pkt.Write(idleResponse.Index);
                    break;
                case BotState.SelectCard:
                    if (OnSelectCardAction == null)
                    {
                        BotLogger.Warn(BotID, $"[{state}] No handler registered, skipping.");
                        return true;
                    }
                    var selectCardState = await _api.GetBotStateAs<SelectCardState>(BotID, ct);
                    var selectCardResponse = await OnSelectCardAction(selectCardState.CurrentStateData) ?? Array.Empty<byte>();
                    pkt.Write((byte)selectCardResponse.Count);
                    foreach (byte optionIndex in selectCardResponse)
                        pkt.Write(optionIndex);
                    break;
                case BotState.SelectUnselectCard:
                    if (OnSelectUnselectCardAction == null)
                    {
                        BotLogger.Warn(BotID, $"[{state}] No handler registered, skipping.");
                        return true;
                    }
                    var selectUnselectCardState = await _api.GetBotStateAs<SelectUnselectCardState>(BotID, ct);
                    var selectUnselectCardResponse = await OnSelectUnselectCardAction(selectUnselectCardState.CurrentStateData) ?? Array.Empty<byte>();
                    pkt.Write((byte)selectUnselectCardResponse.Count);
                    foreach (byte optionIndex in selectUnselectCardResponse)
                        pkt.Write(optionIndex);
                    break;
                case BotState.SelectChain:
                    if (OnSelectChainAction == null)
                    {
                        BotLogger.Warn(BotID, $"[{state}] No handler registered, skipping.");
                        return true;
                    }
                    var selectChainState = await _api.GetBotStateAs<SelectChainState>(BotID, ct);
                    var selectChainResponse = await OnSelectChainAction(selectChainState.CurrentStateData);
                    pkt.Write(selectChainResponse);
                    break;
                case BotState.SelectCounter:
                    if (OnSelectCounterAction == null)
                    {
                        BotLogger.Warn(BotID, $"[{state}] No handler registered, skipping.");
                        return true;
                    }
                    var selectCounterState = await _api.GetBotStateAs<SelectCounterState>(BotID, ct);
                    var selectCounterResponse = await OnSelectCounterAction(selectCounterState.CurrentStateData);
                    pkt.Write((byte)selectCounterResponse.Count);
                    foreach (short counterAmount in selectCounterResponse)
                        pkt.Write(counterAmount);
                    break;
                case BotState.SelectDisfield:
                    if (OnSelectDisfieldAction == null)
                    {
                        BotLogger.Warn(BotID, $"[{state}] No handler registered, skipping.");
                        return true;
                    }
                    var selectDisfieldState = await _api.GetBotStateAs<SelectDisfieldState>(BotID, ct);
                    var selectDisfieldResponse = await OnSelectDisfieldAction(selectDisfieldState.CurrentStateData);
                    pkt.Write(selectDisfieldResponse);
                    break;
                case BotState.SelectEffectYn:
                    if (OnSelectEffectYnAction == null)
                    {
                        BotLogger.Warn(BotID, $"[{state}] No handler registered, skipping.");
                        return true;
                    }
                    var selectEffectYnState = await _api.GetBotStateAs<SelectEffectYnState>(BotID, ct);
                    var selectEffectYnResponse = await OnSelectEffectYnAction(selectEffectYnState.CurrentStateData);
                    pkt.Write((byte)(selectEffectYnResponse ? 1 : 0));
                    break;
                case BotState.SelectOption:
                    if (OnSelectOptionAction == null)
                    {
                        BotLogger.Warn(BotID, $"[{state}] No handler registered, skipping.");
                        return true;
                    }
                    var selectOptionState = await _api.GetBotStateAs<SelectOptionState>(BotID, ct);
                    var selectedOptionResponse = await OnSelectOptionAction(selectOptionState.CurrentStateData);
                    pkt.Write(selectedOptionResponse);
                    break;
                case BotState.SelectPlace:
                    if (OnSelectPlaceAction == null)
                    {
                        BotLogger.Warn(BotID, $"[{state}] No handler registered, skipping.");
                        return true;
                    }
                    var selectPlaceState = await _api.GetBotStateAs<SelectPlaceState>(BotID, ct);
                    var selectPlaceResponse = await OnSelectPlaceAction(selectPlaceState.CurrentStateData);
                    pkt.Write(selectPlaceResponse);
                    break;
                case BotState.SelectPosition:
                    if (OnSelectPositionAction == null)
                    {
                        BotLogger.Warn(BotID, $"[{state}] No handler registered, skipping.");
                        return true;
                    }
                    var selectPositionState = await _api.GetBotStateAs<SelectPositionState>(BotID, ct);
                    var selectPositionResponse = await OnSelectPositionAction(selectPositionState.CurrentStateData);
                    pkt.Write((byte)selectPositionResponse);
                    break;
                case BotState.SelectSum:
                    if (OnSelectSumAction == null)
                    {
                        BotLogger.Warn(BotID, $"[{state}] No handler registered, skipping.");
                        return true;
                    }
                    var selectSumState = await _api.GetBotStateAs<SelectSumState>(BotID, ct);
                    var selectSumResponse = await OnSelectSumAction(selectSumState.CurrentStateData) ?? Array.Empty<byte>();
                    pkt.Write((byte)selectSumResponse.Count);
                    foreach (byte optionIndex in selectSumResponse)
                        pkt.Write(optionIndex);
                    break;
                case BotState.SelectTribute:
                    if (OnSelectTributeAction == null)
                    {
                        BotLogger.Warn(BotID, $"[{state}] No handler registered, skipping.");
                        return true;
                    }
                    var selectTributeState = await _api.GetBotStateAs<SelectTributeState>(BotID, ct);
                    var selectTributeResponse = await OnSelectTributeAction(selectTributeState.CurrentStateData) ?? Array.Empty<byte>();
                    pkt.Write((byte)selectTributeResponse.Count);
                    foreach (byte optionIndex in selectTributeResponse)
                        pkt.Write(optionIndex);
                    break;
                case BotState.AnnounceAttribute:
                    if (OnAnnounceAttributeAction == null)
                    {
                        BotLogger.Warn(BotID, $"[{state}] No handler registered, skipping.");
                        return true;
                    }
                    var announceAttributeState = await _api.GetBotStateAs<AnnounceAttributeState>(BotID, ct);
                    var announceAttributeResponse = await OnAnnounceAttributeAction(announceAttributeState.CurrentStateData);
                    int announcedAttribute = 0;
                    foreach (var attributeSelected in announceAttributeResponse)
                        announcedAttribute += (int)attributeSelected;
                    pkt.Write(announcedAttribute);
                    break;
                case BotState.AnnounceCard:
                    if (OnAnnounceCardAction == null)
                    {
                        BotLogger.Warn(BotID, $"[{state}] No handler registered, skipping.");
                        return true;
                    }
                    var announceCardState = await _api.GetBotStateAs<AnnounceCardState>(BotID, ct);
                    var announceCardResponse = await OnAnnounceCardAction(announceCardState.CurrentStateData);
                    pkt.Write(announceCardResponse);
                    break;
                case BotState.AnnounceNumber:
                    if (OnAnnounceNumberAction == null)
                    {
                        BotLogger.Warn(BotID, $"[{state}] No handler registered, skipping.");
                        return true;
                    }
                    var announceNumberState = await _api.GetBotStateAs<AnnounceNumberState>(BotID, ct);
                    var announceNumberResponse = await OnAnnounceNumberAction(announceNumberState.CurrentStateData);
                    pkt.Write(announceNumberResponse);
                    break;
                case BotState.AnnounceRace:
                    if (OnAnnounceRaceAction == null)
                    {
                        BotLogger.Warn(BotID, $"[{state}] No handler registered, skipping.");
                        return true;
                    }
                    var announceRaceState = await _api.GetBotStateAs<AnnounceRaceState>(BotID, ct);
                    var announceRaceResponse = await OnAnnounceRaceAction(announceRaceState.CurrentStateData);
                    int announcedRace = 0;
                    foreach (var raceSelected in announceRaceResponse)
                        announcedRace += (int)raceSelected;
                    pkt.Write(announcedRace);
                    break;
                case BotState.AnnounceCardFilter:
                    if (OnAnnounceCardFilterAction == null)
                    {
                        BotLogger.Warn(BotID, $"[{state}] No handler registered, skipping.");
                        return true;
                    }
                    var announceCardFilterState = await _api.GetBotStateAs<AnnounceCardFilterState>(BotID, ct);
                    var announceCardFilterResponse = await OnAnnounceCardFilterAction(announceCardFilterState.CurrentStateData);
                    pkt.Write(announceCardFilterResponse);
                    break;
                case BotState.SelectYesNo:
                    if (OnSelectYesNoAction == null)
                    {
                        BotLogger.Warn(BotID, $"[{state}] No handler registered, skipping.");
                        return true;
                    }
                    var selectYesNoState = await _api.GetBotStateAs<SelectYesNoState>(BotID, ct);
                    var selectYesNoResponse = await OnSelectYesNoAction(selectYesNoState.CurrentStateData);
                    pkt.Write((byte)(selectYesNoResponse ? 1 : 0));
                    break;
                case BotState.RockPaperScissors:
                    if (OnSelectRockPaperScissors == null)
                    {
                        BotLogger.Warn(BotID, $"[{state}] No handler registered, skipping.");
                        return true;
                    }
                    var rpsResponse = await OnSelectRockPaperScissors();
                    pkt.Write(rpsResponse);
                    break;
                case BotState.SortCard:
                    if (OnSortCardAction == null)
                    {
                        BotLogger.Warn(BotID, $"[{state}] No handler registered, skipping.");
                        return true;
                    }
                    var sortCardState = await _api.GetBotStateAs<SortCardState>(BotID, ct);
                    var sortCardResponse = await OnSortCardAction(sortCardState.CurrentStateData);
                    pkt.Write((byte)sortCardResponse.Count);
                    foreach (var sortedCardIndex in sortCardResponse)
                        pkt.Write(sortedCardIndex);
                    break;
                case BotState.WaitingForDeck:
                case BotState.None:
                default:
                    return true;
            }
            return await SendResponse(pkt);
        }
        /// <summary>
        /// Queries information about the bot side of the field
        /// </summary>
        /// <returns>Structure containing the data</returns>
        public async Task<FieldInfo> GetBotFieldInfo() =>
            await _api.GetBotFieldInfo(BotID);

        public static async Task<FieldInfo> GetPlayerFieldInfo() =>
            await OmegaApiClient.Default.GetPlayerFieldInfo();

        public static async Task<DynamicCard> GetPlayerFieldCardInfo(int controller, int location, int sequence) =>
            await OmegaApiClient.Default.GetPlayerFieldCardInfo(controller, location, sequence);

        public async Task<DynamicCard> GetBotFieldCardInfo(int controller, int location, int sequence) =>
            await _api.GetBotFieldCardInfo(BotID, controller, location, sequence);

        public static async Task<DeckData> GetPlayerDeck() =>
            await OmegaApiClient.Default.GetPlayerDeck();

        public async Task<bool> SendResponse(BinaryWriter pkt)
        {
            byte[] data = ((MemoryStream)pkt.BaseStream).ToArray();
            string encoded = data.Base64Encode().ToSafeB64();
            return await _api.SendBotResponse(BotID, encoded);
        }

        public static async Task<DBCardList> GetMatchingCards(params int[] filters) =>
            await OmegaApiClient.Default.GetMatchingCards(filters);
        /// <summary>
        /// While on end screen on rematchable duel it will send the rematch request as if you tapped rematch
        /// </summary>
        /// <returns>Rematch Duel</returns>
        public static async Task<string> RematchDuel() =>
            await OmegaApiClient.Default.RematchDuel();
    }
}