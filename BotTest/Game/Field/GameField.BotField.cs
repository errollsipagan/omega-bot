using OmegaBotWrapper.Enum;
using OmegaBotWrapper.Struct;
using OmegaBotWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OmegaBotWrapper.Game.Field;

namespace BotTest.Game.Field
{
    /// <summary>
    /// Classe para gerenciar e verificar o estado do campo do Bot e do Player usando FieldInfo.
    /// </summary>
    public partial class GameField
    {
        /// <summary>
        /// Obtém informações detalhadas do campo do Bot.
        /// </summary>
        /// <returns>Estrutura FieldInfo com os dados do campo do Bot.</returns>
        public async Task<FieldInfo> GetBotField()
        {
            _fieldInfo = await _botWrapper.GetBotFieldInfo();
            return _fieldInfo;
        }

        /// <summary>
        /// Obtém informações de uma carta específica no campo do Bot.
        /// </summary>
        /// <param name="controller">Controlador da carta (0 = Player, 1 = Bot).</param>
        /// <param name="location">Localização da carta (ex.: MonsterZone, SpellZone).</param>
        /// <param name="sequence">Sequência da carta na zona especificada.</param>
        /// <returns>Estrutura DynamicCard com os dados da carta.</returns>
        public async Task<DynamicCard> GetBotFieldCard(int controller, int location, int sequence)
        {
            Console.WriteLine($"controller: {controller} / location: {location} / sequence: {sequence}");
            return await _botWrapper.GetBotFieldCardInfo(controller, location, sequence);
        }

