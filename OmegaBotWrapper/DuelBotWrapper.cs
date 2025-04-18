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
        /// <param name="deckName">Name of the deck being used</param>
        /// <param name="deckHash">Deck code (hash) of the deck being used</param>
        public DuelBotWrapper(string deckName, string deckHash)
        {
            DeckName = deckName;
            DeckHash = deckHash;
            BotID = -1;
        }
        /// <summary>
        /// Creates a new bot wrapper with given set of actions
        /// </summary>
        /// <param name="botActions">Bot actions object containing all instructions and data about this bot instance behaviour</param>
        public DuelBotWrapper(IBotActions botActions)
        {
            botActions.BotWrapper = this;
            DeckName = botActions.DeckName ?? string.Empty;
            DeckHash = botActions.DeckHash ?? string.Empty;
            BotID = -1;
            botActions.RegisterActions(this);
        }
        /// <summary>
        /// Connects the bot to the given client bot slot
        /// </summary>
        /// <param name="slot">Slot of the bot that this instance will control or -1 for first available</param>
        /// <returns>True if the bot was found and connection was realized or False in case there was no slot available or the given slot was already taken or non-existant</returns>
        public async Task<bool> Connect(int slot = -1)
        {
            var botList = await WebHandler.DownloadObject<BotList>(Const.GET_BOT_LIST);
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
                    var botState = await WebHandler.DownloadObject<BotGeneralState>($"{Const.GET_BOT_STATE}/{bot.BotID}");
                    if (botState.CurrentStateHeader.CurrentStateCode != (byte)BotState.WaitingForDeck || botState.CurrentStateData.IsReady)
                        continue;
                    BotID = bot.BotID;
                    break;
                }
            }
            if (BotID < 0)
                return false;
            var resp = await WebHandler.DownloadObject<ResponseReason>($"{Const.SET_BOT_DATA}/{BotID}/{DeckName}/{DeckHash.ToSafeB64()}");
            return resp.Code == 200;
        }
        /// <summary>
        /// Starts all the bot procedures
        /// </summary>
        /// <returns>Will only return once the bot has finished executing</returns>
        public async Task Start()
        {
            try
            {
                while (await Heartbeat())
                {
                    await Task.Delay(Const.HEARTBEAT_DELAY);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Bot heartbeat returned non-success message: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("The bot has finished execution");
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
        /// <summary>
        /// URL for acquiring this bot instance state header
        /// </summary>
        private string BotStateHeaderURL { get => $"{Const.GET_BOT_STATE_HEADER}/{BotID}"; }
        /// <summary>
        /// URL for acquiring this bot instance state
        /// </summary>
        private string BotStateURL { get => $"{Const.GET_BOT_STATE}/{BotID}"; }
        public BotStateHeader CurrentStateHeader;
        /// <summary>
        /// Main loop of actions from the bot
        /// </summary>
        /// <returns>Will return True everytime the bot finishes one loop without error, or False otherwise</returns>
        private async Task<bool> Heartbeat()
        {
            CurrentStateHeader = await WebHandler.DownloadObject<BotStateHeader>(BotStateHeaderURL);
            using MemoryStream ms = new();
            using BinaryWriter pkt = new(ms);
            switch ((BotState)CurrentStateHeader.CurrentStateCode)
            {
                case BotState.SelectBattleCmd:
                    if (OnBattleCmdAction == null)
                        return false;
                    var battleState = await WebHandler.DownloadObject<BattleCmdState>(BotStateURL);
                    var battleResponse = await OnBattleCmdAction(battleState.CurrentStateData);
                    pkt.Write((ushort)battleResponse.Action); // Action to be taken on battle phase (2 bytes)
                    pkt.Write(battleResponse.Index); // Index of the action (or 0 if not applicable) (2 bytes)
                    break; 
                case BotState.SelectIdleCmd:
                    if (OnIdleCmdAction == null)
                        return false;
                    var idleState = await WebHandler.DownloadObject<IdleCmdState>(BotStateURL);
                    var idleResponse = await OnIdleCmdAction(idleState.CurrentStateData);
                    pkt.Write((ushort)idleResponse.Action); // Action to be taken on main phase (2 bytes)
                    pkt.Write(idleResponse.Index); // Index of the action (or 0 if not applicable) (2 bytes)
                    break;
                case BotState.SelectCard:
                    if (OnSelectCardAction == null)
                        return false;
                    var selectCardState = await WebHandler.DownloadObject<SelectCardState>(BotStateURL);
                    var selectCardResponse = await OnSelectCardAction(selectCardState.CurrentStateData) ?? Array.Empty<byte>();
                    pkt.Write((byte)selectCardResponse.Count); // Length of the selected options (1 byte)
                    foreach (byte optionIndex in selectCardResponse)
                        pkt.Write(optionIndex); // Index of the option picked (1 byte)
                    break;
                case BotState.SelectUnselectCard:
                    if (OnSelectUnselectCardAction == null)
                        return false;
                    var selectUnselectCardState = await WebHandler.DownloadObject<SelectUnselectCardState>(BotStateURL);
                    var selectUnselectCardResponse = await OnSelectUnselectCardAction(selectUnselectCardState.CurrentStateData) ?? Array.Empty<byte>();
                    pkt.Write((byte)selectUnselectCardResponse.Count); // Length of the selected options (1 byte)
                    foreach (byte optionIndex in selectUnselectCardResponse)
                        pkt.Write(optionIndex); // Index of the option picked (1 byte)
                    break;
                case BotState.SelectChain:
                    if (OnSelectChainAction == null)
                        return false;
                    var selectChainState = await WebHandler.DownloadObject<SelectChainState>(BotStateURL);
                    var selectChainResponse = await OnSelectChainAction(selectChainState.CurrentStateData);
                    pkt.Write(selectChainResponse); // Index of selected item to chain, or -1 for none if allowed (4 bytes)
                    break;
                case BotState.SelectCounter:
                    if (OnSelectCounterAction == null)
                        return false;
                    var selectCounterState = await WebHandler.DownloadObject<SelectCounterState>(BotStateURL);
                    var selectCounterResponse = await OnSelectCounterAction(selectCounterState.CurrentStateData);
                    pkt.Write((byte)selectCounterResponse.Count); // Amount of indexes on the selection (1 byte)
                    foreach (short counterAmount in selectCounterResponse)
                        pkt.Write(counterAmount); // Amount of counters picked from this index (2 bytes)
                    break;
                case BotState.SelectDisfield:
                    if (OnSelectDisfieldAction == null)
                        return false;
                    var selectDisfieldState = await WebHandler.DownloadObject<SelectDisfieldState>(BotStateURL);
                    var selectDisfieldResponse = await OnSelectDisfieldAction(selectDisfieldState.CurrentStateData);
                    pkt.Write(selectDisfieldResponse); // integer of the zone selected (4 bytes)
                    break;
                case BotState.SelectEffectYn:
                    if (OnSelectEffectYnAction == null)
                        return false;
                    var selectEffectYnState = await WebHandler.DownloadObject<SelectEffectYnState>(BotStateURL);
                    var selectEffectYnResponse = await OnSelectEffectYnAction(selectEffectYnState.CurrentStateData);
                    pkt.Write((byte)(selectEffectYnResponse ? 1 : 0)); // True = 1, False = 0 (1 byte)
                    break;
                case BotState.SelectOption:
                    if (OnSelectOptionAction == null)
                        return false;
                    var selectOptionState = await WebHandler.DownloadObject<SelectOptionState>(BotStateURL);
                    var selectedOptionResponse = await OnSelectOptionAction(selectOptionState.CurrentStateData);
                    pkt.Write(selectedOptionResponse); // Index of the selected option (4 bytes)
                    break;
                case BotState.SelectPlace:
                    if (OnSelectPlaceAction == null)
                        return false;
                    var selectPlaceState = await WebHandler.DownloadObject<SelectPlaceState>(BotStateURL);
                    var selectPlaceResponse = await OnSelectPlaceAction(selectPlaceState.CurrentStateData);
                    pkt.Write(selectPlaceResponse); // integer of the zone selected (4 bytes)
                    break;
                case BotState.SelectPosition:
                    if (OnSelectPositionAction == null)
                        return false;
                    var selectPositionState = await WebHandler.DownloadObject<SelectPositionState>(BotStateURL);
                    var selectPositionResponse = await OnSelectPositionAction(selectPositionState.CurrentStateData);
                    pkt.Write((byte)selectPositionResponse); // Position selected (1 byte)
                    break;
                case BotState.SelectSum:
                    if (OnSelectSumAction == null)
                        return false;
                    var selectSumState = await WebHandler.DownloadObject<SelectSumState>(BotStateURL);
                    var selectSumResponse = await OnSelectSumAction(selectSumState.CurrentStateData) ?? Array.Empty<byte>();
                    pkt.Write((byte)selectSumResponse.Count); // Length of the selected options (1 byte)
                    foreach (byte optionIndex in selectSumResponse)
                        pkt.Write(optionIndex); // Index of the option picked (1 byte)
                    break;
                case BotState.SelectTribute:
                    if (OnSelectTributeAction == null)
                        return false;
                    var selectTributeState = await WebHandler.DownloadObject<SelectTributeState>(BotStateURL);
                    var selectTributeResponse = await OnSelectTributeAction(selectTributeState.CurrentStateData) ?? Array.Empty<byte>();
                    pkt.Write((byte)selectTributeResponse.Count); // Length of the selected options (1 byte)
                    foreach (byte optionIndex in selectTributeResponse)
                        pkt.Write(optionIndex); // Index of the option picked (1 byte)
                    break;
                case BotState.AnnounceAttribute:
                    if (OnAnnounceAttributeAction == null)
                        return false;
                    var announceAttributeState = await WebHandler.DownloadObject<AnnounceAttributeState>(BotStateURL);
                    var announceAttributeResponse = await OnAnnounceAttributeAction(announceAttributeState.CurrentStateData);
                    int announcedAttribute = 0; // Initializes a mask with no attribute selected
                    foreach (var attributeSelected in announceAttributeResponse)
                        announcedAttribute += (int)attributeSelected; // Add the selected attribute to the selection mask
                    pkt.Write(announcedAttribute); // Mask representing all the selected attributes (4 bytes)
                    break;
                case BotState.AnnounceCard:
                    if (OnAnnounceCardAction == null)
                        return false;
                    var announceCardState = await WebHandler.DownloadObject<AnnounceCardState>(BotStateURL);
                    var announceCardResponse = await OnAnnounceCardAction(announceCardState.CurrentStateData);
                    pkt.Write(announceCardResponse); // ID of the selected card (4 bytes)
                    break;
                case BotState.AnnounceNumber:
                    if (OnAnnounceNumberAction == null)
                        return false;
                    var announceNumberState = await WebHandler.DownloadObject<AnnounceNumberState>(BotStateURL);
                    var announceNumberResponse = await OnAnnounceNumberAction(announceNumberState.CurrentStateData);
                    pkt.Write(announceNumberResponse); // Selected index from the selectable number options (4 bytes)
                    break;
                case BotState.AnnounceRace:
                    if (OnAnnounceRaceAction == null)
                        return false;
                    var announceRaceState = await WebHandler.DownloadObject<AnnounceRaceState>(BotStateURL);
                    var announceRaceResponse = await OnAnnounceRaceAction(announceRaceState.CurrentStateData);
                    int announcedRace = 0; // Initializes a mask with no race selected
                    foreach (var raceSelected in announceRaceResponse)
                        announcedRace += (int)raceSelected; // Add the selected race to the selection mask
                    pkt.Write(announcedRace); // Mask representing all the selected races (4 bytes)
                    break;
                case BotState.AnnounceCardFilter:
                    if (OnAnnounceCardFilterAction == null)
                        return false;
                    var announceCardFilterState = await WebHandler.DownloadObject<AnnounceCardFilterState>(BotStateURL);
                    var announceCardFilterResponse = await OnAnnounceCardFilterAction(announceCardFilterState.CurrentStateData);
                    pkt.Write(announceCardFilterResponse); // ID of the selected card (4 bytes)
                    break;
                case BotState.SelectYesNo:
                    if (OnSelectYesNoAction == null)
                        return false;
                    var selectYesNoState = await WebHandler.DownloadObject<SelectYesNoState>(BotStateURL);
                    var selectYesNoResponse = await OnSelectYesNoAction(selectYesNoState.CurrentStateData);
                    pkt.Write((byte)(selectYesNoResponse ? 1 : 0)); // True = 1, False = 0 (1 byte)
                    break;
                case BotState.RockPaperScissors:
                    if (OnSelectRockPaperScissors == null)
                        return false;
                    var rpsResponse = await OnSelectRockPaperScissors();
                    pkt.Write(rpsResponse); // 1 = scissors, 2 = rock, 3 = paper (1 byte)
                    break;
                case BotState.SortCard:
                    if (OnSortCardAction == null)
                        return false;
                    var sortCardState = await WebHandler.DownloadObject<SortCardState>(BotStateURL);
                    var sortCardResponse = await OnSortCardAction(sortCardState.CurrentStateData);
                    pkt.Write((byte)sortCardResponse.Count); // Size of the selection (should always be same size as the selectable cards size)
                    foreach (var sortedCardIndex in sortCardResponse)
                        pkt.Write(sortedCardIndex); // Index of the selectable card to be on this position
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
        public async Task<FieldInfo> GetBotFieldInfo()
        {
            return await WebHandler.DownloadObject<FieldInfo>($"{Const.GET_BOT_FIELD_INFO}/{BotID}");
        }
        /// <summary>
        /// Queries information about the player side of the field (aka. cheating)
        /// </summary>
        /// <returns>Structure containing the data</returns>
        public static async Task<FieldInfo> GetPlayerFieldInfo()
        {
            return await WebHandler.DownloadObject<FieldInfo>($"{Const.GET_PLAYER_FIELD_INFO}");
        }
        /// <summary>
        /// Queries information about a card into player's perspective (aka. cheating)
        /// </summary>
        /// <param name="controller">Controller of the card</param>
        /// <param name="location">Location of the card on given controller</param>
        /// <param name="sequence">Sequence of the card on given location</param>
        /// <returns>Structure with dynamic card data</returns>
        public static async Task<DynamicCard> GetPlayerFieldCardInfo(int controller, int location, int sequence)
        {
            return await WebHandler.DownloadObject<DynamicCard>($"{Const.GET_PLAYER_FIELD_CARD_INFO}/{controller}/{location}/{sequence}");
        }
        /// <summary>
        /// Queries information about a card into bot's perspective
        /// </summary>
        /// <param name="controller">Controller of the card</param>
        /// <param name="location">Location of the card on given controller</param>
        /// <param name="sequence">Sequence of the card on given location</param>
        /// <returns>Structure with dynamic card data</returns>
        public async Task<DynamicCard> GetBotFieldCardInfo(int controller, int location, int sequence)
        {
            return await WebHandler.DownloadObject<DynamicCard>($"{Const.GET_BOT_FIELD_CARD_INFO}/{BotID}/{controller}/{location}/{sequence}");
        }
        /// <summary>
        /// Gets the deck currently being used by the player
        /// </summary>
        /// <returns>Information about the deck being used by the player</returns>
        public static async Task<DeckData> GetPlayerDeck()
        {
            return await WebHandler.DownloadObject<DeckData>(Const.GET_PLAYER_CURRENT_DECK);
        }
        /// <summary>
        /// Sends packet of response to the bot
        /// </summary>
        /// <param name="pkt">Binary packet with response data</param>
        /// <returns>True if there was no error, False otherwise</returns>
        public async Task<bool> SendResponse(BinaryWriter pkt)
        {
            return await pkt.SendResponsePacket(BotID);
        }
        /// <summary>
        /// Gets a list of cards from client database which matches the given filters
        /// </summary>
        /// <param name="filters">Filters to match cards</param>
        /// <returns>List of cards that matched the filters</returns>
        public static async Task<DBCardList> GetMatchingCards(params int[] filters)
        {
            var endPoint = $"{Const.GET_MATCHING_CARDS}/";
            foreach (var filter in filters)
                endPoint = $"filter,";
            if (endPoint.EndsWith(','))
                endPoint = endPoint.Remove(endPoint.Length - 1);
            return await WebHandler.DownloadObject<DBCardList>(endPoint);
        }
        /// <summary>
        /// While on end screen on rematchable duel it will send the rematch request as if you tapped rematch
        /// </summary>
        /// <returns>Rematch Duel without UI interaction</returns>
        public static Task RematchDuel()
        {
            return WebHandler.DownloadString($"{Const.REMATCH_DUEL}");
        }
    }
}