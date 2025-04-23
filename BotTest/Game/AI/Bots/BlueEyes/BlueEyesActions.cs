using BotTest.Game.Base;
using BotTest.Game.Field;
using OmegaBotWrapper;

namespace BotTest.Game.AI.Bots.BlueEyes
{
    public class BlueEyesActions : BaseBotActions
    {
        private GameField _gameField;

        public BlueEyesActions()
        {
            _gameField = null!;
        }

        /// <summary>
        /// Registers all the actions the bot should take during specific states
        /// </summary>
        /// <param name="bot">Bot instance which actions are being registered to</param>
        private void RegisterActions(DuelBotWrapper bot)
        {
            _gameField = new GameField(bot);
        }
    }
}
