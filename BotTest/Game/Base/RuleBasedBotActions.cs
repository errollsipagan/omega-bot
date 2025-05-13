using BotTest.Utils;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;
using OmegaBotWrapper;
using BotTest.Game.Field;
using System.Reflection;
#pragma warning disable 8605
#pragma warning disable 8618
namespace BotTest.Game.Base
{
    public abstract class RuleBasedBotActions : BaseBotActions
    {
        public GameField _gameField;
        protected readonly List<ActionRule> _actionRules;
        protected readonly Dictionary<int, Base.CardInfo> _cardInfoMap;

        protected RuleBasedBotActions()
        {
            _actionRules = new List<ActionRule>();
            _cardInfoMap = new Dictionary<int, Base.CardInfo>();
        }

        public override void RegisterActions(DuelBotWrapper bot)
        {
            _gameField = new GameField(bot);
            base.RegisterActions(bot);
        }

        protected void AddCardInfo(int cardId, string cardName)
        {
            _cardInfoMap[cardId] = new Base.CardInfo(cardId, cardName);
        }

        protected void AddBotAction(ActionRule rule)
        {
            _actionRules.Add(rule);
        }

        protected virtual IdleCmdResponse ExecuteAction(MainAction action, ushort index)
        {
            return new IdleCmdResponse(action, index);
        }

        protected virtual IdleCmdResponse? ExecuteFirstValidAction(IdleCmdData state, IEnumerable<ActionRule> rules)
        {
            foreach (var rule in rules)
            {
                var cards = rule.CardSelector(state).ToList();

                for (int i = 0; i < cards.Count; i++)
                {
                    if (cards[i].ID == rule.CardId && rule.Condition(state, cards[i]))
                    {
                        string cardDisplay = _cardInfoMap.TryGetValue(rule.CardId, out var cardInfo)
                        ? cardInfo.CardName
                        : rule.CardId.ToString();
                        ConsoleUtils.PrintColored($"Bot Actions => ExecuteAction. Card: {cardDisplay}");
                        return ExecuteAction(rule.Action, (ushort)i);
                    }
                }

                //if (cards.TryGetByID(rule.CardId, out var card, out var index) && rule.Condition(state, card))
                //{
                //    string cardDisplay = _cardInfoMap.TryGetValue(rule.CardId, out var cardInfo)
                //        ? cardInfo.CardName
                //        : rule.CardId.ToString();
                //    ConsoleUtils.PrintColored($"Bot Actions => ExecuteAction. Card: {cardDisplay}");
                //    return ExecuteAction(rule.Action, (ushort)index);
                //}
            }
            return null;
        }

        public override async Task<IdleCmdResponse> OnIdleCmd(IdleCmdData state)
        {
            await _gameField.GetBotField();

            foreach (var cards in state.ActivableCards)
                ConsoleUtils.PrintColored($"[Activable cards] => {cards.ID}");
            foreach (var cards in state.SummonableCards)
                ConsoleUtils.PrintColored($"[Summonable cards] => {cards.ID}");
            foreach (var cards in state.ReposableCards)
                ConsoleUtils.PrintColored($"[Reposable cards] => {cards.ID}");
            foreach (var cards in state.SpecialSummonableCards)
                ConsoleUtils.PrintColored($"[SpecialSummonable cards] => {cards.ID}");

            var response = ExecuteFirstValidAction(state, _actionRules);
            return response ?? await base.OnIdleCmd(state);
        }

        protected void InitializeCardInfo<TCardIdClass>()
        {
            var fields = typeof(TCardIdClass).GetFields(BindingFlags.Public | BindingFlags.Static)
                                             .Where(f => f.IsLiteral && !f.IsInitOnly);

            foreach (var field in fields)
            {
                // Get the ID from the constant (e.g. 89631139 for BlueEyesWhiteDragon)
                int cardId = (int)field.GetValue(null);

                // Convert the field name to a human-readable name (e.g., BlueEyesWhiteDragon → Blue Eyes White Dragon)
                string cardName = FormatCardName(field.Name);

                // Add
                AddCardInfo(cardId, cardName);
            }
        }

        // Converts names like "BlueEyesWhiteDragon" to "Blue Eyes White Dragon"
        private static string FormatCardName(string fieldName)
        {
            return string.Concat(fieldName.Select(c => char.IsUpper(c) ? " " + c : c.ToString())).Trim();
        }

        public void BotAction(MainAction action, int cardId, Func<IdleCmdData, DynamicCard, bool>? condition = null)
        {
            AddBotAction(new ActionRule(action, cardId, condition));
        }
    }
}
