using OmegaBotWrapper;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.Struct;
#pragma warning disable 1998
namespace BotTest
{
    public static class Actions
    {
        /// <summary>
        /// Registers all the actions the bot should take during specific states
        /// </summary>
        /// <param name="bot">Bot instance which actions are being registered</param>
        public static void RegisterActions(this DuelBotWrapper bot)
        {
            bot.OnBattleCmdAction = OnBattleCmd;
            bot.OnIdleCmdAction = OnIdleCmd;
            bot.OnSelectChainAction = OnSelectChain;
            bot.OnSelectCardAction = OnSelectCard;
        }

        public static async Task<Tuple<BattleAction, ushort>> OnBattleCmd(BattleCmdState state)
        {
            return new(BattleAction.ToEndPhase, 0);
        }

        public static async Task<Tuple<MainAction, ushort>> OnIdleCmd(IdleCmdState state)
        {
            return new(MainAction.ToEndPhase, 0);
        }

        public static async Task<int> OnSelectChain(SelectChainState state)
        {
            return -1;
        }

        public static async Task<IList<byte>?> OnSelectCard(SelectCardState state)
        {
            if (state.CurrentStateData.Cancelable) return null;
            byte[] selection = new byte[state.CurrentStateData.Minimum];
            for (int i = 0; i < selection.Length; i++)
                selection[i] = (byte)i;
            return selection;
        }
    }
}
