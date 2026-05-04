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
            return await RefreshFieldAsync(FieldPerspective.Player);
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
            return await GetFieldCardAsync(FieldPerspective.Player, controller, (CardLocation)location, sequence);
        }

        /// <summary>
        /// Gets all cards in a specific zone in the Player's field.
        /// </summary>
        /// <param name="location">Location to check (e.g. MonsterZone, SpellZone).</param>
        /// <param name="isOwnSide">If true, checks the Player's side; if false, checks the opponent's side.</param>
        /// <returns>List of DynamicCards in the specified zone.</returns>
        public async Task<IEnumerable<DynamicCard>> GetPlayerCardsInZone(CardLocation location, bool isOwnSide = true)
        {
            return await GetCardsInZoneAsync(FieldPerspective.Player, location, isOwnSide);
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
        /// Gets all cards in a specific location on the player's or opponent's side.
        /// </summary>
        /// <param name="location">Location to check (e.g., CardLocation.Hand, CardLocation.MonsterZone).</param>
        /// <param name="controller">Controller of the cards (0 for player, 1 for opponent [BOT]).</param>
        /// <returns>List of DynamicCard in the specified location.</returns>
        public async Task<IEnumerable<DynamicCard>> GetPlayerCardsInLocation(CardLocation location, int controller = 0)
        {
            return await GetCardsInLocationAsync(FieldPerspective.Player, location, controller);
        }

        /// <summary>
        /// Gets all cards in a specific location with a specific type.
        /// </summary>
        /// <param name="location">Location to check (e.g., CardLocation.Hand, CardLocation.MonsterZone).</param>
        /// <param name="type">Card type to filter by (e.g., CardType.Monster).</param>
        /// <param name="controller">Controller of the cards (0 for player, 1 for opponent).</param>
        /// <returns>List of DynamicCard with the specified type in the specified location.</returns>
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
            var monsters = await GetPlayerCardsWithTypeInLocation(location, CardType.Monster, controller);
            return monsters
                .Where(card => card.IsFacedown() || card.IsFaceup())
                .OrderByDescending(card => card.ID)
                .ToList();
        }

        /// <summary>
        /// Gets the card count in the player's SpellZone.
        /// </summary>
        public async Task<int> GetPlayerSpellZoneCount(CardLocation location = CardLocation.SpellZone)
        {
            var playerSpells = await GetPlayerSpellZoneInLocation(location);
            return playerSpells.Count();
        }

        /// <summary>
        /// Gets the card count in the player's MonsterZone.
        /// </summary>
        public async Task<int> GetPlayerMonsterZoneCount(CardLocation location = CardLocation.MonsterZone)
        {
            var playerMonsters = await GetPlayerMonsterZoneInLocation(location);
            return playerMonsters.Count();
        }

        /// <summary>
        /// Gets a specific card's information from the player's perspective.
        /// </summary>
        /// <param name="controller">Controller of the card (0 for player, 1 for opponent).</param>
        /// <param name="location">Location of the card (e.g., CardLocation.Hand, CardLocation.MonsterZone).</param>
        /// <param name="sequence">Sequence of the card in the given location.</param>
        /// <returns>DynamicCard containing the card's information.</returns>
        public async Task<DynamicCard> GetPlayerFieldCardInfo(int controller, CardLocation location, int sequence)
        {
            return await GetFieldCardAsync(FieldPerspective.Player, controller, location, sequence);
        }

        public async Task<int> GetPlayerSpellsCount()
        {
            var spellsCards = await GetPlayerCardsInLocation(CardLocation.SpellZone);
            return spellsCards.Count(card => card.ID != 0 && card.Cover == 0);
        }

        public async Task<int> GetPlayerMonstersCount()
        {
            var monstersCards = await GetPlayerCardsInLocation(CardLocation.MonsterZone);
            return monstersCards.Count(card => card.ID != 0 && (CardPosition)card.Position == CardPosition.FaceUp);
        }

        public async Task<IEnumerable<DynamicCard>> GetPlayerCardsInHand()
        {
            return await GetPlayerCardsInLocation(CardLocation.Hand);
        }

        public async Task<IEnumerable<DynamicCard>> GetPlayerSpellsCardsInSpellZone()
        {
            return await GetPlayerCardsInZone(CardLocation.SpellZone);
        }

        public async Task<IEnumerable<DynamicCard>> GetPlayerMonstersCardsInMonsterZone()
        {
            return await GetPlayerCardsInZone(CardLocation.MonsterZone);
        }
    }
}
