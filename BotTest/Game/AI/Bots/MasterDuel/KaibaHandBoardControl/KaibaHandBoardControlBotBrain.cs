using BotTest.Game.AI.Brains;
using OmegaBotWrapper.Heuristics;

namespace BotTest.Game.AI.Bots.MasterDuel.KaibaHandBoardControl;

public sealed class KaibaHandBoardControlBotBrain : KaibaBotBrain
{

    public KaibaHandBoardControlBotBrain(
        ICardEvaluator? cardEvaluator = null,
        IBoardEvaluator? boardEvaluator = null,
        IActionScorer? actionScorer = null)
        : base(cardEvaluator, boardEvaluator, actionScorer)
    {
    }
}
