using OmegaBotWrapper.Enum;
using OmegaBotWrapper.Struct;
using OmegaBotWrapper;
using BotTest.Utils;
using OmegaBotWrapper.States;

namespace BotTest.Game.Field
{
    /// <summary>
    /// Class to manage and check the state of the Bot and Player fields using FieldInfo.
    /// </summary>
    public partial class GameField
    {
        /// <summary>
        /// Gets detailed information about the Bot field.
        /// </summary>
        /// <returns>FieldInfo structure with the Bot field data.</returns>
        public async Task<FieldInfo> GetBotField()
        {
            _fieldInfo = await _botWrapper.GetBotFieldInfo();
            return _fieldInfo;
        }

        /// <summary>
        /// Gets information about a specific card in the Bot field.
        /// </summary>
        /// <param name="controller">Card controller (0 = Player, 1 = Bot).</param>
        /// <param name="location">Location of the card (e.g. MonsterZone, SpellZone).</param>
        /// <param name="sequence">Sequence of the card in the specified zone.</param>
        /// <returns>DynamicCard structure with the card data.</returns>
        public async Task<DynamicCard> GetBotFieldCard(int controller, int location, int sequence)
        {
            //Console.WriteLine($"controller: {controller} / location: {location} / sequence: {sequence}");
            return await _botWrapper.GetBotFieldCardInfo(controller, location, sequence);
        }

        /// <summary>
        /// Checks for cards in a specific zone on the Bot's field.
        /// </summary>
        /// <param name="location">Location to check (e.g. MonsterZone, SpellZone).</param>
        /// <param name="isOwnSide">If true, checks the Bot's side; if false, checks the opponent's side.</param>
        /// <returns>True if there are cards in the specified zone, false otherwise.</returns>
        public async Task<bool> HasCardsInBotZone(CardLocation location, bool isOwnSide = true)
        {
            var field = await GetBotField();
            var side = isOwnSide ? field.OwnSide : field.EnemySide;
            return location switch
            {
                CardLocation.MonsterZone => side.MonsterZones.Any(zone => zone.Cards.Any()),
                CardLocation.SpellZone => side.SpellZones.Any(zone => zone.Cards.Any()),
                CardLocation.Hand => side.Hand.Any(),
                CardLocation.Grave => side.Grave.Any(),
                CardLocation.Removed => side.Banish.Any(),
                CardLocation.Deck => side.Main.Any(),
                CardLocation.Extra => side.Extra.Any(),
                _ => false
            };
        }

        /// <summary>
        /// Gets all cards in a specific zone in the Bot's field.
        /// </summary>
        /// <param name="location">Location to check (e.g. MonsterZone, SpellZone).</param>
        /// <param name="isOwnSide">If true, checks the Bot's side; if false, checks the opponent's side.</param>
        /// <returns>List of DynamicCards in the specified zone.</returns>
        public async Task<List<DynamicCard>> GetBotCardsInZone(CardLocation location, bool isOwnSide = true)
        {
            var field = await GetBotField();
            var side = isOwnSide ? field.OwnSide : field.EnemySide;
            var cards = new List<DynamicCard>();
            int controller = isOwnSide ? 0 : 1; // 0 = Bot, 1 = Player

            if (location == CardLocation.MonsterZone)
            {
                for (int seq = 0; seq < side.MonsterZones.Length; seq++)
                {
                    foreach (var cardId in side.MonsterZones[seq].Cards)
                    {
                        Console.WriteLine($"side.MonsterZones[seq] => {side.MonsterZones[seq].Cards}");                       
                        Console.WriteLine($"cardId => {cardId}");
                        if (cardId > 0)
                        {
                            var card = await GetBotFieldCard(controller, (int)CardLocation.MonsterZone, seq);
                            if (card.ID != 0) cards.Add(card);
                        }
                    }
                }
            }
            else if (location == CardLocation.SpellZone)
            {
                for (int seq = 0; seq < side.SpellZones.Length; seq++)
                {
                    foreach (var cardId in side.SpellZones[seq].Cards)
                    {
                        if (cardId > 0)
                        {
                            var card = await GetBotFieldCard(controller, (int)CardLocation.SpellZone, seq);
                            if (card.ID != 0) cards.Add(card);
                        }
                    }
                }
            }
            else if (location == CardLocation.Hand)
            {
                foreach (var cardId in side.Hand)
                {
                    if (cardId > 0)
                    {
                        var card = await GetBotFieldCard(controller, (int)CardLocation.Hand, 0);
                        if (card.ID != 0) cards.Add(card);
                    }
                }
            }
            else if (location == CardLocation.Grave)
            {
                foreach (var cardId in side.Grave)
                {
                    if (cardId > 0)
                    {
                        var card = await GetBotFieldCard(controller, (int)CardLocation.Grave, 0);
                        if (card.ID != 0) cards.Add(card);
                    }
                }
            }
            else if (location == CardLocation.Removed)
            {
                foreach (var cardId in side.Banish)
                {
                    if (cardId > 0)
                    {
                        var card = await GetBotFieldCard(controller, (int)CardLocation.Removed, 0);
                        if (card.ID != 0) cards.Add(card);
                    }
                }
            }

            return cards;
        }