        /// <summary>
        /// Verifica se há cartas em uma zona específica no campo do Bot.
        /// </summary>
        /// <param name="location">Localização a ser verificada (ex.: MonsterZone, SpellZone).</param>
        /// <param name="isOwnSide">Se true, verifica o lado do Bot; se false, verifica o lado do oponente.</param>
        /// <returns>True se houver cartas na zona especificada, false caso contrário.</returns>
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
        /// Verifica se há cartas em uma zona específica no campo do Player.
        /// </summary>
        /// <param name="location">Localização a ser verificada (ex.: MonsterZone, SpellZone).</param>
        /// <param name="isOwnSide">Se true, verifica o lado do Player; se false, verifica o lado do oponente.</param>
        /// <returns>True se houver cartas na zona especificada, false caso contrário.</returns>
        public async Task<bool> HasCardsInPlayerZone(CardLocation location, bool isOwnSide = true)
        {
            var field = await GetPlayerField();
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
        /// Obtém todas as cartas em uma zona específica no campo do Bot.
        /// </summary>
        /// <param name="location">Localização a ser verificada (ex.: MonsterZone, SpellZone).</param>
        /// <param name="isOwnSide">Se true, verifica o lado do Bot; se false, verifica o lado do oponente.</param>
        /// <returns>Lista de DynamicCard na zona especificada.</returns>
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
        /// Verifica se há cartas viradas para cima (FaceUp) em uma zona específica no campo do Bot.
        /// </summary>
        /// <param name="location">Localização a ser verificada (ex.: MonsterZone, SpellZone).</param>
        /// <param name="isOwnSide">Se true, verifica o lado do Bot; se false, verifica o lado do oponente.</param>
        /// <returns>True se houver cartas FaceUp na zona, false caso contrário.</returns>
        public async Task<bool> HasFaceUpCardsInBotZone(CardLocation location, bool isOwnSide = true)
        {
            var cards = await GetBotCardsInZone(location, isOwnSide);
            return cards.Any(card => card.IsFaceup());
        }

        /// <summary>
        /// Obtém a carta com maior poder de ataque em uma zona específica no campo do Bot.
        /// </summary>
        /// <param name="location">Localização a ser verificada (ex.: MonsterZone).</param>
        /// <param name="isOwnSide">Se true, verifica o lado do Bot; se false, verifica o lado do oponente.</param>
        /// <returns>DynamicCard com o maior poder de ataque ou default se não houver cartas.</returns>
        public async Task<DynamicCard> GetStrongestBotCardInZone(CardLocation location, bool isOwnSide = true)
        {
            var cards = await GetBotCardsInZone(location, isOwnSide);
            return cards.OrderByDescending(card => card.GetRealPower()).FirstOrDefault();
        }
        
        /// <summary>
        /// Verifica se uma carta específica (por ID) está presente em uma zona no campo do Bot.
        /// </summary>
        /// <param name="cardId">ID da carta a ser verificada.</param>
        /// <param name="location">Localização a ser verificada (ex.: MonsterZone, SpellZone).</param>
        /// <param name="isOwnSide">Se true, verifica o lado do Bot; se false, verifica o lado do oponente.</param>
        /// <returns>True se a carta estiver presente, false caso contrário.</returns>
        public async Task<bool> IsCardInBotZone(int cardId, CardLocation location, bool isOwnSide = true)
        {
            var cards = await GetBotCardsInZone(location, isOwnSide);
            return cards.Any(card => card.IsCode(cardId));
        }

        /// <summary>
        /// Verifica se uma carta específica (por ID) está presente em uma zona no campo do Player.
        /// </summary>
        /// <param name="cardId">ID da carta a ser verificada.</param>
        /// <param name="location">Localização a ser verificada (ex.: MonsterZone, SpellZone).</param>
        /// <param name="isOwnSide">Se true, verifica o lado do Player; se false, verifica o lado do oponente.</param>
        /// <returns>True se a carta estiver presente, false caso contrário.</returns>
        public async Task<bool> IsCardInPlayerZone(int cardId, CardLocation location, bool isOwnSide = true)
        {
            var cards = await GetPlayerCardsInZone(location, isOwnSide);
            return cards.Any(card => card.IsCode(cardId));
        }

        /// <summary>
        /// Obtém o número de cartas em uma zona específica no campo do Bot.
        /// </summary>
        /// <param name="location">Localização a ser verificada (ex.: MonsterZone, SpellZone).</param>
        /// <param name="isOwnSide">Se true, verifica o lado do Bot; se false, verifica o lado do oponente.</param>
        /// <returns>Número de cartas na zona especificada.</returns>
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
        /// Obtém os pontos de vida (LifePoints) do Bot.
        /// </summary>
        /// <param name="isOwnSide">Se true, retorna os pontos de vida do Bot; se false, do oponente.</param>
        /// <returns>Pontos de vida do lado especificado.</returns>
        public async Task<int> GetBotLifePoints(bool isOwnSide = true)
        {
            var field = await GetBotField();
            var side = isOwnSide ? field.OwnSide : field.EnemySide;
            return side.LifePoints;
        }

        /// <summary>
        /// Obtém os pontos de vida (LifePoints) do Player.
        /// </summary>
        /// <param name="isOwnSide">Se true, retorna os pontos de vida do Player; se false, do oponente.</param>
        /// <returns>Pontos de vida do lado especificado.</returns>
        public async Task<int> GetPlayerLifePoints(bool isOwnSide = true)
        {
            var field = await GetPlayerField();
            var side = isOwnSide ? field.OwnSide : field.EnemySide;
            return side.LifePoints;
        }

        /// <summary>
        /// Verifica se o Bot está sob ataque.
        /// </summary>
        /// <param name="isOwnSide">Se true, verifica o lado do Bot; se false, verifica o lado do oponente.</param>
        /// <returns>True se o lado especificado está sob ataque, false caso contrário.</returns>
        public async Task<bool> IsBotUnderAttack(bool isOwnSide = true)
        {
            var field = await GetBotField();
            var side = isOwnSide ? field.OwnSide : field.EnemySide;
            return side.UnderAttack;
        }

        /// <summary>
        /// Verifica se o Player está sob ataque.
        /// </summary>
        /// <param name="isOwnSide">Se true, verifica o lado do Player; se false, verifica o lado do oponente.</param>
        /// <returns>True se o lado especificado está sob ataque, false caso contrário.</returns>
        public async Task<bool> IsPlayerUnderAttack(bool isOwnSide = true)
        {
            var field = await GetPlayerField();
            var side = isOwnSide ? field.OwnSide : field.EnemySide;
            return side.UnderAttack;
        }

        /// <summary>
        /// Obtém o monstro em batalha no campo do Bot, se houver.
        /// </summary>
        /// <param name="isOwnSide">Se true, verifica o lado do Bot; se false, verifica o lado do oponente.</param>
        /// <returns>DynamicCard do monstro em batalha ou default se não houver.</returns>
        public async Task<DynamicCard> GetBotBattlingMonster(bool isOwnSide = true)
        {
            var field = await GetBotField();
            var side = isOwnSide ? field.OwnSide : field.EnemySide;
            return side.HasBattlingMonster ? side.BattlingMonster : default;
        }

        /// <summary>
        /// Obtém o monstro em batalha no campo do Player, se houver.
        /// </summary>
        /// <param name="isOwnSide">Se true, verifica o lado do Player; se false, verifica o lado do oponente.</param>
        /// <returns>DynamicCard do monstro em batalha ou default se não houver.</returns>
        public async Task<DynamicCard> GetPlayerBattlingMonster(bool isOwnSide = true)
        {
            var field = await GetPlayerField();
            var side = isOwnSide ? field.OwnSide : field.EnemySide;
            return side.HasBattlingMonster ? side.BattlingMonster : default;
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

        /// <summary>
        /// Gets all cards in a specific location on the player's or opponent's side
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
                        Console.WriteLine("cardIds[sequence] => " + cardIds[sequence]);

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
                            Console.WriteLine($"cardZone {cardZone}");

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
        /// Counts the total number of cards in a zone array
        /// </summary>
        private int CountCardsInZones(CardArray[] zones)
        {
            if (zones == null) return 0;
            return zones.Sum(zone => zone.Cards?.Length ?? 0);
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

            Console.WriteLine($"botMonsterCount => {botMonsterCount}");
            Console.WriteLine($"playerMonsterCount => {playerMonsterCount}");

            if (playerMonsterCount > 0 && botMonsterCount == 0)
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
            Console.WriteLine($"Bot Max Attack: {botMaxAttack}, Opponent Max Attack: {opponentMaxAttack}");
            return botMaxAttack >= opponentMaxAttack;
        }
    }

}
