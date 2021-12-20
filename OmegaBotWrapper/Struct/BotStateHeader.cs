namespace OmegaBotWrapper.Struct
{
    /// <summary>
    /// General header for bot state request information
    /// </summary>
    public struct BotStateHeader
    {
        /// <summary>
        /// Numeric representation of the bot current state
        /// </summary>
        public byte CurrentStateCode;
        /// <summary>
        /// String representation of the bot current state
        /// </summary>
        public string CurrentStateName;
    }
}
