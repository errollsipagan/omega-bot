//using OmegaBotWrapper.Decks;
//using OmegaBotWrapper.Enum;
//using OmegaBotWrapper.Game.AI.Bots.BotTest;
//using OmegaBotWrapper.Game.Base;
//using OmegaBotWrapper.States;
//using OmegaBotWrapper.Struct;

//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace OmegaBotWrapper.Game.AI.Bots.BlueEyes
//{
//    public class BlueEyesTestActions : BaseBotActions
//    {
//        private readonly List<ActionRule> _actionRules;
//        private readonly Dictionary<int, Base.CardInfo> _cardInfoMap;

//        public BlueEyesTestActions(MasterDuelDeck deck = MasterDuelDeck.BlueEyes)
//        {
//            DeckName = DeckManager.GetDeckName(deck);
//            DeckHash = DeckManager.GetDeckHash(deck);

//            _actionRules = new List<ActionRule>();
//            _cardInfoMap = new Dictionary<int, Base.CardInfo>();

//            Initialize card info mapping
//            AddCardInfo(BlueEyesCardId.BlueEyesWhiteDragon, "Blue Eyes White Dragon");
//            AddCardInfo(BlueEyesCardId.TheWhiteStoneOfAncients, "The White Stone Of Ancients");
//            AddCardInfo(BlueEyesCardId.BlueEyesAlternativeWhiteDragon, "Blue Eyes Alternative White Dragon");
//            AddCardInfo(BlueEyesCardId.MaidenOfWhite, "Maiden of White");
//            AddCardInfo(BlueEyesCardId.MaidenOfEyesOfBlue, "Maiden of Eyes Of Blue");
//            AddCardInfo(BlueEyesCardId.BlueEyesSpiritDragon, "Blue Eyes Spirit Dragon");
//            AddCardInfo(BlueEyesCardId.DragonSpiritOfWhite, "Dragon Spirit Of White");
//            AddCardInfo(BlueEyesCardId.HieraticSunDragonOverlordOfHeliopolis, "Hieratic Sun Dragon Overlord Of Heliopolis");
//            AddCardInfo(BlueEyesCardId.Number46Dragluon, "Number 46 Dragluon");

//            Normal Summons
//             Add DragonSpiritOfWhite summon rule with custom condition
//            AddBotAction(new ActionRule(
//            action: MainAction.Summon,
//            cardId: BlueEyesCardId.DragonSpiritOfWhite,
//            condition: DragonSpiritOfWhiteSummon
//            ));

//            AddBotAction(new ActionRule(
//                action: MainAction.Summon,
//                cardId: BlueEyesCardId.MaidenOfEyesOfBlue,
//                condition: (s, c) => s.SummonableCards.Any()
//            ));

//            AddBotAction(new ActionRule(
//                action: MainAction.Summon,
//                cardId: BlueEyesCardId.MaidenOfWhite,
//                condition: (s, c) => s.SummonableCards.Any()
//            ));

//            AddBotAction(new ActionRule(
//                action: MainAction.Summon,
//                cardId: BlueEyesCardId.BlueEyesWhiteDragon,
//                condition: (s, c) => s.SummonableCards.Any()
//            ));

//            Set Monsters
//            AddBotAction(new ActionRule(
//                action: MainAction.SetMonster,
//                cardId: BlueEyesCardId.TheWhiteStoneOfAncients
//            ));

//            Reposable Monsters
//            AddBotAction(new ActionRule(
//                action: MainAction.Repos,
//                cardId: BlueEyesCardId.TheWhiteStoneOfAncients
//            ));

//            Special Summons
//            AddBotAction(new ActionRule(
//                action: MainAction.SpSummon,
//                cardId: BlueEyesCardId.BlueEyesAlternativeWhiteDragon
//            ));

//            AddBotAction(new ActionRule(
//                action: MainAction.SpSummon,
//                cardId: BlueEyesCardId.BlueEyesSpiritDragon,
//                condition: (s, c) => s.SpecialSummonableCards.Any()
//            ));

//            AddBotAction(new ActionRule(
//                action: MainAction.SpSummon,
//                cardId: BlueEyesCardId.HieraticSunDragonOverlordOfHeliopolis
//            ));

