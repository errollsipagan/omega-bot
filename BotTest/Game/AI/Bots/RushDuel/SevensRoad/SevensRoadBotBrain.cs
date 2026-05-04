using OmegaBotWrapper.Heuristics;

namespace BotTest.Game.AI.Bots.RushDuel.SevensRoad
{
    public sealed class SevensRoadBotBrain : BotTest.Game.AI.Brains.DeckBotBrain
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
