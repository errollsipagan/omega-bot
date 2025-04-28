namespace OmegaBotWrapper
{
    /// <summary>
    /// Contains constant data for reference
    /// </summary>
    public static class Const
    {
        /// <summary>
        /// Time in ms between heartbeats of the bot
        /// </summary>
        public const int HEARTBEAT_DELAY = 50;
        /// <summary>
        /// Base end point of the api
        /// </summary>
        public const string BASE_API = "http://127.0.0.1:9999";
        /// <summary>
        /// End point for querying the bot current state
        /// </summary>
        public const string GET_BOT_STATE = BASE_API + "/get-bot-state";
        /// <summary>
        /// End point for querying the bot current state header
        /// </summary>
        public const string GET_BOT_STATE_HEADER = BASE_API + "/get-bot-state-header";
        /// <summary>
        /// End point to define actions to be taken by the bot
        /// </summary>
        public const string SET_BOT_RESPONSE = BASE_API + "/set-bot-response";
        /// <summary>
        /// End point to retrieve a list of bots you own on the room
        /// </summary>
        public const string GET_BOT_LIST = BASE_API + "/bot-list";
        /// <summary>
        /// End point to define the bot data on the duel room
        /// </summary>
        public const string SET_BOT_DATA = BASE_API + "/set-bot-data";
        /// <summary>
        /// End point to request information about bot side of the field
        /// </summary>
        public const string GET_BOT_FIELD_INFO = BASE_API + "/bot-field-info";
        /// <summary>
        /// End point to request information about player side of the field
        /// </summary>
        public const string GET_PLAYER_FIELD_INFO = BASE_API + "/field-info";
        /// <summary>
        /// End point to request information about a card on duel on the player perspective
        /// </summary>
        public const string GET_PLAYER_FIELD_CARD_INFO = BASE_API + "/field-card-info";
        /// <summary>
        /// End point to request information about a card on duel on the bot perspective
        /// </summary>
        public const string GET_BOT_FIELD_CARD_INFO = BASE_API + "/bot-field-card-info";
        /// <summary>
        /// End point to request the player's current assigned deck
        /// </summary>
        public const string GET_PLAYER_CURRENT_DECK = BASE_API + "/current-deck";
        /// <summary>
        /// End point to request card list with matching filters
        /// </summary>
        public const string GET_MATCHING_CARDS = BASE_API + "/card-list";
        /// <summary>
        /// End point to the rematch request
        /// </summary>
        public const string REMATCH_DUEL = BASE_API + "/rematch-duel";
    }
    public static class Zones
    {
        public const int z0 = 0x1,
        z1 = 0x2,
        z2 = 0x4,
        z3 = 0x8,
        z4 = 0x10,
        z5 = 0x20,
        z6 = 0x40,

        MonsterZones = 0x7f,
        MainMonsterZones = 0x1f,
        ExtraMonsterZones = 0x60,

        SpellZones = 0x1f,
        LeftPendulumZone = 0x1,
        RightPendulumZone = 0x2,
        PendulumZones = 0x3,


        LinkedZones = 0x10000,
        NotLinkedZones = 0x20000;

        public static bool IsAvailable(int zone, int filter) => (filter & zone) > 0;
    }
}
