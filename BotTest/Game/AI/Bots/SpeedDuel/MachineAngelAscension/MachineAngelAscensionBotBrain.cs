using BotTest.Game.AI.Brains;
using BotTest.Utils;
using OmegaBotWrapper;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.Heuristics;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;

namespace BotTest.Game.AI.Bots.SpeedDuel.MachineAngelAscension
{
    public sealed class MachineAngelAscensionBotBrain : DeckBotBrain
    {
        public MachineAngelAscensionBotBrain(
            ICardEvaluator? cardEvaluator = null,
            IBoardEvaluator? boardEvaluator = null,
            IActionScorer? actionScorer = null)
            : base(cardEvaluator, boardEvaluator, actionScorer)
        {
        }

        public override async Task<IdleCmdResponse> OnIdleCmdAsync(DecisionContext<IdleCmdData> context, CancellationToken ct = default)
        {
            await PrepareContextAsync(context, ct);

            if (TryGetActivable(context.StateData, MachineAngelAscensionCardId.GalaxyCyclone, out var galaxyCyclone))
                return galaxyCyclone.MainActivateFromIndex();

            if (TryGetActivable(context.StateData, MachineAngelAscensionCardId.NightBeam, out var nightBeam))
                return nightBeam.MainActivateFromIndex();

            if (TryGetActivable(context.StateData, MachineAngelAscensionCardId.MachineAngelRitual, out var ritual))
                return ritual.MainActivateFromIndex();

            if (TryGetActivable(context.StateData, MachineAngelAscensionCardId.MysticalSpaceTyphoon, out var mst))
                return mst.MainActivateFromIndex();

            if (TryGetActivable(context.StateData, MachineAngelAscensionCardId.MachineAngelAscension, out var ascension))
                return ascension.MainActivateFromIndex();

            if (TryGetSummonable(context.StateData, MachineAngelAscensionCardId.SenjuOfTheThousandHands, out var senju))
                return senju.NormalSummon();

            if (TryGetSummonable(context.StateData, MachineAngelAscensionCardId.SonicBird, out var sonicBird))
                return sonicBird.NormalSummon();

            return await base.OnIdleCmdAsync(context, ct);
        }

        public override Task<bool> OnSelectEffectYnAsync(DecisionContext<SelectEffectYnData> context, CancellationToken ct = default) =>
            Task.FromResult(true);

        public override async Task<int> OnSelectChainAsync(DecisionContext<SelectChainData> context, CancellationToken ct = default)
        {
            await PrepareContextAsync(context, ct);

            foreach (var cards in context.StateData.SelectableCards ?? Array.Empty<DynamicCard>())
                ConsoleUtils.PrintColored($"[OnSelectChain] [Selectable cards] => {cards.ID}");

            if ((context.StateData.SelectableCards ?? Array.Empty<DynamicCard>()).TryGetByID(MachineAngelAscensionCardId.CyberAngelIdaten, out _, out var index))
                return index;

            if ((context.StateData.SelectableCards ?? Array.Empty<DynamicCard>()).TryGetByID(MachineAngelAscensionCardId.CyberAngelBenten, out _, out index))
                return index;

            return await base.OnSelectChainAsync(context, ct);
        }

        public override async Task<IList<byte>?> OnSelectCardAsync(DecisionContext<SelectCardData> context, CancellationToken ct = default)
        {
            await PrepareContextAsync(context, ct);
            ConsoleUtils.PrintColored("[override][OnSelectCard]" + (DHint)context.StateData.SelectionHint);

            if (context.StateData.SelectionHint.Equals((int)DHint.SPSUMMON))
            {
                bool hasCyberAngelIzana = (await GetBotHandCardsAsync()).Any(card => card.ID == MachineAngelAscensionCardId.CyberAngelIzana);

                if (hasCyberAngelIzana)
                {
                    var cyberAngelIzana = context.StateData.SelectableCards.Select((card, index) => new { Card = card, Index = index })
                        .Where(x => x.Card.ID == MachineAngelAscensionCardId.CyberAngelIzana)
                        .Take(context.StateData.Minimum)
                        .Select(x => (byte)x.Index)
                        .ToArray();

                    if (cyberAngelIzana.Any())
                        return cyberAngelIzana;
                }

                var botCards = context.StateData.SelectableCards
                    .Select((card, index) => new { Card = card, Index = index })
                    .OrderByDescending(x => x.Card.Attack)
                    .Take(context.StateData.Minimum)
                    .Select(x => (byte)x.Index)
                    .ToArray();

                if (botCards.Any())
                    return botCards;
            }

            return await base.OnSelectCardAsync(context, ct);
        }
    }
}