        /// <summary>
        /// Checks for face-up cards in a specific zone on the Bot's field.
        /// </summary>
        /// <param name="location">Location to check (e.g. MonsterZone, SpellZone).</param>
        /// <param name="isOwnSide">If true, checks the Bot's side; if false, checks the opponent's side.</param>
        /// <returns>True if there are FaceUp cards in the zone, false otherwise.</returns>
        public async Task<bool> HasFaceUpCardsInBotZone(CardLocation location, bool isOwnSide = true)
        {
            var cards = await GetBotCardsInZone(location, isOwnSide);
            return cards.Any(card => card.IsFaceup());
        }

        /// <summary>
        /// Gets the card with the highest attack power in a specific zone on the Bot's field.
        /// </summary>
        /// <param name="location">Location to check (e.g. MonsterZone).</param>
        /// <param name="isOwnSide">If true, checks the Bot's side; if false, checks the opponent's side.</param>
        /// <returns>DynamicCard with the highest attack power or default if there are no cards.</returns>
        public async Task<DynamicCard> GetStrongestBotCardInZone(CardLocation location, bool isOwnSide = true)
        {
            var cards = await GetBotCardsInZone(location, isOwnSide);
            return cards.OrderByDescending(card => card.GetRealPower()).FirstOrDefault();
        }

        /// <summary>
        /// Checks if a specific card (by ID) is present in a zone on the Bot's field.
        /// </summary>
        /// <param name="cardId">ID of the card to check.</param>
        /// <param name="location">Location to check (e.g. MonsterZone, SpellZone).</param>
        /// <param name="isOwnSide">If true, checks the Bot's side; if false, checks the opponent's side.</param>
        /// <returns>True if the card is present, false otherwise.</returns>
        public async Task<bool> IsCardInBotZone(int cardId, CardLocation location, bool isOwnSide = true)
        {
            var cards = await GetPlayerCardsInZone(location, isOwnSide);
            return cards.Any(card => card.IsCode(cardId));
        }

        /// <summary>
        /// Checks if a specific card (by ID) is present in a zone in the Bot's field.
        /// </summary>
        /// <param name="cardId">ID of the card to check.</param>
        /// <param name="location">Location to check (e.g. MonsterZone, SpellZone).</param>
        /// <param name="isOwnSide">If true, checks the Bot's side; if false, checks the opponent's side.</param>
        /// <returns>True if the card is present, false otherwise.</returns>
        public async Task<bool> IsCardInPlayerZone(int cardId, CardLocation location, bool isOwnSide = true)
        {
            var cards = await GetPlayerCardsInZone(location, isOwnSide);
            return cards.Any(card => card.IsCode(cardId));
        }

        /// <summary>
        /// Gets the number of cards in a specific zone in the Bot's field.
        /// </summary>
        /// <param name="location">Location to check (e.g. MonsterZone, SpellZone).</param>
        /// <param name="isOwnSide">If true, checks the Bot's side; if false, checks the opponent's side.</param>
        /// <returns>Number of cards in the specified zone.</returns>
        public async Task<int> GetBotZoneCardCount(CardLocation location, bool isOwnSide = true)
        {
            var field = await GetBotField();
            var side = isOwnSide ? field.OwnSide : field.EnemySide;
            return location switch
            {
                CardLocation.MonsterZone => side.MonsterZones.Sum(zone => zone.Cards.Length),
                CardLocation.SpellZone => side.SpellZones.Sum(zone => zone.Cards.Length),
                CardLocation.Hand => side.Hand.Length,
                CardLocation.Grave => side.Grave.Length,
                CardLocation.Removed => side.Banish.Length,
                CardLocation.Deck => side.Main.Length,
                CardLocation.Extra => side.Extra.Length,
                _ => 0
            };
        }

