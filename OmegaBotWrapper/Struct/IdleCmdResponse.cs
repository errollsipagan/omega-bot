using OmegaBotWrapper.Enum;

namespace OmegaBotWrapper.Struct
{
    /// <summary>
    /// Contains response data for IdleCmd state
    /// </summary>
    public struct IdleCmdResponse
    {
        /// <summary>
        /// Action to be taken
        /// </summary>
        public MainAction Action;
        /// <summary>
        /// Index parameter of the action (when applicable)
        /// </summary>
        public ushort Index;
        /// <summary>
        /// Creates a response that does not need an index parameter
        /// </summary>
        /// <param name="action">Action to be taken</param>
        public IdleCmdResponse(MainAction action)
        {
            Action = action;
            Index = 0;
        }
        /// <summary>
        /// Creates a response with an action and an index for that action
        /// </summary>
        /// <param name="action">Action to be taken</param>
        /// <param name="index">Index parameter for the action</param>
        public IdleCmdResponse(MainAction action, ushort index)
        {
            Action = action;
            Index = index;
        }
        /// <summary>
        /// Implicitly converts a main action to a response to IdleCmd without an index parameter
        /// </summary>
        /// <param name="action">Action to be taken</param>
        public static implicit operator IdleCmdResponse(MainAction action) => new(action);
        /// <summary>
        /// Implicitly converts a main action to a response to IdleCmd with an index parameter
        /// </summary>
        /// <param name="action">Action to be taken together wtih index parameter</param>
        public static implicit operator IdleCmdResponse((MainAction, ushort) action) => new(action.Item1, action.Item2);
    }
}
