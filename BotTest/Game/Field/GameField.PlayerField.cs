using OmegaBotWrapper;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTest.Game.Field
{
    public partial class GameField
    {
        /// <summary>
        /// Obtém informações detalhadas do campo do Player (usando perspectiva de trapaça).
        /// </summary>
        /// <returns>Estrutura FieldInfo com os dados do campo do Player.</returns>
        public async Task<FieldInfo> GetPlayerField()
        {
            _fieldInfo = await DuelBotWrapper.GetPlayerFieldInfo();
            return _fieldInfo;
        }

        /// <summary>
        /// Obtém informações de uma carta específica no campo do Player (usando perspectiva de trapaça).
        /// </summary>
        /// <param name="controller">Controlador da carta (0 = Player, 1 = Bot).</param>
        /// <param name="location">Localização da carta (ex.: MonsterZone, SpellZone).</param>
        /// <param name="sequence">Sequência da carta na zona especificada.</param>
        /// <returns>Estrutura DynamicCard com os dados da carta.</returns>
        public async Task<DynamicCard> GetPlayerFieldCard(int controller, int location, int sequence)
        {
            return await DuelBotWrapper.GetPlayerFieldCardInfo(controller, location, sequence);
        }

        /// <summary>
        /// Obtém todas as cartas em uma zona específica no campo do Player.
        /// </summary>
        /// <param name="location">Localização a ser verificada (ex.: MonsterZone, SpellZone).</param>
        /// <param name="isOwnSide">Se true, verifica o lado do Player; se false, verifica o lado do oponente.</param>
        /// <returns>Lista de DynamicCard na zona especificada.</returns>
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
        /// Verifica se há cartas viradas para cima (FaceUp) em uma zona específica no campo do Player.
        /// </summary>
        /// <param name="location">Localização a ser verificada (ex.: MonsterZone, SpellZone).</param>
        /// <param name="isOwnSide">Se true, verifica o lado do Player; se false, verifica o lado do oponente.</param>
        /// <returns>True se houver cartas FaceUp na zona, false caso contrário.</returns>
        public async Task<bool> HasFaceUpCardsInPlayerZone(CardLocation location, bool isOwnSide = true)
        {
            var cards = await GetPlayerCardsInZone(location, isOwnSide);
            return cards.Any(card => card.IsFaceup());
        }

        /// <summary>
        /// Obtém a carta com maior poder de ataque em uma zona específica no campo do Player.
        /// </summary>
        /// <param name="location">Localização a ser verificada (ex.: MonsterZone).</param>
        /// <param name="isOwnSide">Se true, verifica o lado do Player; se false, verifica o lado do oponente.</param>
        /// <returns>DynamicCard com o maior poder de ataque ou default se não houver cartas.</returns>
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
                                var card = await GetPlayerFieldCard(controller, (int)location, sequence);
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
                            Console.WriteLine($"cardZone {cardZone}");

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

        public async Task<IEnumerable<DynamicCard>> GetPlayerSpellZoneInLocation(CardLocation location, int controller = 0)
        {
            var spells = await GetPlayerCardsWithTypeInLocation(location, CardType.Spell, controller);
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
    }
}