        /// <summary>
        /// Gets the Bot's LifePoints.
        /// </summary>
        /// <param name="isOwnSide">If true, returns the Bot's lifepoints; if false, the opponent's.</param>
        /// <returns>Lifepoints of the specified side.</returns>
        public async Task<int> GetBotLifePoints(bool isOwnSide = true)
        {
            var field = await GetBotField();
            var side = isOwnSide ? field.OwnSide : field.EnemySide;
            return side.LifePoints;
        }

        /// <summary>
        /// Gets the Bot's LifePoints.
        /// </summary>
        /// <param name="isOwnSide">If true, returns the Bot's lifepoints; if false, the opponent's.</param>
        /// <returns>Lifepoints of the specified side.</returns>
        public async Task<int> GetPlayerLifePoints(bool isOwnSide = true)
        {
            var field = await GetPlayerField();
            var side = isOwnSide ? field.OwnSide : field.EnemySide;
            return side.LifePoints;
        }

        /// <summary>
        /// Checks if the Bot is under attack.
        /// </summary>
        /// <param name="isOwnSide">If true, checks the Bot's side; if false, checks the opponent's side.</param>
        /// <returns>True if the specified side is under attack, false otherwise.</returns>
        public async Task<bool> IsBotUnderAttack(bool isOwnSide = true)
        {
            var field = await GetBotField();
            var side = isOwnSide ? field.OwnSide : field.EnemySide;
            return side.UnderAttack;
        }

        /// <summary>
        /// Checks if the Player is under attack.
        /// </summary>
        /// <param name="isOwnSide">If true, checks the Bot's side; if false, checks the opponent's side.</param>
        /// <returns>True if the specified side is under attack, false otherwise.</returns>
        public async Task<bool> IsPlayerUnderAttack(bool isOwnSide = true)
        {
            var field = await GetPlayerField();
            var side = isOwnSide ? field.OwnSide : field.EnemySide;
            return side.UnderAttack;
        }

        /// <summary>
        /// Gets the monster in battle on the Bot's field, if any.
        /// </summary>
        /// <param name="isOwnSide">If true, checks the Bot's side; if false, checks the opponent's side.</param>
        /// <returns>DynamicCard of the monster in battle, or default if none.</returns>
        public async Task<DynamicCard> GetBotBattlingMonster(bool isOwnSide = true)
        {
            var field = await GetBotField();
            var side = isOwnSide ? field.OwnSide : field.EnemySide;
            return side.HasBattlingMonster ? side.BattlingMonster : default;
        }

        /// <summary>
        /// Gets all cards in a specific location on the bot's or opponent's side
        /// </summary>
        /// <param name="location">Location to check (e.g., CardLocation.Hand, CardLocation.MonsterZone)</param>
        /// <param name="controller">Controller of the cards (0 for player, 1 for opponent [BOT])</param>
        /// <returns>List of DynamicCard in the specified location</returns>
        public async Task<List<DynamicCard>> GetCardsInLocation(CardLocation location, int controller = 0)
        {
            var cards = new List<DynamicCard>();
            var side = controller == 0 ? OwnSide : EnemySide;

            switch (location)
            {
                case CardLocation.Hand:
                case CardLocation.Grave:
                case CardLocation.Removed:
                case CardLocation.Deck:
                case CardLocation.Extra:
                    int[] cardIds = location switch
                    {
                        CardLocation.Hand => side.Hand,
                        CardLocation.Grave => side.Grave,
                        CardLocation.Removed => side.Banish,
                        CardLocation.Deck => side.Main,
                        CardLocation.Extra => side.Extra,
                        _ => Array.Empty<int>()
                    };

                    for (int sequence = 0; sequence < cardIds.Length; sequence++)
                    {
                        if (cardIds[sequence] > 0) // Checks for a potentially valid card
                        {
                            try
                            {
                                var card = await GetBotFieldCard(controller, (int)location, sequence);
                                if (card.ID > 0) // Ensures that the letter is valid
                                    cards.Add(card);
                            }
                            catch (Exception ex) when (ex.Message.Contains("404") || ex.Message.Contains("No card found"))
                            {
                                Console.WriteLine("******** Ignore 404 errors (no card at coordinates) ********");
                                // Ignore 404 errors (no card at coordinates)
                                continue;
                            }
                        }
                    }
                    break;

                case CardLocation.MonsterZone:
                case CardLocation.SpellZone:
                case CardLocation.PendulumZone:
                case CardLocation.FieldZone:
                    CardArray[] zones = location switch
                    {
                        CardLocation.MonsterZone => side.MonsterZones,
                        CardLocation.SpellZone => side.SpellZones,
                        CardLocation.PendulumZone => side.SpellZones, // Pendulum zones are typically part of SpellZones
                        CardLocation.FieldZone => side.SpellZones,   // Field zone is typically part of SpellZones
                        _ => Array.Empty<CardArray>()
                    };

                    if (zones != null)
                    {
                        for (int zoneIndex = 0; zoneIndex < zones.Length; zoneIndex++)
                        {
                            var zone = zones[zoneIndex];
                            var cardZone = zone.Cards.FirstOrDefault();

                            if (cardZone > 0)
                            {
                                int sequence = zoneIndex;
                                try
                                {
                                    var card = await GetBotFieldCard(controller, (int)location, sequence);
                                    if (card.ID > 0)
                                        cards.Add(card);
                                }
                                catch (Exception ex) when (ex.Message.Contains("404") || ex.Message.Contains("No card found"))
                                {
                                    Console.WriteLine("******** Ignore 404 errors (no card at coordinates) ********");
                                    // Ignore 404 errors (no card at coordinates)
                                    continue;
                                }
                            }
                        }
                    }
                    break;

                default:
                    throw new ArgumentException($"Unsupported location: {location}");
            }

            return cards;
        }

