namespace OmegaBotWrapper.Heuristics
{
    internal static class DefaultCardKnowledge
    {
        internal static readonly HashSet<int> HandTrapMonsters =
        [
            14558127, // Ash Blossom & Joyous Spring
            23434538, // Maxx "C"
            94145021, // Droll & Lock Bird
            59438930, // Ghost Ogre & Snow Rabbit
            73642296, // Ghost Belle & Haunted Mansion
            97268402, // Effect Veiler
            52038441, // Ghost Mourner & Moonlit Chill
            34267821, // Artifact Lancea
            91800273, // Dimension Shifter
            27204311, // Nibiru, the Primal Being
            84192580, // Mulcharmy Purulia
            42141493, // Mulcharmy Fuwalos
            87126721  // Mulcharmy Nyalus
        ];

        internal static readonly HashSet<int> Floodgates =
        [
            71564252, // Thunder King Rai-Oh
            42009836, // Fossil Dyna Pachycephalo
            47084486, // Vanity's Fiend
            33746252, // Majesty's Fiend
            12435193, // Koa'ki Meiru Drago
            77585513, // Jinzo
            81674782, // Dimensional Fissure
            47355498, // Necrovalley
            68462976, // Secret Village of the Spellcasters
            4064256,  // Zombie World
            61740673, // Imperial Order
            30241314, // Macro Cosmos
            82732705, // Skill Drain
            53334471, // Gozen Match
            90846359, // Rivalry of Warlords
            58921041, // Anti-Spell Fragrance
            5851097,  // Vanity's Emptiness
            24348804, // Lose 1 Turn
            23516703, // Summon Limit
            30459350, // Imperial Iron Wall
            99916754, // Naturia Exterio
            58481572, // Masked HERO Dark Law
            33198837, // Naturia Beast
            91279700, // Evilswarm Ophion
            21044178, // Abyss Dweller
            75286621, // Invoked Mechaba
            94977269, // El Shaddoll Winda
            10443957, // Cyber Dragon Infinity
            15397015, // Inspector Boarder
            69248256, // Swordsoul Grandmaster - Chixiao
            47710198, // Swordsoul Sinister Sovereign - Qixing Longyuan
            92107604, // Runick Fountain
            48626373, // Kashtira Arise-Heart
            29301450  // S:P Little Knight
        ];

        internal static readonly HashSet<int> DangerousMonsters =
        [
            54366836, // Number 54: Lion Heart
            78371393, // Yubel
            4779091,  // Yubel - Terror Incarnate
            31764700, // Yubel - The Ultimate Nightmare
            28929131, // Zaphion the Timelord
            65314286, // Sadion the Timelord
            74530899, // Metaion the Timelord
            91712985, // Kamion the Timelord
            92435533, // Lazion the Timelord
            7733560,  // Michion the Timelord
            34137269, // Hailon the Timelord
            60222213, // Raphion the Timelord
            6616912,  // Gabrion the Timelord
            33015627, // Sandaion the Timelord
            63845230, // Eater of Millions
            20366274, // El Shaddoll Construct
            67547370, // Zushin the Sleeping Giant
            97403510, // Number 92: Heart-eartH Dragon
            29552709, // Daigusto Sphreeze
            81260679, // Ohime the Manifested Mikanko
            75771170, // Arahime the Manifested Mikanko
            47172959  // Yubel - Das Ewig Liebe Wachter
        ];

        internal static readonly HashSet<int> InvincibleMonsters =
        [
            23205979, // Spirit Reaper
            31764700, // Yubel - The Ultimate Nightmare
            4779091,  // Yubel - Terror Incarnate
            33015627, // Sandaion the Timelord
            60417395, // Darkness Neosphere
            6616912,  // Gabrion the Timelord
            7733560,  // Michion the Timelord
            28929131, // Zaphion the Timelord
            34137269, // Hailon the Timelord
            60222213, // Raphion the Timelord
            65314286, // Sadion the Timelord
            74530899, // Metaion the Timelord
            78371393, // Yubel
            91712985, // Kamion the Timelord
            92435533, // Lazion the Timelord
            12600382, // Exodia Necross
            11662742, // Gellenduo
            31305911, // Marshmallon
            2129638,  // Blue-Eyes Twin Burst Dragon
            69031175, // Blackwing Armor Master
            29552709, // Daigusto Sphreeze
            97403510, // Number 92: Heart-eartH Dragon
            26973555, // Number F0: Utopic Future Dragon
            98630720, // Borrelend Dragon
            84988419, // Bloom Diva the Melodious Choir
            15951532, // Amazoness Queen
            81260679, // Ohime the Manifested Mikanko
            75771170, // Arahime the Manifested Mikanko
            90829280, // Spirit of Yubel
            47172959  // Yubel - Das Ewig Liebe Wachter
        ];

        internal static readonly HashSet<int> AvoidTargeting =
        [
            82103466, // Divine Serpent Geh
            10000000, // Obelisk the Tormentor
            10000080, // The Winged Dragon of Ra - Sphere Mode
            10000090, // The Winged Dragon of Ra - Immortal Phoenix
            85991529, // Kozmo Dark Planet
            67547370, // Zushin the Sleeping Giant
            58604027, // The Legendary Exodia Incarnate
            64063868, // Kozmo Dark Eclipser
            55885348, // Kozmo Dark Destroyer
            55410871, // Blue-Eyes Chaos MAX Dragon
            13331639, // Supreme King Z-ARC
            72664875, // Crimson Nova Trinity the Dark Cubic Lord
            24550676, // Lunalight Leo Dancer
            87182127, // Ancient Gear Howitzer
            85908279, // Invoked Cocytus
            86221741, // Raidraptor - Ultimate Falcon
            50383626, // Darkest Diabolos, Lord of the Lair
            54082269, // Blackwing - Full Armor Master
            37818794, // Red-Eyes Dark Dragoon
            11738489, // The Arrival Cyberse @Ignister
            19801646, // The Legendary Fisherman II
            58601383, // Gaia Drake, the Universal Force
            10817524, // First of the Dragons
            31833038, // Borreload Dragon
            98630720, // Borrelend Dragon
            69815951, // Drytron Meteonis Draconids
            27279764, // Apoqliphort Towers
            40061558, // Apoqliphort Skybase
            44968687, // The Legendary Fisherman III
            51788412, // Chaos Ancient Gear Giant
            95209656  // Drytron Meteonis Quadrantids
        ];

        internal static int GetStableId(Struct.DynamicCard card) => card.Alias != 0 ? card.Alias : card.ID;
    }
}
