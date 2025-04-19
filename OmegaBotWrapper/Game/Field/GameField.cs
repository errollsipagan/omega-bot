using OmegaBotWrapper.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        /// Gets the card count in the player's side MonsterZones
        /// </summary>
        public static int GetOwnMonsterZoneCardCount()
        {
            return CountCardsInZones(OwnSide.MonsterZones);
        }

        /// <summary>
        /// Obtém a contagem de cartas nas MonsterZones do lado do oponente
        /// </summary>
        public static int GetEnemyMonsterZoneCardCount()
        {
            return CountCardsInZones(EnemySide.MonsterZones);
        }

        /// <summary>
        /// Gets the card count in the opponent's MonsterZones
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
    }
}
