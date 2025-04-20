using System;
using System.Collections.Generic;
using System.Linq;

namespace OmegaBotWrapper.Decks
{
    /// <summary>
    /// Enum to identify available decks.
    /// </summary>
    public enum DeckId
    {
        BotTest,
        BlueEyes,
        DarkMagician
        // Add more deck IDs here as needed, e.g., BlueEyes, DarkMagician
    }

    /// <summary>
    /// Manages deck names and hashes, providing access to deck data and options.
    /// </summary>
    public static class DeckManager
    {
        private static readonly Dictionary<DeckId, (string DeckName, string DeckHash)> _decks = new()
        {
            {
                DeckId.BotTest,
                (
                    DeckName: "BotTest",
                    DeckHash: "02KwdIhg0fCxZ8hOv8cAomHsxSvDWYVuLmDKvDOfBcRn8xdhhOHJP8vh+NyMiUww7LrgAhxzXrgFx1942eGYIcsMjj+6tTPDMMguEF6jH8oIwyA3XBINZgLRIHcBAA=="
                )
            },
            {
                DeckId.BlueEyes,
                (
                    DeckName: "Blue-Eyes",
                    DeckHash: "M2cwO76V6eVub6b5v61ZkkqdWEH04pXhrEuBeBkQg+RA+GbvJcYKXltmxkQBiNqaHUww3PzgIAsMG94RZHTfU8sAw9sbfJl3APFyDWvGxXsmsoJwQgYv49YyPsa1lVoM16QPMqb32bJ0zWJgTsrazAzSD8Igt4DwszoFhrNO4aymFyYww3CwWQETDDPFNMHx5N5gprwiXYbjC68yZ0UrMoL8AQA="
                )
            }
            ,
            {
                DeckId.DarkMagician,
                (
                    DeckName: "Dark Magician",
                    DeckHash: "M2Pg6L/AlCiRw/AwzIVp1wVGlhU/LzEKqaiygMQnWrEyXNjkyAhiz3Y/zrxhEyPDui9nmDKmngfTETNzmUD6uC48A8vnWUszsMj2MIPYvzYKsTjHpbLc3fSYESQP0vN6QiMrSD2I/eaHHgPIXpDc4ycVDLFnXzDA1IPsAcmB7Nixby5T0EtB1msT/jGtEnRkCv0SwfCQ9T7rTqA4iG95KoHlMZAPYlfNrWE6b17H8gjIB+kFAA=="
                )
            }
            // Add more decks here, e.g.:
            // { DeckId.BlueEyes, (DeckName: "Blue-Eyes", DeckHash: "another-hash") }
        };

        /// <summary>
        /// Gets the deck name for the specified deck ID.
        /// </summary>
        /// <param name="deckId">The deck ID.</param>
        /// <returns>The deck name.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the deck ID is not found.</exception>
        public static string GetDeckName(DeckId deckId)
        {
            return _decks[deckId].DeckName;
        }

        /// <summary>
        /// Gets the deck hash for the specified deck ID.
        /// </summary>
        /// <param name="deckId">The deck ID.</param>
        /// <returns>The deck hash.</returns>
        /// <exception cref="KeyNotFoundException">Thrown if the deck ID is not found.</exception>
        public static string GetDeckHash(DeckId deckId)
        {
            return _decks[deckId].DeckHash;
        }

        /// <summary>
        /// Gets a list of available deck options for selection.
        /// </summary>
        /// <returns>A list of tuples containing DeckId and DeckName.</returns>
        public static IEnumerable<(DeckId Id, string Name)> GetDeckOptions()
        {
            return _decks.Select(d => (Id: d.Key, Name: d.Value.DeckName));
        }
    }
}