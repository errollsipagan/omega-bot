using BotTest.Game.AI.Brains;
using BotTest.Utils;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.Heuristics;
using OmegaBotWrapper.States;

namespace BotTest.Game.AI.Bots.MasterDuel.HandTraps
{
    public sealed class HandTrapsBotBrain : DeckBotBrain
    {
        public HandTrapsBotBrain(
            ICardEvaluator? cardEvaluator = null,
            IBoardEvaluator? boardEvaluator = null,
            IActionScorer? actionScorer = null)
            : base(cardEvaluator, boardEvaluator, actionScorer)
        {
        }

        public override async Task<int> OnSelectChainAsync(DecisionContext<SelectChainData> context, CancellationToken ct = default)
        {
            await PrepareContextAsync(context, ct);
            ConsoleUtils.PrintColored($"Current Duel Phase: {(DuelPhase)context.Header.CurrentPhase}");

            foreach (var cards in context.StateData.SelectableCards ?? Array.Empty<OmegaBotWrapper.Struct.DynamicCard>())
                ConsoleUtils.PrintColored($"[OnSelectChain] [override] [Selectable cards] => {cards.ID}");

            if (!context.StateData.Forced)
                return 0;

            return await base.OnSelectChainAsync(context, ct);
        }
    }
}
