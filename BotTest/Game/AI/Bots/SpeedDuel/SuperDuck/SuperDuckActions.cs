using BotTest.Decks;
using BotTest.Game.AI.Bots.MasterDuel.BlueEyes;
using BotTest.Game.Base;
using BotTest.Utils;
using OmegaBotWrapper;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;
using System.Data;

namespace BotTest.Game.AI.Bots.SpeedDuel.SuperDuck
{
    public class SuperDuckActions : RuleBasedBotActions
    {
        private readonly Dictionary<int, bool> _selectChainMap = new();

        public SuperDuckActions(SpeedDuelDeck deck = SpeedDuelDeck.SuperDuck)
        {
            DeckName = DeckManager.GetDeckName(deck);
            DeckHash = DeckManager.GetDeckHash(deck);
            InitializeCardInfo<SuperDuckCardId>();

            BotAction(
                action: MainAction.Activate,
                cardId: SuperDuckCardId.HammerShot,
                condition: HammerShotCondition
            );

            BotAction(
                action: MainAction.Activate,
                cardId: SuperDuckCardId.EnchantingFittingRoom,
                condition: EnchantingFittingRoomCondition
            );

            BotAction(
                action: MainAction.Activate,
                cardId: SuperDuckCardId.AllureOfDarkness,
                condition: AllureOfDarknessCondition
            );

            BotAction(
                action: MainAction.Activate,
                cardId: SuperDuckCardId.RoomForGrowth
            );

            //Set Trap
            BotAction(
                action: MainAction.SetSpell,
                cardId: SuperDuckCardId.Waboku
            );

            BotAction(
                action: MainAction.SetSpell,
                cardId: SuperDuckCardId.WakingTheDragon
            );

            BotAction(
                action: MainAction.SetSpell,
                cardId: SuperDuckCardId.JustiBreak
            );

            BotAction(
                action: MainAction.SetSpell,
                cardId: SuperDuckCardId.FloodgateTrapHole
            );

            BotAction(
                action: MainAction.Activate,
                cardId: SuperDuckCardId.BreakerTheMagicalWarrior,
                condition: BreakerTheMagicalWarriorCondition
            );

            //Repos if Face-up defense
            BotAction(
                action: MainAction.Repos,
                cardId: SuperDuckCardId.HunterDragon,
                condition: ReposableCondition
            );

            BotAction(
                action: MainAction.Repos,
                cardId: SuperDuckCardId.SonicDuck,
                condition: ReposableCondition
            );

            BotAction(
                action: MainAction.Repos,
                cardId: SuperDuckCardId.DragonZombie,
                condition: ReposableCondition
            );

            BotAction(
                action: MainAction.Repos,
                cardId: SuperDuckCardId.CyberEndDragon,
                condition: ReposableCondition
            );

            BotAction(
                action: MainAction.Repos,
                cardId: SuperDuckCardId.DestinyEndDragon,
                condition: ReposableCondition
            );

            BotAction(
                action: MainAction.Repos,
                cardId: SuperDuckCardId.ArcanaKnightJoker,
                condition: ReposableCondition
            );

            BotAction(
                action: MainAction.Repos,
                cardId: SuperDuckCardId.SuperVehicroidJumboDrill,
                condition: ReposableCondition
            );

            BotAction(
                action: MainAction.Repos,
                cardId: SuperDuckCardId.OjamaKing,
                condition: ReposableConditionForOjamaKing
            );
        }

        /// <summary>
        /// Overrides the base OnIdleCmd to execute action rules in sequence.
        /// </summary>
        public override async Task<IdleCmdResponse> OnIdleCmd(IdleCmdData state)
        {
            await _gameField.GetBotField();

            if (_selectChainMap.Any())
                _selectChainMap.Clear();

            /// <summary>
            /// Removes the SphereKuriboh directly from state.SummonableCards.
            /// </summary>
            state.SummonableCards = state.SummonableCards.Where(card => card.ID != SuperDuckCardId.SphereKuriboh).ToArray();

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
            //Selectable cards, for each
            foreach (var cards in state.SelectableCards)
            {
                Console.WriteLine($"[OnSelectChain] [Selectable cards] => {cards.ID}");
            }

            //Descriptions cards, for each
            foreach (var cards in state.Descriptions)
            {
                Console.WriteLine($"[Descriptions cards] => {cards}");
            }

            bool battleStartPhase = GetCurrentDuelPhase() == DuelPhase.BattleStart;
            bool currentBotPlayer = GetCurrentPlayer() == 1; //Opponent/Player/Enemy
            if (state.SelectableCards.TryGetByID(SuperDuckCardId.Waboku, out var card, out var index) &&
                !_selectChainMap.ContainsKey(card.ID) &&
                battleStartPhase &&
                currentBotPlayer)
            {
                _selectChainMap.Add(SuperDuckCardId.Waboku, true); //unique trap
                return index;
            }

            if (state.SelectableCards.TryGetByID(SuperDuckCardId.FloodgateTrapHole, out _, out index))
            {
                bool isUniqueTrap = _gameField.IsUniqueTrapChain(SuperDuckCardId.FloodgateTrapHole).Result;
                if (!isUniqueTrap)
                    return index;
            }

            //if (state.SelectableCards.TryGetByID(SuperDuckCardId.FloodgateTrapHole, out card, out index) &&
            //    !_selectChainMap.ContainsKey(card.ID))
            //{
            //    _selectChainMap.Add(card.ID, true); //unique trap
            //    return index;
            //}

            if (state.SelectableCards.TryGetByID(SuperDuckCardId.JustiBreak, out _, out index))
                return index;

            if (state.SelectableCards.TryGetByID(SuperDuckCardId.WakingTheDragon, out _, out index))
                return index;

            if (state.SelectableCards.TryGetByID(SuperDuckCardId.SphereKuriboh, out _, out index))
                return index;

            return await base.OnSelectChain(state); // Fallback to base implementation
        }

