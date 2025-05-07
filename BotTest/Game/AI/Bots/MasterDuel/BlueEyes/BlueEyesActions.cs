using BotTest.Decks;
using BotTest.Game.Base;
using BotTest.Game.Field;
using BotTest.Utils;
using OmegaBotWrapper;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;

namespace BotTest.Game.AI.Bots.MasterDuel.BlueEyes
{
    public class BlueEyesActions : RuleBasedBotActions
    {
        public BlueEyesActions(MasterDuelDeck deck = MasterDuelDeck.BlueEyes)
        {
            DeckName = DeckManager.GetDeckName(deck);
            DeckHash = DeckManager.GetDeckHash(deck);
            InitializeCardInfo<BlueEyesCardId>();

            //Normal Summons
            //Add DragonSpiritOfWhite summon rule with custom condition
            BotAction(
                action: MainAction.Summon,
                cardId: BlueEyesCardId.DragonSpiritOfWhite,
                condition: DragonSpiritOfWhiteSummon
            );

            BotAction(
                action: MainAction.Summon,
                cardId: BlueEyesCardId.MaidenOfEyesOfBlue,
                condition: (s, c) => s.SummonableCards.Any()
            );

            BotAction(
                action: MainAction.Summon,
                cardId: BlueEyesCardId.MaidenOfWhite,
                condition: (s, c) => s.SummonableCards.Any()
            );

            BotAction(
                action: MainAction.Summon,
                cardId: BlueEyesCardId.BlueEyesWhiteDragon,
                condition: (s, c) => s.SummonableCards.Any()
            );

            //Set Monsters
            BotAction(
                action: MainAction.SetMonster,
                cardId: BlueEyesCardId.TheWhiteStoneOfAncients
            );

            //Reposable Monsters
            BotAction(
                action: MainAction.Repos,
                cardId: BlueEyesCardId.TheWhiteStoneOfAncients
            );

            //Special Summons
            BotAction(
                action: MainAction.SpSummon,
                cardId: BlueEyesCardId.BlueEyesAlternativeWhiteDragon
            );

            BotAction(
                action: MainAction.SpSummon,
                cardId: BlueEyesCardId.BlueEyesSpiritDragon,
                condition: (s, c) => s.SpecialSummonableCards.Any()
            );

            BotAction(
                action: MainAction.SpSummon,
                cardId: BlueEyesCardId.HieraticSunDragonOverlordOfHeliopolis
            );

            BotAction(
                action: MainAction.SpSummon,
                cardId: BlueEyesCardId.Number46Dragluon
            );

            //Activable cards
            BotAction(
                action: MainAction.Activate,
                cardId: BlueEyesCardId.TheWhiteStoneOfAncients,
                condition: (s, c) => s.ActivableCards.Any()
            );

            BotAction(
                action: MainAction.Activate,
                cardId: BlueEyesCardId.MaidenOfWhite,
                condition: (s, c) => s.ActivableCards.Any()
            );

            BotAction(
                action: MainAction.Activate,
                cardId: BlueEyesCardId.MaidenOfEyesOfBlue,
                condition: (s, c) => s.ActivableCards.Any()
            );

            BotAction(
                action: MainAction.Activate,
                cardId: BlueEyesCardId.BingoMachineGo
            );
        }

        /// <summary>
        /// Overrides the base OnSelectEffectYn to handle specific card effects.
        /// </summary>
        public override async Task<bool> OnSelectEffectYn(SelectEffectYnData state)
        {
            //Check if the card is Dragon Spirit Of White
            if (state.EffectCard.IsCode(BlueEyesCardId.DragonSpiritOfWhite))
            {
                ConsoleUtils.PrintColored("Activating effect for Dragon Spirit of White");
                return true;
            }

            //Fallback to base implementation for other cards
            return await base.OnSelectEffectYn(state);
        }

        /// <summary>
        /// Condition to summon DragonSpiritOfWhite: checks if there are cards in the SpellZone and if the card is summonable.
        /// </summary>
        private bool DragonSpiritOfWhiteSummon(IdleCmdData state, DynamicCard card)
        {
            //Checks for cards in player's SpellZones
            int playerSpellZoneCount = _gameField.GetPlayerSpellZoneCount().Result;
            bool hasSpellCards = playerSpellZoneCount > 0;

            //Checks for summonable cards
            bool canSummon = state.SummonableCards.Any(c => c.ID == BlueEyesCardId.DragonSpiritOfWhite);

            if (hasSpellCards && canSummon)
                ConsoleUtils.PrintColored($"[[[[BINGO!]]]] DragonSpiritOfWhiteSummon: Can summon (SpellZoneCount: {playerSpellZoneCount})");
            return hasSpellCards && canSummon;
        }
    }
}
