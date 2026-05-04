using OmegaBotWrapper.Heuristics;

namespace BotTest.Game.AI.Bots.MasterDuel.DarkMagician
{
    public sealed class DarkMagicianBotBrain : BotTest.Game.AI.Brains.DeckBotBrain
    {
        public DarkMagicianBotBrain(
            ICardEvaluator? cardEvaluator = null,
            IBoardEvaluator? boardEvaluator = null,
            IActionScorer? actionScorer = null)
            : base(cardEvaluator, boardEvaluator, actionScorer)
        {
        }
    }
}