        public override async Task<bool> OnSelectEffectYn(SelectEffectYnData state)
        {
            Console.WriteLine($"[override] [OnSelectEffectYn]");
            if (state.EffectCard.IsCode(SuperDuckCardId.SphereKuriboh) &&
                !_selectChainMap.ContainsKey(SuperDuckCardId.Waboku))
                return true;

            if (state.EffectCard.IsCode(SuperDuckCardId.WakingTheDragon))
                return true;

            return await base.OnSelectEffectYn(state);
        }

        public override async Task<IList<byte>?> OnSelectCard(SelectCardData state)
        {
            //Check if Monsters Zone from Player are empty (Ojama King)
            /*bool monstersZonePlayer = _gameField.GetPlayerMonsterZoneCount().Result == 1;*/

            if (state.SelectionHint.Equals((int)DHint.SPSUMMON))
            {
                Console.WriteLine("[override][OnSelectCard] - Selecting Card to SPSUMMON");

                /*if (monstersZonePlayer)
                {
                    var ojamaKing = state.SelectableCards.Select((card, index) => new { Card = card, Index = index })
                                                        .Where(x => x.Card.ID == SuperDuckCardId.OjamaKing)
                                                        .Take(state.Minimum)
                                                        .Select(x => (byte)x.Index).ToArray();

                    if (ojamaKing.Any())
                        return ojamaKing;
                }*/

                var botCards = state.SelectableCards
                                    .Select((card, index) => new { Card = card, Index = index })
                                    .OrderByDescending(x => x.Card.Attack) // Order by best Attack
                                    .Take(state.Minimum)
                                    .Select(x => (byte)x.Index)
                                    .ToArray();
                // Respect Minimum (or Maximum if needed)
                if (botCards.Any())
                {
                    Console.WriteLine($"Selected {botCards.Length} bot-owned cards for SPSUMMON.");
                    return botCards;
                }
            }

            return await base.OnSelectCard(state);
        }

        public override async Task<CardPosition> OnSelectPosition(SelectPositionData state)
        {
            Console.WriteLine("[override][OnSelectPosition]");
            if (state.CardID.Equals(SuperDuckCardId.OjamaKing))
                return (CardPosition)state.Positions[1]; //Face-up Defense

            return await base.OnSelectPosition(state);
        }

        private bool HammerShotCondition(IdleCmdData state, DynamicCard card)
        {
            bool canActivate = card.IsCode(SuperDuckCardId.HammerShot);
            bool opponentHasStrongest = _gameField.IsFaceUpAttackStrongestMonsterPlayer().Result;
            if (canActivate && opponentHasStrongest)
                Console.WriteLine($"HammerShotCondition: Can activate Hammer Shot (Opponent has strongest monster)");
            return canActivate && opponentHasStrongest;
        }

        private bool EnchantingFittingRoomCondition(IdleCmdData state, DynamicCard card)
        {
            bool canActivate = card.IsCode(SuperDuckCardId.EnchantingFittingRoom);
            int botMonstersCount = _gameField.GetMonstersCount().Result;
            Console.WriteLine($"[EnchantingFittingRoomCondition] botMonstersCount {botMonstersCount}");
            bool botMonstersCondition = botMonstersCount <= 1;
            if (canActivate && botMonstersCondition)
                Console.WriteLine($"EnchantingFittingRoom: Can activate Enchanting Fitting Room");
            return canActivate && botMonstersCondition;
        }

        private bool AllureOfDarknessCondition(IdleCmdData state, DynamicCard card)
        {
            bool canActivate = card.IsCode(SuperDuckCardId.AllureOfDarkness);
            bool botHasDarkAttribute = _gameField.HasCardWithAttributeInHand(attribute: CardAttribute.Dark).Result;
            if (canActivate && botHasDarkAttribute)
                Console.WriteLine($"AllureOfDarknessCondition: Can activate Allure of Darkness");
            return canActivate && botHasDarkAttribute;
        }

        private bool BreakerTheMagicalWarriorCondition(IdleCmdData state, DynamicCard card)
        {
            bool canActivate = card.IsCode(SuperDuckCardId.BreakerTheMagicalWarrior);
            int playerSpellsCount = _gameField.GetPlayerSpellsCount().Result;
            Console.WriteLine($"BreakerTheMagicalWarriorCondition : playerSpellsCount {playerSpellsCount}");
            bool playerHasSpells = playerSpellsCount > 0;
            if (canActivate && playerHasSpells)
                Console.WriteLine($"BreakerTheMagicalWarrior: Can activate Breaker The Magical Warrior");
            return canActivate && playerHasSpells;
        }

        private bool ReposableCondition(IdleCmdData state, DynamicCard card)
        {
            bool isFaceUpDefense = card.IsFaceup() && card.IsDefense();
            return isFaceUpDefense;
        }

        private bool ReposableConditionForOjamaKing(IdleCmdData state, DynamicCard card)
        {
            bool isFaceUpAttack = card.IsFaceup() && card.IsAttack();
            return isFaceUpAttack;
        }

        //JustiBreak Condition
        //RoomForGrowth condition and Selectable
    }
}
