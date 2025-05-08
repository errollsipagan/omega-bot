using BotTest.Decks;
using BotTest.Game.Base;
using BotTest.Utils;
using OmegaBotWrapper;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;

namespace BotTest.Game.AI.Bots.SpeedDuel.Amazoness
{
    public class AmazonessActions : RuleBasedBotActions
    {
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
                condition: DestroyOrRemoveSpellTrapCondition
            );

            BotAction(
                action: MainAction.Activate,
                cardId: AmazonessCardId.NightBeam,
                condition: DestroyOrRemoveSpellTrapCondition
            );

            BotAction(
                action: MainAction.Activate,
                cardId: AmazonessCardId.MysticalSpaceTyphoon,
                condition: DestroyOrRemoveSpellTrapCondition
            );

            BotAction(
                action: MainAction.SetSpell,
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
                action: MainAction.SetSpell,
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

            ConsoleUtils.PrintColored($">>>>> [OnSelectEffectYn][override] => {state.EffectCard.ID}");
            ConsoleUtils.PrintColored($">>>>> hasAmazonessPrincess => {hasAmazonessPrincess}");
            ConsoleUtils.PrintColored($">>>>> hasOneAmazonessQueen => {hasOneAmazonessQueen}");
            ConsoleUtils.PrintColored($">>>>> hasTwoAmazonessQueen => {hasTwoAmazonessQueen}");
            ConsoleUtils.PrintColored($">>>>> hasCardInHand => {hasCardInHand}");
            ConsoleUtils.PrintColored($">>>>> hasSpellTrap => {hasSpellTrap}");
            ConsoleUtils.PrintColored($">>>>> OnSelectEffectYn Amazoness Princess Condition => {hasAmazonessPrincess || hasCardInHand || hasSpellTrap}");

            if (state.EffectCard.IsCode(AmazonessCardId.AmazonessPrincess) && (hasAmazonessPrincess || (hasCardInHand && hasOneAmazonessQueen) || hasSpellTrap || !hasTwoAmazonessQueen))
                return true;

            return await base.OnSelectEffectYn(state);
        }

