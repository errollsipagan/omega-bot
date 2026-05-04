using BotTest.Utils;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;
using System.Reflection;

#pragma warning disable 8605
#pragma warning disable 8618

namespace BotTest.Game.Base
{
    public abstract class RuleEngineBotActions : FieldBotActions
    {
        protected readonly List<ActionRule> _actionRules;
        protected readonly Dictionary<int, Base.CardInfo> _cardInfoMap;

        protected RuleEngineBotActions()
        {
            _actionRules = new List<ActionRule>();
            _cardInfoMap = new Dictionary<int, Base.CardInfo>();
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

        protected virtual async Task<IdleCmdResponse?> ExecuteFirstValidAction(IdleCmdData state, IEnumerable<ActionRule> rules)
        {
            foreach (var rule in rules)
            {
                var cards = rule.CardSelector(state).ToList();
                var originalCards = GetCardsForAction(state, rule.Action);

                for (int i = 0; i < cards.Count; i++)
                {
                    if (cards[i].ID == rule.CardId && await rule.Condition(state, cards[i]))
                    {
                        int originalIndex = Array.IndexOf(originalCards, cards[i]);
                        if (originalIndex < 0)
                            continue;

                        string cardDisplay = _cardInfoMap.TryGetValue(rule.CardId, out var cardInfo)
                            ? cardInfo.CardName
                            : rule.CardId.ToString();
                        ConsoleUtils.PrintColored($"Bot Actions => ExecuteAction. Card: {cardDisplay}");
                        return ExecuteAction(rule.Action, (ushort)originalIndex);
                    }
                }
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

            var response = await ExecuteFirstValidAction(state, _actionRules);
            return response ?? await base.OnIdleCmd(state);
        }

        private static DynamicCard[] GetCardsForAction(IdleCmdData state, MainAction action)
        {
            return action switch
            {
                MainAction.Summon => state.SummonableCards,
                MainAction.SpSummon => state.SpecialSummonableCards,
                MainAction.Activate => state.ActivableCards,
                MainAction.SetMonster => state.MonsterSetableCards,
                MainAction.SetSpell => state.SpellSetableCards,
                MainAction.Repos => state.ReposableCards,
                _ => Array.Empty<DynamicCard>()
            };
        }

        protected void InitializeCardInfo<TCardIdClass>()
        {
            var fields = typeof(TCardIdClass).GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(field => field.IsLiteral && !field.IsInitOnly);

            foreach (var field in fields)
            {
                int cardId = (int)field.GetValue(null);
                string cardName = FormatCardName(field.Name);
                AddCardInfo(cardId, cardName);
            }
        }

        private static string FormatCardName(string fieldName)
        {
            return string.Concat(fieldName.Select(c => char.IsUpper(c) ? " " + c : c.ToString())).Trim();
        }

        public void BotAction(MainAction action, int cardId, Func<IdleCmdData, DynamicCard, bool>? condition = null)
        {
            AddBotAction(new ActionRule(action, cardId, condition));
        }

        public void BotAction(MainAction action, int cardId, Func<IdleCmdData, DynamicCard, Task<bool>> condition)
        {
            AddBotAction(new ActionRule(action, cardId, condition));
        }
    }
}