        /// <summary>
        /// Gets all cards in a specific location with a specific type
        /// </summary>
        /// <param name="location">Location to check (e.g., CardLocation.Hand, CardLocation.MonsterZone)</param>
        /// <param name="type">Card type to filter by (e.g., CardType.Monster)</param>
        /// <param name="controller">Controller of the cards (0 for player, 1 for opponent)</param>
        /// <returns>List of DynamicCard with the specified type in the specified location</returns>
        public async Task<List<DynamicCard>> GetCardsWithTypeInLocation(CardLocation location, CardType type, int controller = 0)
        {
            var cards = await GetCardsInLocation(location, controller);
            return cards.Where(card => card.HasType(type)).ToList();
        }

        /// <summary>
        /// Gets the strongest monster card in a specific location (based on attack or defense)
        /// </summary>
        /// <param name="location">Location to check (e.g., CardLocation.MonsterZone)</param>
        /// <param name="controller">Controller of the cards (0 for player, 1 for opponent)</param>
        /// <param name="useDefense">If true, compares defense instead of attack</param>
        /// <returns>DynamicCard with the highest attack or defense in the specified location</returns>
        public async Task<IEnumerable<DynamicCard>> GetMonsterInLocation(CardLocation location, int controller = 0)
        {
            var monsters = await GetCardsWithTypeInLocation(location, CardType.Monster, controller);
            return monsters
                .Where(card => card.IsFaceup())
                .OrderByDescending(card => card.ID)
                .ToList();
        }

        /// <summary>
        /// Checks if the strongest face-up monster on the field belongs to the opponent
        /// </summary>
        /// <param name="location">Location to check (default is CardLocation.MonsterZone)</param>
        /// <returns>True if the opponent's face-up monster has the highest attack or if the opponent has monsters and the bot has none</returns>
        public async Task<bool> IsFaceUpAttackStrongestMonsterPlayer(CardLocation location = CardLocation.MonsterZone)
        {
            // Step 1: Check monster counts
            IEnumerable<DynamicCard> botMonster = await GetMonsterInLocation(location: CardLocation.MonsterZone, controller: 0);
            IEnumerable<DynamicCard> playerMonster = await GetMonsterInLocation(location: CardLocation.MonsterZone, controller: 1);
            int botMonsterCount = botMonster.Count();
            int playerMonsterCount = playerMonster.Count();

            if (playerMonsterCount > 0 && botMonsterCount == 0)
                return true;

            // Step 2: Get lists of face-up + attack face-up monsters
            var botMonsters = await GetCardsWithTypeInLocation(location, CardType.Monster, controller: 0);
            var opponentMonsters = await GetCardsWithTypeInLocation(location, CardType.Monster, controller: 1);

            var opponentFaceUpMonsters = opponentMonsters.Where(card => card.IsFaceup() && card.IsAttack()).ToList();
            var botFaceUpMonsters = botMonsters.Where(card => card.IsFaceup() && card.IsAttack()).ToList();

            // Step 3: Calculate maximum attack values
            int opponentMaxAttack = opponentFaceUpMonsters.Any() ? opponentFaceUpMonsters.Max(card => card.Attack) : 0;
            int botMaxAttack = botFaceUpMonsters.Any() ? botFaceUpMonsters.Max(card => card.Attack) : 0;

            // Step 4: Compare max attack values
            return opponentMaxAttack >= botMaxAttack;
        }

