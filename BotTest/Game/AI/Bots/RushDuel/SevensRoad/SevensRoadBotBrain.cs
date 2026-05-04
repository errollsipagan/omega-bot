using BotTest.Game.AI.Brains;
using OmegaBotWrapper.Heuristics;

namespace BotTest.Game.AI.Bots.RushDuel.SevensRoad
{
    public sealed class SevensRoadBotBrain : DeckBotBrain
    {
        public SevensRoadBotBrain(
            ICardEvaluator? cardEvaluator = null,
            IBoardEvaluator? boardEvaluator = null,
            IActionScorer? actionScorer = null)
            : base(cardEvaluator, boardEvaluator, actionScorer)
        {
        }
    }
}
