using OmegaBotWrapper;
using OmegaBotWrapper.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTest.Game.Field
{
    public partial class GameField
    {
        private readonly DuelBotWrapper _botWrapper;
        private FieldInfo _fieldInfo;

        public CardInfo OwnSide => _fieldInfo.OwnSide;

        public CardInfo EnemySide => _fieldInfo.EnemySide;

        public GameField(DuelBotWrapper botWrapper)
        {
            _botWrapper = botWrapper ?? throw new ArgumentNullException(nameof(botWrapper));
        }
    }
}
