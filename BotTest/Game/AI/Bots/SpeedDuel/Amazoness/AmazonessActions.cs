using BotTest.Decks;
using BotTest.Game.AI.Bots.MasterDuel.SwordSoul;
using BotTest.Game.AI.Bots.TestOnly;
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

        bool sendToGrave = false;

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

            if (state.EffectCard.IsCode(AmazonessCardId.AmazonessOnslaught))
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

            //Player will be destroy all Spells and Traps cards
            bool playerWillBeDestroyAllSpellsAndTraps = _gameField.PlayerDestroyAllSpellAndTraps();
            if (playerWillBeDestroyAllSpellsAndTraps)
                return 0;

            //Player will be destroy all cards
            bool playerWillBeDestroyAllCards = _gameField.PlayerDestroyAllSpellAndTraps();
            if (playerWillBeDestroyAllCards)
                return 0;

            //get zone (sequence) targeted for destroy/removal Test
            IEnumerable<DynamicCard> spellsTrapsCards = _gameField.GetPlayerSpellsCardsInSpellZone().Result;
            var cardsActivated = spellsTrapsCards.Where(card => card.IsFaceup());

            if (cardsActivated.Any())
            {
                foreach (var card in cardsActivated)
                {
                    if (card.CurrentChainTargets.Length > 0)
                    {
                        int controllerTarget = card.CurrentChainTargets[0].Controller;
                        if (controllerTarget != 0) //IS BOT
                        {
                            int sequenceTarget = card.CurrentChainTargets[0].Sequence;
                            if (state.SelectableCards.TryGetByIDWithZone(AmazonessCardId.AmazonessCall, sequenceTarget, out _, out var indexx) && AmazonessCallCondition())
                                return indexx;
                        }
                    }
                }
            }

            if (cardsActivated.Any())
            {
                foreach (var card in cardsActivated)
                {
                    if (card.CurrentChainTargets.Length > 0)
                    {
                        int controllerTarget = card.CurrentChainTargets[0].Controller;
                        if (controllerTarget != 0) //IS BOT
                        {
                            int sequenceTarget = card.CurrentChainTargets[0].Sequence;

                            if (state.SelectableCards.TryGetByIDWithZone(AmazonessCardId.AmazonessOnslaught, sequenceTarget, out _, out var indexx) && AmazonessOnslaughtCondition())
                                return indexx;
                        }
                    }
                }
            }

            if (state.SelectableCards.TryGetByID(AmazonessCardId.AmazonessCall, out _, out var index) &&
                AmazonessCallCondition())
                return index;

            if (state.SelectableCards.TryGetByID(AmazonessCardId.AmazonessOnslaught, out _, out index) &&
                AmazonessOnslaughtCondition())
                return index;

            if (state.SelectableCards.TryGetByID(AmazonessCardId.MindCrush, out _, out index))
                return index;

            return await base.OnSelectChain(state);
        }

        public override async Task<BattleCmdResponse> OnBattleCmd(BattleCmdData state)
        {
            if (state.AttackableCards.Any())
            {
                bool hasAmazonessOnslaught = _gameField.HasCardInBotSpellZone(AmazonessCardId.AmazonessOnslaught).Result;
                if (hasAmazonessOnslaught)
                {
                    return state.AttackableCards.First().DeclareAttack();
                }
            }

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

            if (state.SelectionHint.Equals((int)DHint.OPERATECARD))
            {
                sendToGrave = false;
                ConsoleUtils.PrintColored("[override][OnSelectCard] - Selecting Card to OPERATECARD");

                //if call targeted for removal...
                IEnumerable<DynamicCard> cardHand = await _gameField.GetCardsInHand();
                IEnumerable<DynamicCard> cardGrave = await _gameField.GetCardsInGrave();

                //also, I think bot needs prio for call:
                //no princess in hand or field or gy = add princess.
                //no cards in hand = add princess
                bool amazonessPrincessInMonsterZone = _gameField.HasCardInBotMonsterZone(AmazonessCardId.AmazonessPrincess).Result;
                var amazonessPrincessInGrave = cardGrave.Where(card => card.IsCode(AmazonessCardId.AmazonessPrincess));
                var amazonessPrincessInHand = cardHand.Where(card => card.IsCode(AmazonessCardId.AmazonessPrincess));

                if (!amazonessPrincessInHand.Any() && !amazonessPrincessInMonsterZone && !amazonessPrincessInGrave.Any())
                    return _gameField.FilterSelectableCardsById(state, AmazonessCardId.AmazonessPrincess);

                var amazonessCards = cardHand.Where(card => card.IsCode(AmazonessCardId.AmazonessCall) ||
                                                    card.IsCode(AmazonessCardId.AmazonessOnslaught) ||
                                                    card.IsCode(AmazonessCardId.AmazonessPrincess) ||
                                                    card.IsCode(AmazonessCardId.AmazonessQueen));

                //no amaz in hand - add amaz queen
                if (!amazonessCards.Any())
                    return _gameField.FilterSelectableCardsById(state, AmazonessCardId.AmazonessQueen);


                //if there is amaz queen in hand and no amaz in gy - send queen to gy
                var amazonessQueenInHand = cardHand.Where(card => card.IsCode(AmazonessCardId.AmazonessQueen));
                var amazonessCardsInGrave = cardGrave.Where(card => card.IsCode(AmazonessCardId.AmazonessCall) ||
                                                            card.IsCode(AmazonessCardId.AmazonessOnslaught) ||
                                                            card.IsCode(AmazonessCardId.AmazonessPrincess) ||
                                                            card.IsCode(AmazonessCardId.AmazonessQueen));

                if (amazonessQueenInHand.Any() && !amazonessCardsInGrave.Any())
                {
                    sendToGrave = true;
                    return _gameField.FilterSelectableCardsById(state, AmazonessCardId.AmazonessQueen);
                }

                //if amaz queen in hand and in gy - add princess
                var amazonessQueenInGrave = cardGrave.Where(card => card.IsCode(AmazonessCardId.AmazonessQueen));

                if (amazonessQueenInHand.Any() && amazonessQueenInGrave.Any())
                    return _gameField.FilterSelectableCardsById(state, AmazonessCardId.AmazonessPrincess);

                //if hand has princess and queen and there is queen in gy - add onslaught.
                if (amazonessQueenInHand.Any() && amazonessPrincessInHand.Any() && amazonessQueenInGrave.Any())
                    return _gameField.FilterSelectableCardsById(state, AmazonessCardId.AmazonessOnslaught);
            }

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

        public override async Task<int> OnSelectOption(SelectOptionData state)
        {
            ConsoleUtils.PrintColored("[OnSelectOption][override]");

            if (sendToGrave)
                return 1;

            return 0;
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
                        int controllerTarget = card.CurrentChainTargets[0].Controller;
                        if (controllerTarget != 0) //IS BOT
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
            }

            bool battleStartPhase = GetCurrentDuelPhase() == DuelPhase.BattleStart;
            return battleStartPhase;
        }

        private bool AmazonessCallCondition()
        {
            IEnumerable<DynamicCard> spellsTrapsCards = _gameField.GetPlayerSpellsCardsInSpellZone().Result;
            var cardsActivated = spellsTrapsCards.Where(card => card.IsFaceup());

            if (cardsActivated.Any())
            {
                foreach (var card in cardsActivated)
                {
                    if (card.CurrentChainTargets.Length > 0)
                    {
                        int controllerTarget = card.CurrentChainTargets[0].Controller;
                        if (controllerTarget != 0) //IS BOT
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
            }

            bool battleStartPhase = GetCurrentDuelPhase() == DuelPhase.BattleStart;
            return battleStartPhase;
        }
    }
}