        /// <summary>
        /// Checks if there is a card with the specified attribute in the bot's hand
        /// </summary>
        /// <param name="attribute">Attribute to check (e.g., CardAttribute.Dark)</param>
        /// <returns>True if a card with the specified attribute is found in the bot's hand</returns>
        public async Task<bool> HasCardWithAttributeInHand(CardAttribute attribute)
        {
            var handCards = await GetCardsInLocation(CardLocation.Hand);
            return handCards.Any(card => (card.Attribute & (byte)attribute) != 0);
        }

        /// <summary>
        /// Gets all cards in the bot's hand with a specific attribute
        /// </summary>
        /// <param name="attribute">Attribute to filter by (e.g., CardAttribute.Dark)</param>
        /// <returns>List of DynamicCard with the specified attribute in the bot's hand</returns>
        public async Task<List<DynamicCard>> GetCardsWithAttributeInHand(CardAttribute attribute)
        {
            var handCards = await GetCardsInLocation(CardLocation.Hand);
            return handCards.Where(card => (card.Attribute & (byte)attribute) != 0).ToList();
        }

        /// <summary>
        /// Checks if there is a card with the specified type in a given location
        /// </summary>
        /// <param name="location">Location to check (e.g., CardLocation.Hand, CardLocation.MonsterZone)</param>
        /// <param name="type">Card type to check (e.g., CardType.Monster)</param>
        /// <param name="controller">Controller of the cards (0 for player, 1 for opponent)</param>
        /// <returns>True if a card with the specified type is found in the location</returns>
        public async Task<bool> HasCardWithTypeInLocation(CardLocation location, CardType type, int controller = 0)
        {
            var cards = await GetCardsInLocation(location, controller);
            return cards.Any(card => card.HasType(type));
        }

        public async Task<int> GetMonstersCount()
        {
            var monstersCards = await GetCardsInLocation(CardLocation.MonsterZone);
            return monstersCards.Where(card => (card.ID != 0)).Count();
        }

        /// <summary>
        /// Checks if there is a card with the specified id in the bot's hand
        /// </summary>
        /// <param name="attribute">Card ID to check (e.g., card.ID.IsCode(id))</param>
        /// <returns>True if a card with the specified card id is found in the bot's hand</returns>
        public async Task<bool> HasCardInHand(int cardId)
        {
            var handCards = await GetCardsInLocation(CardLocation.Hand);
            return handCards.Any(card => card.ID == cardId);
        }

        /// <summary>
        /// Checks if there is a card with the specified id in the bot's monster zone
        /// </summary>
        /// <param name="attribute">Card ID to check (e.g., card.ID.IsCode(id))</param>
        /// <returns>True if a card with the specified card id is found in the bot's monster zone</returns>
        public async Task<bool> HasCardInBotMonsterZone(int cardId)
        {
            var handCards = await GetCardsInLocation(CardLocation.MonsterZone);
            return handCards.Any(card => card.ID == cardId);
        }

        public static byte[] FilterSelectableCardsById(SelectCardData selectCardData, int cardId)
        {
            var botCards = selectCardData.SelectableCards
                .Select((card, index) => new { Card = card, Index = index })
                .Where(x => x.Card.ID == cardId) // Filter by cardId
                .Take(selectCardData.Minimum) // Respect state.Minimum
                .Select(x => (byte)x.Index)
                .ToArray();

            if (botCards.Any())
            {
                Console.WriteLine($"Selected {botCards.Length} cards with ID {cardId}.");
            }

            return botCards;
        }

        public async Task<bool> IsUniqueTrapChain(int trap)
        {
            var trapCards = await GetCardsInLocation(CardLocation.SpellZone);
            return trapCards.Any(card => card.IsCode(trap) && card.IsFaceup()); ;
        }
    }

}
