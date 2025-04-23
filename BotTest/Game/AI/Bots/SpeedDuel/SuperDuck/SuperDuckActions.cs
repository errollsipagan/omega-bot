using BotTest.Decks;
using BotTest.Game.AI.Bots.MasterDuel.BlueEyes;
using BotTest.Game.Base;
using BotTest.Game.Field;
using BotTest.Utils;
using OmegaBotWrapper;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;

#pragma warning disable CS8618

namespace BotTest.Game.AI.Bots.SpeedDuel.SuperDuck
{
    public class SuperDuckActions : BaseBotActions
    {
        public GameField _gameField;

        private readonly List<ActionRule> _actionRules;
        private readonly Dictionary<int, Base.CardInfo> _cardInfoMap;

        public SuperDuckActions(SpeedDuelDeck deck = SpeedDuelDeck.SuperDuck)
        {
            _gameField = null!;

            _actionRules = new List<ActionRule>();
            _cardInfoMap = new Dictionary<int, Base.CardInfo>();

            //Initialize card info mapping
            AddCardInfo(SuperDuckCardId.HammerShot, "Hammer Shot");
            AddCardInfo(SuperDuckCardId.EnchantingFittingRoom, "Enchanting Fitting Room");

            DeckName = DeckManager.GetDeckName(deck);
            DeckHash = DeckManager.GetDeckHash(deck);

            AddBotAction(new ActionRule(
                action: MainAction.Activate,
                cardId: SuperDuckCardId.HammerShot,
                condition: HammerShotCondition
            ));

            AddBotAction(new ActionRule(
                action: MainAction.Activate,
                cardId: SuperDuckCardId.EnchantingFittingRoom
            ));
        }

        /// <summary>
        /// Registers all the actions the bot should take during specific states
        /// </summary>
        /// <param name="bot">Bot instance which actions are being registered to</param>
        public override void RegisterActions(DuelBotWrapper bot)
        {
            _gameField = new GameField(bot);
            base.RegisterActions(bot);
        }

        /// <summary>
        /// Adds a card info entry to the mapping.
        /// </summary>
        private void AddCardInfo(int cardId, string cardName)
        {
            _cardInfoMap[cardId] = new Base.CardInfo(cardId, cardName);
        }

        /// <summary>
        /// Adds a bot action rule to the list.
        /// </summary>
        private void AddBotAction(ActionRule rule)
        {
            _actionRules.Add(rule);
        }

        /// <summary>
        /// Overrides the base OnIdleCmd to execute action rules in sequence.
        /// </summary>
        public override async Task<IdleCmdResponse> OnIdleCmd(IdleCmdData state)
        {
            await _gameField.GetBotField();
            //await _gameField.GetPlayerField();

            //Activable cards, for each
            foreach (var cards in state.ActivableCards)
            {
                Console.WriteLine($"[Activable cards] => {cards.ID}");
            }

            //Summonable cards, for each
            foreach (var cards in state.SummonableCards)
            {
                Console.WriteLine($"[Summonable cards] => {cards.ID}");
            }

            //Reposable cards, for each
            foreach (var cards in state.ReposableCards)
            {
                Console.WriteLine($"[Reposable cards] => {cards.ID}");
            }

            //Special Summonable cards, for each
            foreach (var cards in state.SpecialSummonableCards)
            {
                Console.WriteLine($"[SpecialSummonable cards] => {cards.ID}");
            }

            var response = ExecuteFirstValidAction(state, _actionRules);
            return response ?? await base.OnIdleCmd(state); // Fallback to base implementation
        }

        /// <summary>
        /// Executes the first valid action from the rules in list order.
        /// </summary>
        protected virtual IdleCmdResponse? ExecuteFirstValidAction(IdleCmdData state, IEnumerable<ActionRule> rules)
        {
            foreach (var rule in rules)
            {
                var cards = rule.CardSelector(state).ToList();
                if (cards.TryGetByID(rule.CardId, out var card, out var index) && rule.Condition(state, card))
                {
                    // Get card name from mapping, fallback to CardId if not found
                    string cardDisplay = _cardInfoMap.TryGetValue(rule.CardId, out var cardInfo)
                        ? cardInfo.CardName
                        : rule.CardId.ToString();
                    ////ConsoleUtils.PrintColored($"Bot Actions => ExecuteAction. Card: {cardDisplay}");
                    return ExecuteAction(rule.Action, (ushort)index);
                }
            }

            return null; // No valid action found
        }

        /// <summary>
        /// Executes the specified action with the given card index.
        /// </summary>
        protected virtual IdleCmdResponse ExecuteAction(MainAction action, ushort index)
        {
            return new IdleCmdResponse(action, index);
        }

        private bool HammerShotCondition(IdleCmdData state, DynamicCard card)
        {
            bool canActivate = state.ActivableCards.Any(c => c.ID == SuperDuckCardId.HammerShot);
            bool opponentHasStrongest = _gameField.IsFaceUpAttackStrongestMonsterPlayer().Result;
            if (canActivate && opponentHasStrongest)
                ConsoleUtils.PrintColored($"HammerShotCondition: Can activate Hammer Shot (Opponent has strongest monster)");
            return canActivate && opponentHasStrongest;
        }
    }
}
