//using OmegaBotWrapper.Decks;
//using OmegaBotWrapper.Enum;
//using OmegaBotWrapper.Game.Base;
//using OmegaBotWrapper.States;
//using OmegaBotWrapper.Struct;
//using System.Collections.Generic;
//using System.Linq;

//namespace OmegaBotWrapper.Game.AI.Bots.BotTest
//{
//    public class BotTestActions : BaseBotActions
//    {
//        private readonly List<ActionRule> _actionRules;
//        private readonly Dictionary<int, Base.CardInfo> _cardInfoMap;

//        public BotTestActions(MasterDuelDeck deck = MasterDuelDeck.BotTest)
//        {
//            DeckName = DeckManager.GetDeckName(deck);
//            DeckHash = DeckManager.GetDeckHash(deck);

//            _actionRules = new List<ActionRule>();
//            _cardInfoMap = new Dictionary<int, Base.CardInfo>();

//            // Initialize card info mapping
//            AddCardInfo(BotTestCardId.ShinyBlack, "Shiny Black");
//            AddCardInfo(BotTestCardId.CardDestruction, "Card Destruction");
//            AddCardInfo(BotTestCardId.Jinzo, "Jinzo");
//            AddCardInfo(BotTestCardId.BlueEyes, "Blue-Eyes");

//            AddBotAction(new ActionRule(
//                action: MainAction.Summon,
//                cardId: BotTestCardId.ShinyBlack,
//                cardSelector: s => s.SummonableCards,
//                condition: (s, c) => s.SummonableCards.Any()
//            ));

//            AddBotAction(new ActionRule(
//                action: MainAction.Activate,
//                cardId: BotTestCardId.CardDestruction,
//                cardSelector: s => s.ActivableCards,
//                condition: (s, c) => s.ActivableCards.Any()
//            ));

//            AddBotAction(new ActionRule(
//                action: MainAction.Summon,
//                cardId: BotTestCardId.Jinzo,
//                cardSelector: s => s.SummonableCards,
//                condition: (s, c) => s.SummonableCards.Any()
//            ));

//            AddBotAction(new ActionRule(
//                action: MainAction.Summon,
//                cardId: BotTestCardId.BlueEyes,
//                cardSelector: s => s.SummonableCards,
//                condition: (s, c) => s.SummonableCards.Any()
//            ));
//        }

//        /// <summary>
//        /// Adds a card info entry to the mapping.
//        /// </summary>
//        private void AddCardInfo(int cardId, string cardName)
//        {
//            _cardInfoMap[cardId] = new Base.CardInfo(cardId, cardName);
//        }

//        /// <summary>
//        /// Adds a bot action rule to the list.
//        /// </summary>
//        private void AddBotAction(ActionRule rule)
//        {
//            _actionRules.Add(rule);
//        }

//        /// <summary>
//        /// Overrides the base OnIdleCmd to execute action rules in sequence.
//        /// </summary>
//        public override async Task<IdleCmdResponse> OnIdleCmd(IdleCmdData state)
//        {
//            var response = ExecuteFirstValidAction(state, _actionRules);
//            return response ?? await base.OnIdleCmd(state); // Fallback to base implementation
//        }

//        /// <summary>
//        /// Executes the first valid action from the rules in list order.
//        /// </summary>
//        protected virtual IdleCmdResponse? ExecuteFirstValidAction(IdleCmdData state, IEnumerable<ActionRule> rules)
//        {
//            foreach (var rule in rules)
//            {
//                var cards = rule.CardSelector(state).ToList();
//                if (cards.TryGetByID(rule.CardId, out var card, out var index) && rule.Condition(state, card))
//                {
//                    // Get card name from mapping, fallback to CardId if not found
//                    string cardDisplay = _cardInfoMap.TryGetValue(rule.CardId, out var cardInfo)
//                        ? cardInfo.CardName
//                        : rule.CardId.ToString();
//                    ////ConsoleUtils.PrintColored($"BotTestActions => ExecuteAction. Card: {cardDisplay}");
//                    return ExecuteAction(rule.Action, (ushort)index);
//                }
//            }

//            return null; // No valid action found
//        }

//        /// <summary>
//        /// Executes the specified action with the given card index.
//        /// </summary>
//        protected virtual IdleCmdResponse ExecuteAction(MainAction action, ushort index)
//        {
//            return new IdleCmdResponse(action, index);
//        }
//    }
//}