using OmegaBotWrapper.Enum;

namespace OmegaBotWrapper.Struct
{
    /// <summary>
    /// Contains response data for BattleCmd state
    /// </summary>
    public struct BattleCmdResponse
    {
        /// <summary>
        /// Action to be taken
        /// </summary>
        public BattleAction Action;
        /// <summary>
        /// Index parameter of the action (when applicable)
        /// </summary>
        public ushort Index;
        /// <summary>
        /// Creates a response that does not need an index parameter
        /// </summary>
        /// <param name="action">Action to be taken</param>
        public BattleCmdResponse(BattleAction action)
        {
            Action = action;
            Index = 0;
        }
        /// <summary>
        /// Creates a response with an action and an index for that action
        /// </summary>
        /// <param name="action">Action to be taken</param>
        /// <param name="index">Index parameter for the action</param>
        public BattleCmdResponse(BattleAction action, ushort index)
        {
            Action = action;
            Index = index;
        }
        /// <summary>
        /// Implicitly converts a battle action to a BattleCmd response without an index parameter
        /// </summary>
        /// <param name="action">Action to be taken</param>
        public static implicit operator BattleCmdResponse(BattleAction action) => new(action);
        /// <summary>
        /// Implicitly converts a battle action to a BattleCmd response with an index parameter
        /// </summary>
        /// <param name="action">Action to be taken together wtih index parameter</param>
        public static implicit operator BattleCmdResponse((BattleAction, ushort) action) => new(action.Item1, action.Item2);
    }
}
