using BotTest.Decks;
using BotTest.Game.AI.Bots.MasterDuel.SwordSoul;
using BotTest.Game.AI.Bots.SpeedDuel.SuperDuck;
using BotTest.Game.Base;
using BotTest.Utils;
using OmegaBotWrapper;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.States;

namespace BotTest.Game.AI.Bots.SpeedDuel.MachineAngelAscension
{
    public class MachineAngelAscensionActions : RuleBasedBotActions
    {
        public MachineAngelAscensionActions(SpeedDuelDeck deck = SpeedDuelDeck.MachineAngelAscension)
        {
            DeckName = DeckManager.GetDeckName(deck);
            DeckHash = DeckManager.GetDeckHash(deck);
            InitializeCardInfo<MachineAngelAscensionCardId>();

            BotAction(
                action: MainAction.Activate,
                cardId: MachineAngelAscensionCardId.GalaxyCyclone
            );

            BotAction(
                action: MainAction.Activate,
                cardId: MachineAngelAscensionCardId.NightBeam
            );

            BotAction(
                action: MainAction.Activate,
                cardId: MachineAngelAscensionCardId.MachineAngelRitual
            );

            BotAction(
                action: MainAction.Activate,
                cardId: MachineAngelAscensionCardId.MysticalSpaceTyphoon
            );

            BotAction(
                action: MainAction.Activate,
                cardId: MachineAngelAscensionCardId.MachineAngelAscension
            );

            BotAction(
                action: MainAction.Summon,
                cardId: MachineAngelAscensionCardId.SenjuOfTheThousandHands
            );

            BotAction(
                action: MainAction.Summon,
                cardId: MachineAngelAscensionCardId.SonicBird
            );
        }

        public override async Task<bool> OnSelectEffectYn(SelectEffectYnData state)
        {
            return true;
        }

        public override async Task<int> OnSelectChain(SelectChainData state)
        {
            foreach (var cards in state.SelectableCards)
            {
                ConsoleUtils.PrintColored($"[OnSelectChain] [Selectable cards] => {cards.ID}");
            }

            foreach (var cards in state.Descriptions)
            {
                ConsoleUtils.PrintColored($"[Descriptions cards] => {cards}");
            }

            if (state.SelectableCards.TryGetByID(MachineAngelAscensionCardId.CyberAngelIdaten, out _, out var index))
            {
                return index;
            }

            if (state.SelectableCards.TryGetByID(MachineAngelAscensionCardId.CyberAngelBenten, out _, out index))
            {
                return index;
            }

            return await base.OnSelectChain(state);
        }

        public override async Task<IList<byte>?> OnSelectCard(SelectCardData state)
        {
            ConsoleUtils.PrintColored("[override][OnSelectCard]" + (DHint)state.SelectionHint);

            if (state.SelectionHint.Equals((int)DHint.SPSUMMON))
            {
                //Check if Cyber Angel Izana is available in the hand
                bool hasCyberAngelIzana = _gameField.HasCardInHand(MachineAngelAscensionCardId.CyberAngelIzana).Result;

                if (hasCyberAngelIzana)
                {
                    var cyberAngelIzana = state.SelectableCards.Select((card, index) => new { Card = card, Index = index })
                                                        .Where(x => x.Card.ID == MachineAngelAscensionCardId.CyberAngelIzana)
                                                        .Take(state.Minimum)
                                                        .Select(x => (byte)x.Index).ToArray();

                    if (cyberAngelIzana.Any())
                        return cyberAngelIzana;
                }

                var botCards = state.SelectableCards
                                    .Select((card, index) => new { Card = card, Index = index })
                                    .OrderByDescending(x => x.Card.Attack)
                                    .Take(state.Minimum)
                                    .Select(x => (byte)x.Index)
                                    .ToArray();
                
                if (botCards.Any())
                    return botCards;
            }

            return await base.OnSelectCard(state);
        }
    }
}
