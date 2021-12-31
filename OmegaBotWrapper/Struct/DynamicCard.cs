namespace OmegaBotWrapper.Struct
{
    /// <summary>
    /// Contains dynamic data about the card on the game state
    /// </summary>
    public struct DynamicCard
    {
        /// <summary>
        /// Current ID of the card
        /// </summary>
        public int ID;
        /// <summary>
        /// Current alias ID of the card
        /// </summary>
        public int Alias;
        /// <summary>
        /// Current card type
        /// </summary>
        public int Type;
        /// <summary>
        /// Current card level
        /// </summary>
        public sbyte Level;
        /// <summary>
        /// Current card rank
        /// </summary>
        public sbyte Rank;
        /// <summary>
        /// Current card left pendulum scale
        /// </summary>
        public byte LScale;
        /// <summary>
        /// Current card right pendulum scale
        /// </summary>
        public byte RScale;
        /// <summary>
        /// Current card link markers' mask
        /// </summary>
        public ushort LinkMarker;
        /// <summary>
        /// Current card attributes
        /// </summary>
        public byte Attribute;
        /// <summary>
        /// Current card races
        /// </summary>
        public int Race;
        /// <summary>
        /// Current card attack points
        /// </summary>
        public int Attack;
        /// <summary>
        /// Current card base attack points
        /// </summary>
        public int BaseAttack;
        /// <summary>
        /// Current card defense points
        /// </summary>
        public int Defense;
        /// <summary>
        /// Current card base defense points
        /// </summary>
        public int BaseDefense;
        /// <summary>
        /// Current coordinates where the card is located on the duel
        /// </summary>
        public CardCoordinate CurrentCoordinates;
        /// <summary>
        /// Position on which the card is currently in
        /// </summary>
        public byte Position;
        /// <summary>
        /// Player who owns the card
        /// </summary>
        public byte Owner;
        /// <summary>
        /// Selection sequence of the card for being used during specific operations
        /// </summary>
        public byte SelectSeq;
        /// <summary>
        /// Operation parameter 1 of the card for being used during specific operations
        /// </summary>
        public int OpParam1;
        /// <summary>
        /// Operation parameter 2 of the card for being used during specific operations
        /// </summary>
        public int OpParam2;
        /// <summary>
        /// Latest reason of why the card is on its current state
        /// </summary>
        public int Reason;
        /// <summary>
        /// Indexes of the actions for actions that references this card (Used on main phase and battle phase) (eg. ActionIndex[(int)MainAction.Summon] is the action index for summoning this card on main phase)
        /// </summary>
        public int[] ActionIndex;
        /// <summary>
        /// Indexes of activation actions for actions that references this card with descriptions for multiple options (Used on main phase) (eg. ActivableCards[0].GetActivationIndexForDescription(ActivableDescriptions[0]))
        /// </summary>
        public ActivationCardIndex[] ActionActivateIndex;
        /// <summary>
        /// Location of where the card that caused the reason is located at
        /// </summary>
        public CardCoordinate ReasonCard;
        /// <summary>
        /// Location of where the card that this card is equipped to is
        /// </summary>
        public CardCoordinate EquipTarget;
        /// <summary>
        /// Location of where a card that is negating card is located at
        /// </summary>
        public CardCoordinate Negator;
        /// <summary>
        /// Location of where the card this card is negating is located at
        /// </summary>
        public CardCoordinate Negating;
        /// <summary>
        /// Locations of all cards that are equipped to this card
        /// </summary>
        public CardCoordinate[] EquipCards;
        public CardCoordinate[] OwnTargets;
        public CardCoordinate[] ChainOnlyOwnTargets;
        public CardCoordinate[] TargetCards;
        public CardCoordinate[] CurrentChainTargets;
        /// <summary>
        /// IDs of the cards that are XYZ materials for this card currently
        /// </summary>
        public int[] Overlays;
        /// <summary>
        /// Additional effect descriptions added by other cards
        /// </summary>
        public int[] AdditionalDescs;
        /// <summary>
        /// Additional effect texts added by other cards
        /// </summary>
        public string[] AdditionalTexts;
        /// <summary>
        /// Counters that this card currently has type and amount
        /// </summary>
        public ushort[][] Counters;
        /// <summary>
        /// Whether this card can declare an direct attack
        /// </summary>
        public bool CanDirectAttack;
        /// <summary>
        /// Whether this card was attacked
        /// </summary>
        public bool Attacked;
        /// <summary>
        /// Whether this card was special summoned
        /// </summary>
        public bool IsSpecialSummoned;
        /// <summary>
        /// Current status of this card
        /// </summary>
        public int Status;
    }
}
