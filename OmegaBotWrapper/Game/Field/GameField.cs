using OmegaBotWrapper.Struct;
using OmegaBotWrapper.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OmegaBotWrapper.Game.Field
{
    /// <summary>
    /// Class to manage global playing field information
    /// </summary>
    public static class GameField
    {
        private static FieldInfo _fieldInfo;

        /// <summary>
        /// Initializes or updates field information
        /// </summary>
        public static async Task Initialize()
        {
            _fieldInfo = await DuelBotWrapper.GetPlayerFieldInfo();
        }

        /// <summary>
        /// Update field information manually
        /// </summary>
        public static void UpdateFieldInfo(FieldInfo fieldInfo)
        {
            _fieldInfo = fieldInfo;
        }

        /// <summary>
        /// Player Side (OwnSide)
        /// </summary>
        public static CardInfo OwnSide => _fieldInfo.OwnSide;

        /// <summary>
        /// Opponent's Side (EnemySide)
        /// </summary>
        public static CardInfo EnemySide => _fieldInfo.EnemySide;

        /// <summary>
        /// Gets a specific card's information from the player's perspective
        /// </summary>
        /// <param name="controller">Controller of the card (0 for player, 1 for opponent)</param>
        /// <param name="location">Location of the card (e.g., CardLocation.Hand, CardLocation.MonsterZone)</param>
        /// <param name="sequence">Sequence of the card in the given location</param>
        /// <returns>DynamicCard containing the card's information</returns>
        public static async Task<DynamicCard> GetPlayerFieldCardInfo(int controller, CardLocation location, int sequence)
        {
            return await DuelBotWrapper.GetPlayerFieldCardInfo(controller, (int)location, sequence);
        }
        /// <summary>
        /// Gets all cards in a specific location on the player's or opponent's side
        /// </summary>
        /// <param name="location">Location to check (e.g., CardLocation.Hand, CardLocation.MonsterZone)</param>
        /// <param name="controller">Controller of the cards (0 for player, 1 for opponent [BOT])</param>
        /// <returns>List of DynamicCard in the specified location</returns>
        public static async Task<List<DynamicCard>> GetCardsInLocation(CardLocation location, int controller = 1)
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
                        Console.WriteLine("cardIds[sequence] => " + cardIds[sequence]);

                        if (cardIds[sequence] > 0) // Checks for a potentially valid card
                        {
                            try
                            {
                                var card = await GetPlayerFieldCardInfo(controller, location, sequence);
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
                            //Console.WriteLine($"cardZone {cardZone}");

                            if (cardZone > 0)
                            {
                                int sequence = zoneIndex;
                                try
                                {
                                    var card = await GetPlayerFieldCardInfo(controller, location, sequence);
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
        /// Checks if there is a card with the specified attribute in the player's hand
        /// </summary>
        /// <param name="attribute">Attribute to check (e.g., CardAttribute.Dark)</param>
        /// <returns>True if a card with the specified attribute is found in the player's hand</returns>
        public static async Task<bool> HasCardWithAttributeInHand(CardAttribute attribute)
        {
            var handCards = await GetCardsInLocation(CardLocation.Hand);
            return handCards.Any(card => (card.Attribute & (byte)attribute) != 0);
        }

        /// <summary>
        /// Gets all cards in the player's hand with a specific attribute
        /// </summary>
        /// <param name="attribute">Attribute to filter by (e.g., CardAttribute.Dark)</param>
        /// <returns>List of DynamicCard with the specified attribute in the player's hand</returns>
        public static async Task<List<DynamicCard>> GetCardsWithAttributeInHand(CardAttribute attribute)
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
        public static async Task<bool> HasCardWithTypeInLocation(CardLocation location, CardType type, int controller = 0)
        {
            var cards = await GetCardsInLocation(location, controller);
            return cards.Any(card => card.HasType(type));
        }

        /// <summary>
        /// Gets all cards in a specific location with a specific type
        /// </summary>
        /// <param name="location">Location to check (e.g., CardLocation.Hand, CardLocation.MonsterZone)</param>
        /// <param name="type">Card type to filter by (e.g., CardType.Monster)</param>
        /// <param name="controller">Controller of the cards (0 for player, 1 for opponent)</param>
        /// <returns>List of DynamicCard with the specified type in the specified location</returns>
        public static async Task<List<DynamicCard>> GetCardsWithTypeInLocation(CardLocation location, CardType type, int controller = 0)
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
        public static async Task<DynamicCard> GetStrongestMonsterInLocation(CardLocation location, int controller = 0, bool useDefense = false)
        {
            var monsters = await GetCardsWithTypeInLocation(location, CardType.Monster, controller);
            return monsters
                .Where(card => card.IsFaceup() && card.IsAttack()) // Only consider face-up monsters
                .OrderByDescending(card => useDefense ? card.GetRealPower() : card.Attack)
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets the strongest monster card in a specific location (based on attack or defense)
        /// </summary>
        /// <param name="location">Location to check (e.g., CardLocation.MonsterZone)</param>
        /// <param name="controller">Controller of the cards (0 for player, 1 for opponent)</param>
        /// <param name="useDefense">If true, compares defense instead of attack</param>
        /// <returns>DynamicCard with the highest attack or defense in the specified location</returns>
        public static async Task<IEnumerable<DynamicCard>> GetMonsterInLocation(CardLocation location, int controller = 0)
        {
            var monsters = await GetCardsWithTypeInLocation(location, CardType.Monster, controller);
            return monsters
                .Where(card => card.IsFaceup())
                .OrderByDescending(card => card.ID)
                .ToList();
        }

        /// <summary>
        /// Checks if a card with a specific ID exists in a given location
        /// </summary>
        /// <param name="cardId">ID of the card to search for</param>
        /// <param name="location">Location to check (e.g., CardLocation.Hand)</param>
        /// <param name="controller">Controller of the cards (0 for player, 1 for opponent)</param>
        /// <returns>True if a card with the specified ID is found</returns>
        public static async Task<bool> HasCardWithIdInLocation(int cardId, CardLocation location, int controller = 0)
        {
            var cards = await GetCardsInLocation(location, controller);
            return cards.TryGetByID(cardId, out _);
        }

        /// <summary>
        /// Gets the card count in the player's side MonsterZones
        /// </summary>
        public static int GetOwnMonsterZoneCardCount()
        {
            return CountCardsInZones(OwnSide.MonsterZones);
        }

        /// <summary>
        /// Gets the card count in the opponent's MonsterZones
        /// </summary>
        public static int GetEnemyMonsterZoneCardCount()
        {
            return CountCardsInZones(EnemySide.MonsterZones);
        }

        /// <summary>
        /// Gets the card count in the player's SpellZones
        /// </summary>
        public static int GetOwnSpellZoneCardCount()
        {
            return CountCardsInZones(OwnSide.SpellZones);
        }

        /// <summary>
        /// Gets the card count in the opponent's SpellZones
        /// </summary>
        public static int GetEnemySpellZoneCardCount()
        {
            return CountCardsInZones(EnemySide.SpellZones);
        }

        /// <summary>
        /// Counts the total number of cards in a zone array
        /// </summary>
        private static int CountCardsInZones(CardArray[] zones)
        {
            if (zones == null) return 0;
            return zones.Sum(zone => zone.Cards?.Length ?? 0);
        }

        /// <summary>
        /// Gets the card count in a specific zone on the player's side
        /// </summary>
        public static int GetOwnZoneCardCount(int zoneIndex, bool isMonsterZone = true)
        {
            var zones = isMonsterZone ? OwnSide.MonsterZones : OwnSide.SpellZones;
            return zones != null && zoneIndex < zones.Length ? zones[zoneIndex].Cards?.Length ?? 0 : 0;
        }

        /// <summary>
        /// Gets the card count in a specific zone on the opponent's side
        /// </summary>
        public static int GetEnemyZoneCardCount(int zoneIndex, bool isMonsterZone = true)
        {
            var zones = isMonsterZone ? EnemySide.MonsterZones : EnemySide.SpellZones;
            return zones != null && zoneIndex < zones.Length ? zones[zoneIndex].Cards?.Length ?? 0 : 0;
        }

        /// <summary>
        /// Checks if the strongest face-up monster on the field belongs to the opponent
        /// </summary>
        /// <param name="location">Location to check (default is CardLocation.MonsterZone)</param>
        /// <param name="useDefense">If true, compares defense instead of attack (not used in this implementation)</param>
        /// <returns>True if the opponent's face-up monster has the highest attack or if the opponent has monsters and the bot has none</returns>
        public static async Task<bool> IsFaceUpAttackStrongestMonsterOpponent(CardLocation location = CardLocation.MonsterZone)
        {
            // Step 1: Check monster counts
            int botMonsterCount = GetEnemyMonsterZoneCardCount();
            int opponentMonsterCount = GetOwnMonsterZoneCardCount();

            Console.WriteLine($"opponentMonsterCount => {opponentMonsterCount}");
            Console.WriteLine($"botMonsterCount => {botMonsterCount}");

            if (opponentMonsterCount > 0 && botMonsterCount == 0)
            {
                Console.WriteLine("Opponent has monsters and bot has none. Activating spell.");
                return true;
            }

            // Step 2: Get lists of face-up + attack face-up monsters
            var opponentMonsters = await GetCardsWithTypeInLocation(location, CardType.Monster, controller: 0);
            var botMonsters = await GetCardsWithTypeInLocation(location, CardType.Monster, controller: 1);

            var opponentFaceUpMonsters = opponentMonsters.Where(card => card.IsFaceup() && card.IsAttack()).ToList();
            var botFaceUpMonsters = botMonsters.Where(card => card.IsFaceup() && card.IsAttack()).ToList();

            // Step 3: Calculate maximum attack values
            int opponentMaxAttack = opponentFaceUpMonsters.Any() ? opponentFaceUpMonsters.Max(card => card.Attack) : 0;
            int botMaxAttack = botFaceUpMonsters.Any() ? botFaceUpMonsters.Max(card => card.Attack) : 0;

            // Step 4: Compare max attack values
            Console.WriteLine($"Opponent Max Attack: {opponentMaxAttack}, Bot Max Attack: {botMaxAttack}");
            return opponentMaxAttack >= botMaxAttack;
        }
    }
}