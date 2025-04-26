using BotTest.Utils;
using OmegaBotWrapper;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.Struct;

namespace BotTest.Game.Field
{
    public partial class GameField
    {
        /// <summary>
        /// Get detailed information from the Player field (using cheating perspective).
        /// </summary>
        /// <returns>FieldInfo structure with the Player field data.</returns>
        public async Task<FieldInfo> GetPlayerField()
        {
            _fieldInfo = await DuelBotWrapper.GetPlayerFieldInfo();
            return _fieldInfo;
        }

        /// <summary>
        /// Gets information about a specific card in the Player field (using cheating perspective).
        /// </summary>
        /// <param name="controller">Card controller (0 = Player, 1 = Bot).</param>
        /// <param name="location">Location of the card (e.g. MonsterZone, SpellZone).</param>
        /// <param name="sequence">Sequence of the card in the specified zone.</param>
        /// <returns>DynamicCard structure with the card data.</returns>
        public async Task<DynamicCard> GetPlayerFieldCard(int controller, int location, int sequence)
        {
            return await DuelBotWrapper.GetPlayerFieldCardInfo(controller, location, sequence);
        }

        /// <summary>
        /// Gets all cards in a specific zone in the Player's field.
        /// </summary>
        /// <param name="location">Location to check (e.g. MonsterZone, SpellZone).</param>
        /// <param name="isOwnSide">If true, checks the Player's side; if false, checks the opponent's side.</param>
        /// <returns>List of DynamicCards in the specified zone.</returns>
        public async Task<List<DynamicCard>> GetPlayerCardsInZone(CardLocation location, bool isOwnSide = true)
        {
            var field = await GetPlayerField();
            var side = isOwnSide ? field.OwnSide : field.EnemySide;
            var cards = new List<DynamicCard>();
            int controller = isOwnSide ? 0 : 1; // 0 = Player, 1 = Bot

            if (location == CardLocation.MonsterZone)
            {
                for (int seq = 0; seq < side.MonsterZones.Length; seq++)
                {
                    foreach (var cardId in side.MonsterZones[seq].Cards)
                    {
                        if (cardId > 0)
                        {
                            var card = await GetPlayerFieldCard(controller, (int)CardLocation.MonsterZone, seq);
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
                            var card = await GetPlayerFieldCard(controller, (int)CardLocation.SpellZone, seq);
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
                        var card = await GetPlayerFieldCard(controller, (int)CardLocation.Hand, 0);
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
                        var card = await GetPlayerFieldCard(controller, (int)CardLocation.Grave, 0);
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
                        var card = await GetPlayerFieldCard(controller, (int)CardLocation.Removed, 0);
                        if (card.ID != 0) cards.Add(card);
                    }
                }
            }

            return cards;
        }

        /// <summary>
        /// Checks for face-up cards in a specific zone on the Player's field.
        /// </summary>
        /// <param name="location">Location to check (e.g. MonsterZone, SpellZone).</param>
        /// <param name="isOwnSide">If true, checks the Player's side; if false, checks the opponent's side.</param>
        /// <returns>True if there are FaceUp cards in the zone, false otherwise.</returns>
        public async Task<bool> HasFaceUpCardsInPlayerZone(CardLocation location, bool isOwnSide = true)
        {
            var cards = await GetPlayerCardsInZone(location, isOwnSide);
            return cards.Any(card => card.IsFaceup());
        }

        /// <summary>
        /// Gets the card with the highest attack power in a specific zone on the Player's field.
        /// </summary>
        /// <param name="location">Location to check (e.g. MonsterZone).</param>
        /// <param name="isOwnSide">If true, checks the Player's side; if false, checks the opponent's side.</param>
        /// <returns>DynamicCard with the highest attack power or default if there are no cards.</returns>
        public async Task<DynamicCard> GetStrongestPlayerCardInZone(CardLocation location, bool isOwnSide = true)
        {
            var cards = await GetPlayerCardsInZone(location, isOwnSide);
            return cards.OrderByDescending(card => card.GetRealPower()).FirstOrDefault();
        }

        /// <summary>
        /// Gets all cards in a specific location on the player's or opponent's side
        /// </summary>
        /// <param name="location">Location to check (e.g., CardLocation.Hand, CardLocation.MonsterZone)</param>
        /// <param name="controller">Controller of the cards (0 for player, 1 for opponent [BOT])</param>
        /// <returns>List of DynamicCard in the specified location</returns>
        public async Task<List<DynamicCard>> GetPlayerCardsInLocation(CardLocation location, int controller = 0)
        {
            await GetPlayerField();
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
                        ConsoleUtils.PrintColored("cardIds[sequence] => " + cardIds[sequence]);

                        if (cardIds[sequence] > 0) // Checks for a potentially valid card
                        {
                            try
                            {
                                var card = await GetPlayerFieldCard(controller, (int)location, sequence);
                                if (card.ID > 0) // Ensures that the letter is valid
                                    cards.Add(card);
                            }
                            catch (Exception ex) when (ex.Message.Contains("404") || ex.Message.Contains("No card found"))
                            {
                                ConsoleUtils.PrintColored("******** Ignore 404 errors (no card at coordinates) ********");
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
                            ConsoleUtils.PrintColored($"cardZone {cardZone}");

                            if (cardZone > 0)
                            {
                                int sequence = zoneIndex;
                                try
                                {
                                    var card = await GetPlayerFieldCard(controller, (int)location, sequence);
                                    if (card.ID > 0)
                                        cards.Add(card);
                                }
                                catch (Exception ex) when (ex.Message.Contains("404") || ex.Message.Contains("No card found"))
                                {
                                    ConsoleUtils.PrintColored("******** Ignore 404 errors (no card at coordinates) ********");
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
        public async Task<List<DynamicCard>> GetPlayerCardsWithTypeInLocation(CardLocation location, CardType type, int controller = 0)
        {
            var cards = await GetPlayerCardsInLocation(location, controller);
            return cards.Where(card => card.HasType(type)).ToList();
        }

        public async Task<IEnumerable<DynamicCard>> GetPlayerSpellZoneInLocation(CardLocation location, int controller = 0)
        {
            var spells = await GetPlayerCardsWithTypeInLocation(location, CardType.Spell, controller);
            return spells
                .Where(card => card.IsFacedown() || card.IsFaceup())
                .OrderByDescending(card => card.ID)
                .ToList();
        }

        public async Task<IEnumerable<DynamicCard>> GetPlayerMonsterZoneInLocation(CardLocation location, int controller = 0)
        {
            var spells = await GetPlayerCardsWithTypeInLocation(location, CardType.Monster, controller);
            return spells
                .Where(card => card.IsFacedown() || card.IsFaceup())
                .OrderByDescending(card => card.ID)
                .ToList();
        }

        /// <summary>
        /// Gets the card count in the player's SpellZone
        /// </summary>
        public async Task<int> GetPlayerSpellZoneCount(CardLocation location = CardLocation.SpellZone)
        {
            await GetPlayerField();

            IEnumerable<DynamicCard> playerSpells = await GetPlayerSpellZoneInLocation(location);
            int playerSpellCount = playerSpells.Count();
            return playerSpellCount;
        }

        /// <summary>
        /// Gets the card count in the player's SpellZone
        /// </summary>
        public async Task<int> GetPlayerMonsterZoneCount(CardLocation location = CardLocation.MonsterZone)
        {
            await GetPlayerField();

            IEnumerable<DynamicCard> playerMonsters = await GetPlayerMonsterZoneInLocation(location);
            int playerMonstersCount = playerMonsters.Count();
            return playerMonstersCount;
        }

        /// <summary>
        /// Gets a specific card's information from the player's perspective
        /// </summary>
        /// <param name="controller">Controller of the card (0 for player, 1 for opponent)</param>
        /// <param name="location">Location of the card (e.g., CardLocation.Hand, CardLocation.MonsterZone)</param>
        /// <param name="sequence">Sequence of the card in the given location</param>
        /// <returns>DynamicCard containing the card's information</returns>
        public async Task<DynamicCard> GetPlayerFieldCardInfo(int controller, CardLocation location, int sequence)
        {
            return await DuelBotWrapper.GetPlayerFieldCardInfo(controller, (int)location, sequence);
        }

        public async Task<int> GetPlayerSpellsCount()
        {
            var spellsCards = await GetPlayerCardsInLocation(CardLocation.SpellZone);
            return spellsCards.Where(card => ((card.ID != 0) && (card.Cover == 0))).Count();
        }
    }
}