        public override async Task<int> OnSelectChain(SelectChainData state)
        {
            foreach (var cards in state.SelectableCards)
            {
                ConsoleUtils.PrintColored($"[OnSelectChain] [override] [Selectable cards] => {cards.ID}");
            }

            foreach (var cards in state.Descriptions)
            {
                ConsoleUtils.PrintColored($"[Descriptions cards] => {cards}");
            }

            //get zone (sequence) targeted for destroy/removal Test
            IEnumerable<DynamicCard> spellsTrapsCards = _gameField.GetPlayerSpellsCardsInSpellZone().Result;
            var cardsActivated = spellsTrapsCards.Where(card => card.IsFaceup());

            if (cardsActivated.Any())
            {
                foreach (var card in cardsActivated)
                {
                    if (card.CurrentChainTargets.Length > 0)
                    {
                        int sequenceTarget = card.CurrentChainTargets[0].Sequence;

                        if (state.SelectableCards.TryGetByIDWithZone(AmazonessCardId.AmazonessCall, sequenceTarget, out _, out var indexx) && AmazonessCallCondition())
                            return indexx;
                    }
                }
            }

            if (cardsActivated.Any())
            {
                foreach (var card in cardsActivated)
                {
                    if (card.CurrentChainTargets.Length > 0)
                    {
                        int sequenceTarget = card.CurrentChainTargets[0].Sequence;

                        if (state.SelectableCards.TryGetByIDWithZone(AmazonessCardId.AmazonessOnslaught, sequenceTarget, out _, out var indexx) && AmazonessOnslaughtCondition())
                            return indexx;
                    }
                }
            }

            if (state.SelectableCards.TryGetByID(AmazonessCardId.AmazonessCall, out _, out var index) &&
                AmazonessCallCondition())
                return index;

            if (state.SelectableCards.TryGetByID(AmazonessCardId.AmazonessOnslaught, out _, out index) &&
                AmazonessOnslaughtCondition())
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

        public override async Task<IList<byte>?> OnSelectCard(SelectCardData state)
        {

            foreach (var cards in state.SelectableCards)
            {
                ConsoleUtils.PrintColored($"[OnSelectCard] [override] [SelectableCards cards] => {cards.ID}");
            }
            ConsoleUtils.PrintColored($"[OnSelectCard] [override] [Minimum] => {state.Minimum}");
            ConsoleUtils.PrintColored($"[OnSelectCard] [override] [Maximum] => {state.Maximum}");
            ConsoleUtils.PrintColored($"[OnSelectCard] [override] [SelectionHint] => {(DHint)state.SelectionHint}");
            ConsoleUtils.PrintColored($"[OnSelectCard] [override] [Cancelable] => {state.Cancelable}");
            ConsoleUtils.PrintColored($"[OnSelectCard] [override] [SelectableCount] => {state.SelectableCount}");

            if (state.SelectionHint.Equals((int)DHint.TOGRAVE))
            {
                ConsoleUtils.PrintColored("[override][OnSelectCard] - Selecting Card to TOGRAVE");

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
            ConsoleUtils.PrintColored($"OnAnnounceCard called!");

            //cheater
            IEnumerable<DynamicCard> playerCards = await _gameField.GetPlayerCardsInHand();

            if (playerCards.Any())
            {
                Random rng = new();
                return playerCards.ElementAt(rng.Next(0, playerCards.Count())).Alias;
            }

            return await base.OnAnnounceCard(state);
        }

        private bool ReposableCondition(IdleCmdData state, DynamicCard card)
        {
            bool isFaceUpDefense = card.IsFaceup() && card.IsDefense();
            return isFaceUpDefense;
        }

        private bool DestroyOrRemoveSpellTrapCondition(IdleCmdData state, DynamicCard card)
        {
            int playerSpellsCount = _gameField.GetPlayerSpellsCount().Result;
            bool playerHasSpells = playerSpellsCount > 0;
            return playerHasSpells;
        }

        private bool AmazonessOnslaughtCondition()
        {
            //targeted for destroy/removal Test
            IEnumerable<DynamicCard> spellsTrapsCards = _gameField.GetPlayerSpellsCardsInSpellZone().Result;
            var cardsActivated = spellsTrapsCards.Where(card => card.IsFaceup());

            if (cardsActivated.Any())
            {
                foreach (var card in cardsActivated)
                {
                    if (card.CurrentChainTargets.Length > 0)
                    {
                        var getBotCardTargets = _gameField.GetBotFieldCard(
                            controller: 0,
                            location: card.CurrentChainTargets[0].Location,
                            sequence: card.CurrentChainTargets[0].Sequence
                            ).Result;

                        bool amazonessOnslaughtTargets = getBotCardTargets.IsCode(AmazonessCardId.AmazonessOnslaught) && getBotCardTargets.IsFacedown();

                        if (amazonessOnslaughtTargets)
                            return true;
                    }
                }
            }

            bool battleStartPhase = GetCurrentDuelPhase() == DuelPhase.BattleStart;
            return battleStartPhase;
        }

        private bool AmazonessCallCondition()
        {
            //targeted for destroy/removal Test
            IEnumerable<DynamicCard> spellsTrapsCards = _gameField.GetPlayerSpellsCardsInSpellZone().Result;
            var cardsActivated = spellsTrapsCards.Where(card => card.IsFaceup());

            if (cardsActivated.Any())
            {
                foreach (var card in cardsActivated)
                {
                    if (card.CurrentChainTargets.Length > 0)
                    {
                        var getBotCardTargets = _gameField.GetBotFieldCard(
                            controller: 0,
                            location: card.CurrentChainTargets[0].Location,
                            sequence: card.CurrentChainTargets[0].Sequence
                            ).Result;

                        bool amazonessCallTargets = getBotCardTargets.IsCode(AmazonessCardId.AmazonessCall) && getBotCardTargets.IsFacedown();

                        if (amazonessCallTargets)
                            return true;
                    }
                }
            }

            bool battleStartPhase = GetCurrentDuelPhase() == DuelPhase.BattleStart;
            return battleStartPhase;
        }
    }
}
