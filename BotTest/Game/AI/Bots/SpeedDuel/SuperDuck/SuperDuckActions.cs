using BotTest.Decks;
using BotTest.Game.AI.Bots.MasterDuel.BlueEyes;
using BotTest.Game.Base;
using BotTest.Game.Field;
using BotTest.Utils;
using OmegaBotWrapper;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;
using System.Data;

#pragma warning disable CS8618

namespace BotTest.Game.AI.Bots.SpeedDuel.SuperDuck
{
    public class SuperDuckActions : BaseBotActions
    {
        public GameField _gameField;

        private readonly List<ActionRule> _actionRules;
        private readonly Dictionary<int, Base.CardInfo> _cardInfoMap;
        private readonly Dictionary<int, bool> _selectChainMap = new();

        public SuperDuckActions(SpeedDuelDeck deck = SpeedDuelDeck.SuperDuck)
        {
            _gameField = null!;

            _actionRules = new List<ActionRule>();
            _cardInfoMap = new Dictionary<int, Base.CardInfo>();

            //Initialize card info mapping
            AddCardInfo(SuperDuckCardId.HammerShot, "Hammer Shot");
            AddCardInfo(SuperDuckCardId.EnchantingFittingRoom, "Enchanting Fitting Room");
            AddCardInfo(SuperDuckCardId.AllureOfDarkness, "Allure Of Darkness");
            AddCardInfo(SuperDuckCardId.AllureOfDarkness, "Room For Growth");
            AddCardInfo(SuperDuckCardId.Waboku, "Waboku");
            AddCardInfo(SuperDuckCardId.WakingTheDragon, "Waking The Dragon");
            AddCardInfo(SuperDuckCardId.JustiBreak, "Justi-Break");
            AddCardInfo(SuperDuckCardId.FloodgateTrapHole, "Floodgate Trap Hole");

            DeckName = DeckManager.GetDeckName(deck);
            DeckHash = DeckManager.GetDeckHash(deck);

            AddBotAction(new ActionRule(
                action: MainAction.Activate,
                cardId: SuperDuckCardId.HammerShot,
                condition: HammerShotCondition
            ));

            AddBotAction(new ActionRule(
                action: MainAction.Activate,
                cardId: SuperDuckCardId.EnchantingFittingRoom,
                condition: EnchantingFittingRoomCondition
            ));

            AddBotAction(new ActionRule(
                action: MainAction.Activate,
                cardId: SuperDuckCardId.AllureOfDarkness,
                condition: AllureOfDarknessCondition
            ));

            AddBotAction(new ActionRule(
                action: MainAction.Activate,
                cardId: SuperDuckCardId.RoomForGrowth
            ));

            //Set Trap
            AddBotAction(new ActionRule(
                action: MainAction.SetSpell,
                cardId: SuperDuckCardId.Waboku
            ));

            AddBotAction(new ActionRule(
                action: MainAction.SetSpell,
                cardId: SuperDuckCardId.WakingTheDragon
            ));

            AddBotAction(new ActionRule(
                action: MainAction.SetSpell,
                cardId: SuperDuckCardId.JustiBreak
            ));

            AddBotAction(new ActionRule(
                action: MainAction.SetSpell,
                cardId: SuperDuckCardId.FloodgateTrapHole
            ));

            AddBotAction(new ActionRule(
                action: MainAction.Activate,
                cardId: SuperDuckCardId.SphereKuriboh
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
            _selectChainMap.Clear();
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

        public override async Task<int> OnSelectChain(SelectChainData state)
        {
            Console.WriteLine("OnSelectChain entrei aqui!");
            //Selectable cards, for each
            foreach (var cards in state.SelectableCards)
            {
                Console.WriteLine($"[Selectable cards] => {cards.ID}");
            }

            //Descriptions cards, for each
            foreach (var cards in state.Descriptions)
            {
                Console.WriteLine($"[Descriptions cards] => {cards}");
            }
            
            //bool wabokuIsFaceUp = await _gameField.IsCardInBotZone(SuperDuckCardId.Waboku, CardLocation.SpellZone, isOwnSide: false);
            if (state.SelectableCards.TryGetByID(SuperDuckCardId.Waboku, out var card, out var index) && !_selectChainMap.ContainsKey(card.ID))
            {
                _selectChainMap.Add(SuperDuckCardId.Waboku, true);
                return index;
            }

            if (state.SelectableCards.TryGetByID(SuperDuckCardId.FloodgateTrapHole, out _, out index))
                return index;

            if (state.SelectableCards.TryGetByID(SuperDuckCardId.JustiBreak, out _, out index))
                return index;

            if (state.SelectableCards.TryGetByID(SuperDuckCardId.WakingTheDragon, out _, out index))
                return index;

            if (state.SelectableCards.TryGetByID(SuperDuckCardId.SphereKuriboh, out _, out index))
                return index;

            return await base.OnSelectChain(state); // Fallback to base implementation
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
                    ConsoleUtils.PrintColored($"Bot Actions => ExecuteAction. Card: {cardDisplay}");
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

        private bool EnchantingFittingRoomCondition(IdleCmdData state, DynamicCard card)
        {
            bool canActivate = state.ActivableCards.Any(c => c.ID == SuperDuckCardId.EnchantingFittingRoom);
            int botMonstersCount = _gameField.GetMonstersCount().Result;
            Console.WriteLine($"[EnchantingFittingRoomCondition] botMonstersCount {botMonstersCount}");
            bool botMonstersCondition = botMonstersCount <= 1;
            if (canActivate && botMonstersCondition)
                ConsoleUtils.PrintColored($"EnchantingFittingRoom: Can activate Enchanting Fitting Room");
            return canActivate && botMonstersCondition;
        }

        private bool AllureOfDarknessCondition(IdleCmdData state, DynamicCard card)
        {
            bool canActivate = state.ActivableCards.Any(c => c.ID == SuperDuckCardId.AllureOfDarkness);
            bool botHasDarkAttribute = _gameField.HasCardWithAttributeInHand(attribute: CardAttribute.Dark).Result;
            if (canActivate && botHasDarkAttribute)
                ConsoleUtils.PrintColored($"AllureOfDarknessCondition: Can activate Allure of Darkness");
            return canActivate && botHasDarkAttribute;
        }
    }
}