//            AddBotAction(new ActionRule(
//                action: MainAction.SpSummon,
//                cardId: BlueEyesCardId.Number46Dragluon
//            ));

//            Activable cards
//            AddBotAction(new ActionRule(
//                action: MainAction.Activate,
//                cardId: BlueEyesCardId.TheWhiteStoneOfAncients,
//                condition: (s, c) => s.ActivableCards.Any()
//            ));

//            AddBotAction(new ActionRule(
//                action: MainAction.Activate,
//                cardId: BlueEyesCardId.MaidenOfWhite,
//                condition: (s, c) => s.ActivableCards.Any()
//            ));

//            AddBotAction(new ActionRule(
//                action: MainAction.Activate,
//                cardId: BlueEyesCardId.MaidenOfEyesOfBlue,
//                condition: (s, c) => s.ActivableCards.Any()
//            ));
//        }

//        / <summary>
//        / Adds a card info entry to the mapping.
//        / </summary>
//        private void AddCardInfo(int cardId, string cardName)
//        {
//            _cardInfoMap[cardId] = new Base.CardInfo(cardId, cardName);
//        }

//        / <summary>
//        / Adds a bot action rule to the list.
//        / </summary>
//        private void AddBotAction(ActionRule rule)
//        {
//            _actionRules.Add(rule);
//        }

//        / <summary>
//        / Overrides the base OnIdleCmd to execute action rules in sequence.
//        / </summary>
//        public override async Task<IdleCmdResponse> OnIdleCmd(IdleCmdData state)
//        {
//            await GameFieldInitialize();

//            await CheckDarkAttributeCard();
//            await GetEnemyGraveyardCards();
//            await GetStrongestMonster();

//            Activable cards, for each
//            foreach (var cards in state.ActivableCards)
//                {
//                    Console.WriteLine($"[Activable cards] => {cards.ID}");
//                }

//            Summonable cards, for each
//            foreach (var cards in state.SummonableCards)
//                {
//                    Console.WriteLine($"[Summonable cards] => {cards.ID}");
//                }

//            Activable cards, for each
//            foreach (var cards in state.ActivableCards)
//                {
//                    Console.WriteLine($"[Activable cards] => {cards.ID}");
//                }

//            Special Summonable cards, for each
//            foreach (var cards in state.SpecialSummonableCards)
//                {
//                    Console.WriteLine($"[SpecialSummonable cards] => {cards.ID}");
//                }

//            var response = ExecuteFirstValidAction(state, _actionRules);
//            return response ?? await base.OnIdleCmd(state); // Fallback to base implementation
//        }

//        / <summary>
//        / Overrides the base OnSelectEffectYn to handle specific card effects.
//        / </summary>
//        public override async Task<bool> OnSelectEffectYn(SelectEffectYnData state)
//        {
//            Check if the card is Dragon Spirit Of White
//            if (state.EffectCard.ID == BlueEyesCardId.DragonSpiritOfWhite)
//            {
//                ConsoleUtils.PrintColored("Activating effect for Dragon Spirit of White");
//                return true;
//            }

//            Fallback to base implementation for other cards
//            return await base.OnSelectEffectYn(state);
//        }

//        / <summary>
//        / Executes the first valid action from the rules in list order.
//        / </summary>
//        protected virtual IdleCmdResponse? ExecuteFirstValidAction(IdleCmdData state, IEnumerable<ActionRule> rules)
//        {
//            foreach (var rule in rules)
//            {
//                var cards = rule.CardSelector(state).ToList();
//                if (cards.TryGetByID(rule.CardId, out var card, out var index) && rule.Condition(state, card))
//                {
//                    Get card name from mapping, fallback to CardId if not found
//                    string cardDisplay = _cardInfoMap.TryGetValue(rule.CardId, out var cardInfo)
//                        ? cardInfo.CardName
//                        : rule.CardId.ToString();
//                    ConsoleUtils.PrintColored($"Bot Actions => ExecuteAction. Card: {cardDisplay}");
//                    return ExecuteAction(rule.Action, (ushort)index);
//                }
//            }

//            return null; // No valid action found
//        }

