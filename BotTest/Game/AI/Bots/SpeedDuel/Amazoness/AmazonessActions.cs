using BotTest.Decks;
using BotTest.Game.AI.Bots.SpeedDuel.SuperDuck;
using BotTest.Game.Base;
using OmegaBotWrapper;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;
using System.Diagnostics;

namespace BotTest.Game.AI.Bots.SpeedDuel.Amazoness
{
    public class AmazonessActions : RuleBasedBotActions
    {
        List<DBCardInfo> mindCrushCards = new();

        public AmazonessActions(SpeedDuelDeck deck = SpeedDuelDeck.Amazoness)
        {
            DeckName = DeckManager.GetDeckName(deck);
            DeckHash = DeckManager.GetDeckHash(deck);
            InitializeCardInfo<AmazonessCardId>();

            BotAction(
                action: MainAction.Activate,
                cardId: AmazonessCardId.BattleCityBestie
            );

            BotAction(
                action: MainAction.Activate,
                cardId: AmazonessCardId.CosmicCyclone,
                condition: destroyOrRemoveSpellTrapCondition
            );

            BotAction(
                action: MainAction.Activate,
                cardId: AmazonessCardId.NightBeam,
                condition: destroyOrRemoveSpellTrapCondition
            );

            BotAction(
                action: MainAction.Activate,
                cardId: AmazonessCardId.MysticalSpaceTyphoon,
                condition: destroyOrRemoveSpellTrapCondition
            );

            BotAction(
                action: MainAction.SetSpell,
                cardId: AmazonessCardId.AmazonessOnslaught
            );

            BotAction(
                action: MainAction.Activate,
                cardId: AmazonessCardId.AmazonessOnslaught
            );

            BotAction(
                action: MainAction.SetSpell,
                cardId: AmazonessCardId.MindCrush
            );

            BotAction(
                action: MainAction.Activate,
                cardId: AmazonessCardId.MindCrush
            );

            BotAction(
                action: MainAction.Activate,
                cardId: AmazonessCardId.ReinforcementOfTheArmy
            );

            BotAction(
                action: MainAction.Activate,
                cardId: AmazonessCardId.EnemyController
            );

            BotAction(
                action: MainAction.Activate,
                cardId: AmazonessCardId.AmazonessCall
            );

            BotAction(
                action: MainAction.Summon,
                cardId: AmazonessCardId.AmazonessPrincess
            );

            BotAction(
                action: MainAction.Summon,
                cardId: AmazonessCardId.AmazonessQueen
            );

            //Reposable
            BotAction(
                action: MainAction.Repos,
                cardId: AmazonessCardId.AmazonessQueen,
                condition: ReposableCondition
            );
        }

        public override async Task<bool> OnSelectEffectYn(SelectEffectYnData state)
        {
            //Amazoness Princess
            bool battleStartPhase = GetCurrentDuelPhase() == DuelPhase.BattleStart;

            if (state.EffectCard.IsCode(AmazonessCardId.AmazonessPrincess) && !battleStartPhase)
                return true;

            IEnumerable<DynamicCard> botMonster = await _gameField.GetMonsterInLocation(location: CardLocation.MonsterZone, controller: 0);
            IEnumerable<DynamicCard> botSpellTrap = await _gameField.GetCardsInSpellTrapZone();
            IEnumerable<DynamicCard> cardHand = await _gameField.GetCardsInHand();

            bool hasAmazonessPrincess = botMonster.Where(card => card.ID == AmazonessCardId.AmazonessPrincess).ToArray().Length >= 2;
            bool hasOneAmazonessQueen = botMonster.Where(card => card.ID == AmazonessCardId.AmazonessQueen).ToArray().Length == 1;
            bool hasTwoAmazonessQueen = botMonster.Where(card => card.ID == AmazonessCardId.AmazonessQueen).ToArray().Length == 2;
            cardHand = cardHand.Where(card => card.ID != AmazonessCardId.AmazonessQueen).ToArray();
            bool hasCardInHand = cardHand?.Count() > 0;
            botSpellTrap = botSpellTrap.Where(card => card.ID != AmazonessCardId.AmazonessOnslaught).ToArray();
            bool hasSpellTrap = botSpellTrap?.Count() > 0;

            Console.WriteLine($">>>>> [OnSelectEffectYn][override] => {state.EffectCard.ID}");
            Console.WriteLine($">>>>> hasAmazonessPrincess => {hasAmazonessPrincess}");
            Console.WriteLine($">>>>> hasOneAmazonessQueen => {hasOneAmazonessQueen}");
            Console.WriteLine($">>>>> hasTwoAmazonessQueen => {hasTwoAmazonessQueen}");
            Console.WriteLine($">>>>> hasCardInHand => {hasCardInHand}");
            Console.WriteLine($">>>>> hasSpellTrap => {hasSpellTrap}");
            Console.WriteLine($">>>>> OnSelectEffectYn Amazoness Princess Condition => {hasAmazonessPrincess || hasCardInHand || hasSpellTrap}");

            if (state.EffectCard.IsCode(AmazonessCardId.AmazonessPrincess) && (hasAmazonessPrincess || (hasCardInHand && hasOneAmazonessQueen) || hasSpellTrap || !hasTwoAmazonessQueen))
                return true;

            return await base.OnSelectEffectYn(state);
        }

