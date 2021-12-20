namespace OmegaBotWrapper.Struct
{
    /// <summary>
    /// Information about the field
    /// </summary>
    public struct FieldInfo
    {
        /// <summary>
        /// Side of the requester of the query
        /// </summary>
        public CardInfo OwnSide;
        /// <summary>
        /// Side of the opponent of the requester of the query
        /// </summary>
        public CardInfo EnemySide;
    }
    /// <summary>
    /// Information about cards into a side of the field
    /// </summary>
    public struct CardInfo
    {
        /// <summary>
        /// LP remaining
        /// </summary>
        public int LifePoints;
        /// <summary>
        /// Mask of linked zones
        /// </summary>
        public int LinkedZones;
        /// <summary>
        /// Card ids known for main deck
        /// </summary>
        public int[] Main;
        /// <summary>
        /// Card ids known for extra deck
        /// </summary>
        public int[] Extra;
        /// <summary>
        /// Card ids known for graveyard
        /// </summary>
        public int[] Grave;
        /// <summary>
        /// Card ids known for banished zone
        /// </summary>
        public int[] Banish;
        /// <summary>
        /// Card ids known for hand
        /// </summary>
        public int[] Hand;
        /// <summary>
        /// Cards contained into each of the monster zones
        /// </summary>
        public CardArray[] MonsterZones;
        /// <summary>
        /// Cards contained into each of the spell zones
        /// </summary>
        public CardArray[] SpellZones;
    }
    /// <summary>
    /// IDs of cards contained into given slot
    /// </summary>
    public struct CardArray
    {
        /// <summary>
        /// Card ids contained on this slot
        /// </summary>
        public int[] Cards;
    }
}
