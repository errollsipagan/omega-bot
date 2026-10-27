using BotTest.Game.AI.Brains;
using OmegaBotWrapper.Heuristics;

namespace BotTest.Game.AI.Bots.MasterDuel.KaibaGunBarrelDragon;

public sealed class KaibaGunBarrelDragonBotBrain : KaibaBotBrain
{

    public KaibaGunBarrelDragonBotBrain(
        ICardEvaluator? cardEvaluator = null,
        IBoardEvaluator? boardEvaluator = null,
        IActionScorer? actionScorer = null)
        : base(cardEvaluator, boardEvaluator, actionScorer)
    {
    }
}
