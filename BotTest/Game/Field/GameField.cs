using OmegaBotWrapper.Enum;
using OmegaBotWrapper.Struct;
using OmegaBotWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotTest.Game.Field
{
    /// <summary>
    /// Classe para gerenciar e verificar o estado do campo do Bot e do Player usando FieldInfo.
    /// </summary>
    public class GameField
    {
        private readonly DuelBotWrapper _botWrapper;

        public GameField(DuelBotWrapper botWrapper)
        {
            _botWrapper = botWrapper ?? throw new ArgumentNullException(nameof(botWrapper));
        }

        /// <summary>
        /// Obtém informações detalhadas do campo do Bot.
        /// </summary>
        /// <returns>Estrutura FieldInfo com os dados do campo do Bot.</returns>
        public async Task<FieldInfo> GetBotField()
        {
            return await _botWrapper.GetBotFieldInfo();
        }

        /// <summary>
        /// Obtém informações detalhadas do campo do Player (usando perspectiva de trapaça).
        /// </summary>
        /// <returns>Estrutura FieldInfo com os dados do campo do Player.</returns>
        public async Task<FieldInfo> GetPlayerField()
        {
            return await DuelBotWrapper.GetPlayerFieldInfo();
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
        /// Obtém o número de cartas em uma zona específica no campo do Player.
        /// </summary>
        /// <param name="location">Localização a ser verificada (ex.: MonsterZone, SpellZone).</param>
        /// <param name="isOwnSide">Se true, verifica o lado do Player; se false, verifica o lado do oponente.</param>
        /// <returns>Número de cartas na zona especificada.</returns>
        public async Task<int> GetPlayerZoneCardCount(CardLocation location, bool isOwnSide = true)
        {
            var field = await GetPlayerField();
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
    }

}
