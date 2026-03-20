using OmegaBotWrapper;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.Struct;

namespace BotTest.Game.Field
{
    public partial class GameField
    {
        public async Task<List<DynamicCard>> GetBotHandCards()
        {
            return (await GetCardsInLocation(CardLocation.Hand)).ToList();
        }

        public async Task<List<DynamicCard>> GetBotGraveyardCards()
        {
            return (await GetCardsInLocation(CardLocation.Grave)).ToList();
        }

        public async Task<List<DynamicCard>> GetBotMonsterCards(bool onlyFaceUp = false)
        {
            var monsters = (await GetCardsInLocation(CardLocation.MonsterZone)).ToList();
            return onlyFaceUp ? monsters.Where(card => card.IsFaceup()).ToList() : monsters;
        }

        public async Task<List<DynamicCard>> GetEnemyMonsterCards(bool onlyFaceUp = false)
        {
            var monsters = (await GetPlayerCardsInLocation(CardLocation.MonsterZone)).ToList();
            return onlyFaceUp ? monsters.Where(card => card.IsFaceup()).ToList() : monsters;
        }

        public async Task<List<DynamicCard>> GetEnemySpellCards(bool onlyFaceUp = false)
        {
            var spells = (await GetPlayerCardsInLocation(CardLocation.SpellZone)).ToList();
            return onlyFaceUp ? spells.Where(card => card.IsFaceup()).ToList() : spells;
        }

        public async Task<DynamicCard> GetBestEnemyMonster(bool onlyFaceUp = false)
        {
            var monsters = await GetEnemyMonsterCards(onlyFaceUp);
            return monsters
                .OrderByDescending(card => card.IsFaceup())
                .ThenByDescending(card => card.IsAttack())
                .ThenByDescending(card => card.GetRealPower())
                .FirstOrDefault();
        }

        public async Task<DynamicCard> GetBestEnemySpell(bool onlyFaceUp = false)
        {
            var spells = await GetEnemySpellCards(onlyFaceUp);
            return spells
                .OrderByDescending(card => card.IsFaceup())
                .ThenByDescending(card => card.HasType(CardType.Continuous) || card.HasType(CardType.Field) || card.HasType(CardType.Equip))
                .ThenByDescending(card => card.ID)
                .FirstOrDefault();
        }

        public async Task<DynamicCard> GetBestEnemyCard(bool onlyFaceUp = false)
        {
            var monster = await GetBestEnemyMonster(onlyFaceUp);
            if (monster.ID > 0)
            {
                return monster;
            }

            return await GetBestEnemySpell(onlyFaceUp);
        }

        public async Task<bool> HasBotHandCard(params int[] ids)
        {
            var hand = await GetBotHandCards();
            return hand.Any(card => card.IsCode(ids));
        }

        public async Task<bool> HasBotGraveyardCard(params int[] ids)
        {
            var graveyard = await GetBotGraveyardCards();
            return graveyard.Any(card => card.IsCode(ids));
        }

        public async Task<int> CountBotMonsters(Func<DynamicCard, bool> predicate)
        {
            var monsters = await GetBotMonsterCards();
            return monsters.Count(predicate);
        }
    }
}