//        / <summary>
//        / Executes the specified action with the given card index.
//        / </summary>
//        protected virtual IdleCmdResponse ExecuteAction(MainAction action, ushort index)
//        {
//            return new IdleCmdResponse(action, index);
//        }
//        private static async Task GameFieldInitialize()
//        {
//            await GameField.Initialize();

//            var ownMonsterZones = GameField.OwnSide.MonsterZones;
//            var enemyMonsterZones = GameField.EnemySide.MonsterZones;

//            Player(Me)
//            int ownMonsterCount = GameField.GetOwnMonsterZoneCardCount();
//            Console.WriteLine($"Player (Me) [MonsterZones] => {ownMonsterCount}");
//            int ownSpellZone0Count = GameField.GetOwnSpellZoneCardCount();
//            Console.WriteLine($"Player (Me) [SpellZones] =>  {ownSpellZone0Count}");

//            Bot
//            int enemyMonsterCount = GameField.GetEnemyMonsterZoneCardCount();
//            Console.WriteLine($"Bot [MonsterZones] => : {enemyMonsterCount}");
//            int enemySpellZone0Count = GameField.GetEnemyZoneCardCount(0, isMonsterZone: false);
//            Console.WriteLine($"Bot [SpellZones] => : {enemySpellZone0Count}");
//        }

//        / <summary>
//        / Condition to summon DragonSpiritOfWhite: checks if there are cards in the SpellZone and if the card is summonable.
//        / </summary>
//        private bool DragonSpiritOfWhiteSummon(IdleCmdData state, DynamicCard card)
//        {
//            Checks for cards in player's SpellZones

//           int ownSpellZoneCount = 0;// GameField.GetOwnSpellZoneCardCount();
//           bool hasSpellCards = ownSpellZoneCount > 0;

//            Checks for summonable cards

//           bool canSummon = state.SummonableCards.Any(c => c.ID == BlueEyesCardId.DragonSpiritOfWhite);

//            if (hasSpellCards && canSummon)
//                ConsoleUtils.PrintColored($"DragonSpiritOfWhiteSummon: Can summon (SpellZoneCount: {ownSpellZoneCount})");
//            return false;
//            return hasSpellCards && canSummon;
//        }

//        Verificar se há uma carta com atributo Dark na mão do jogador:
//        private static async Task CheckDarkAttributeCard()
//        {
//            Console.WriteLine("#### Check Dark Attribute Card! ####");
//            await GameField.Initialize();
//            bool hasDarkCard = false;// await GameField.HasCardWithAttributeInHand(CardAttribute.Dark);
//            Console.WriteLine(hasDarkCard ? "Found a Dark attribute card in hand!" : "No Dark attribute card in hand.");
//        }
//        private static async Task GetEnemyGraveyardCards()
//        {
//            Console.WriteLine("#### GetEnemyGraveyardCards! ####");
//            await GameField.Initialize();
//            var graveCards = await GameField.GetCardsInLocation(location: CardLocation.Grave, controller: 1);
//            Console.WriteLine($"Enemy graveyard has {graveCards.Count} cards:");
//            foreach (var card in graveCards)
//            {
//                Convert the numeric Attribute to a string of attribute names
//                string attributeNames = card.Attribute == 0 ? "None" : string.Join(", ", System.Enum.GetValues(typeof(CardAttribute))
//                    .Cast<CardAttribute>()
//                    .Where(attr => attr != CardAttribute.None && card.Attribute.Equals((byte)attr))
//                    .Select(attr => attr.ToString()));
//                Console.WriteLine($"Card ID: {card.ID}, Attribute: {attributeNames}");
//            }
//        }

//        Find the strongest monster in the player's monster zone:
//        private static async Task GetStrongestMonster()
//        {
//            await GameField.Initialize();
//            var strongestMonster = await GameField.GetStrongestMonsterInLocation(CardLocation.MonsterZone);
//            if (strongestMonster.ID > 0)
//                Console.WriteLine($"Strongest face-up monster: ID {strongestMonster.ID}, Attack {strongestMonster.Attack}");
//            else
//                Console.WriteLine("No face-up monsters found in the monster zone.");
//        }
//    }
//}