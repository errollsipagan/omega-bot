using BotTest.Game.AI.Brains;
using OmegaBotWrapper.Heuristics;

namespace BotTest.Game.AI.Bots.MasterDuel.KaibaAntiMeta;

public sealed class KaibaAntiMetaBotBrain : KaibaBotBrain
{

    public KaibaAntiMetaBotBrain(
        ICardEvaluator? cardEvaluator = null,
        IBoardEvaluator? boardEvaluator = null,
        IActionScorer? actionScorer = null)
        : base(cardEvaluator, boardEvaluator, actionScorer)
    {
    }
}