        public override async Task<int> OnSelectChain(SelectChainData state)
        {
            foreach (var cards in state.SelectableCards)
            {
                Console.WriteLine($"[OnSelectChain] [override] [Selectable cards] => {cards.ID}");
            }

            foreach (var cards in state.Descriptions)
            {
                Console.WriteLine($"[Descriptions cards] => {cards}");
            }

            if (state.SelectableCards.TryGetByID(AmazonessCardId.AmazonessOnslaught, out _, out var index))
                return index;

            return await base.OnSelectChain(state);
        }

        public override async Task<BattleCmdResponse> OnBattleCmd(BattleCmdData state)
        {
            if (state.AttackableCards.Any())
                return state.AttackableCards.First().DeclareAttack();
            if (state.CanMainPhaseTwo)
                return BattleAction.ToMainPhaseTwo;

            return await base.OnBattleCmd(state);
        }

        private bool ReposableCondition(IdleCmdData state, DynamicCard card)
        {
            bool isFaceUpDefense = card.IsFaceup() && card.IsDefense();
            return isFaceUpDefense;
        }

        private bool destroyOrRemoveSpellTrapCondition(IdleCmdData state, DynamicCard card)
        {
            int playerSpellsCount = _gameField.GetPlayerSpellsCount().Result;
            bool playerHasSpells = playerSpellsCount > 0;
            return playerHasSpells;
        }

        public override async Task<IList<byte>?> OnSelectCard(SelectCardData state)
        {

            foreach (var cards in state.SelectableCards)
            {
                Console.WriteLine($"[OnSelectCard] [override] [SelectableCards cards] => {cards.ID}");
            }
            Console.WriteLine($"[OnSelectCard] [override] [Minimum] => {state.Minimum}");
            Console.WriteLine($"[OnSelectCard] [override] [Maximum] => {state.Maximum}");
            Console.WriteLine($"[OnSelectCard] [override] [SelectionHint] => {(DHint)state.SelectionHint}");
            Console.WriteLine($"[OnSelectCard] [override] [Cancelable] => {state.Cancelable}");
            Console.WriteLine($"[OnSelectCard] [override] [SelectableCount] => {state.SelectableCount}");

            if (state.SelectionHint.Equals((int)DHint.TOGRAVE))
            {
                Console.WriteLine("[override][OnSelectCard] - Selecting Card to TOGRAVE");

                // Create a list of cards with their original indices
                var cardsWithIndices = state.SelectableCards
                    .Select((card, index) => new { Card = card, OriginalIndex = index })
                    .ToArray();

                // Apply filters while preserving original indices
                var filteredCards = cardsWithIndices
                    .Where(x => x.Card.ID != AmazonessCardId.AmazonessQueen)
                    .Where(x => x.Card.ID != AmazonessCardId.AmazonessOnslaught)
                    .ToArray();

                // Select the first 'state.Minimum' cards, using original indices
                var botCards = filteredCards
                    .Take(state.Minimum)
                    .Select(x => (byte)x.OriginalIndex)
                    .ToArray();

                // Respect Minimum (or Maximum if needed)
                if (botCards.Any())
                    return botCards;

            }

            return await base.OnSelectCard(state);
        }

        public override async Task<int> OnAnnounceCard(AnnounceCardData state)
        {
            //mindCrushCards.Add(new DBCardInfo
            //{
            //    ID = 89631139
            //});

            //if (mindCrushCards.Any())
            //{
            //    return mindCrushCards.FirstOrDefault().ID;
            //}

            return 89631139;

            //return await base.OnAnnounceCard(state);
        }
    }
}
