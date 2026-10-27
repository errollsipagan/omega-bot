using System;
using System.Linq;
using System.Reflection;
using BotTest.Game.AI.Brains;
using BotTest.Utils;
using OmegaBotWrapper;
using OmegaBotWrapper.Enum;
using OmegaBotWrapper.Heuristics;
using OmegaBotWrapper.States;
using OmegaBotWrapper.Struct;

namespace BotTest.Game.AI.Brains;

public abstract class KaibaBotBrain : DeckBotBrain
{
    private const int WeakSummonAttack = 1000;
    private const int MinimumTributeSummonAttack = 2000;
    private const int StrongSummonAttack = 1700;

    public enum CardScore
    {
        None = 0,
        Discourage = -1,
        Low = 50,
        Moderate = 100,
        High = 180,
        Excellent = 200,
        Perfect = 250,
    }

    private enum KaibaPendingAction
    {
        None = 1000,
        TributeToDoomed,
        MysticalSpaceTyphoon,
        ChangeOfHeart,
        SnatchSteal,
        SoulExchange,
        GracefulCharity,
        RingOfDestruction,
        CallOfTheHaunted,
        GiantGerm,
        NoblemanOfCrossout,
        CrushCardVirus,
        BarrelDragon,
        SearchThunderDragon,
        PrematureBurial,
        MonsterReborn,
        AxeOfDespair,
        MaggicJammer
    }

    private int _pendingTributeSummonAttack;
    private int _preferredTributeCardId;
    private int _preferredEquip;
    private DynamicCard? _selectedBattleTarget;
    private int _selectedBattleAttackerAttack;
    private readonly IActionScorer _kaibaActionScorer;

    protected record KaibaMoveSequence(
        string PathName,
        List<KaibaScoredMove> Sequence,
        int TotalScore
    );

    protected sealed record KaibaScoredMove(
        string Name,
        DynamicCard? Card,
        IdleCmdResponse Response,
        int Score,
        Action? OnSelected = null,
        bool IsLegal = true);

    protected KaibaBotBrain(
        ICardEvaluator? cardEvaluator = null,
        IBoardEvaluator? boardEvaluator = null,
        IActionScorer? actionScorer = null)
        : base(cardEvaluator, boardEvaluator, actionScorer)
    {
        _kaibaActionScorer = actionScorer ?? new KaibaMatrixActionScorer();
    }

    private static readonly Dictionary<int, string> CardNames = typeof(KaibaCardId)
      .GetFields(BindingFlags.Public | BindingFlags.Static)
      .Where(f => f.IsLiteral && f.FieldType == typeof(int))
      .GroupBy(f => (int)f.GetRawConstantValue()!).ToDictionary(g => g.Key, g => g.First().Name);

    public override async Task<BattleCmdResponse> OnBattleCmdAsync(
        DecisionContext<BattleCmdData> context,
        CancellationToken ct = default)
    {
        await PrepareContextAsync(context, ct);
        _selectedBattleTarget = null;
        _selectedBattleAttackerAttack = 0;

        var attackableCardArray = context.StateData.AttackableCards ?? Array.Empty<DynamicCard>();
        var attackableCards = attackableCardArray
            .Where(card => !card.Attacked && !card.IsDisabled())
            .ToList();
        var enemyMonsters = (await GetEnemyMonsterCardsAsync(false))
            .ToList();

        // 1. No attackable cards available
        if (attackableCards.Count == 0)
        {
            var nextPhase = context.StateData.CanMainPhaseTwo ? BattleAction.ToMainPhaseTwo : BattleAction.ToEndPhase;
            ConsoleUtils.PrintColored($" -> Seq: [None] | Reason: [No attackable cards] | Action: [{nextPhase}]");
            
            return context.StateData.CanMainPhaseTwo
                ? BattleAction.ToMainPhaseTwo
                : BattleAction.ToEndPhase;
        }

        // 2. Clear field -> Direct Attack Logic
        if (enemyMonsters.Count == 0)
        {
            var directAttacker = attackableCards
                .Where(card => card.CanDirectAttack)
                .OrderByDescending(card => card.Attack)
                .FirstOrDefault();

            if (directAttacker.ID > 0 &&
                attackableCardArray.TryGetByIDWithZone(
                    directAttacker.ID,
                    directAttacker.CurrentCoordinates.Sequence,
                    out _,
                    out var directIndex))
            {
                // Added comprehensive console info for Direct Attack
                ConsoleUtils.PrintColored($" -> Seq: [{directAttacker.CurrentCoordinates.Sequence}] | Name: [{GetKaibaCardName(directAttacker)}] | Action: [Direct Attack] | ATK: [{directAttacker.Attack}]");
                return attackableCardArray[directIndex].DeclareAttack();
            }

            var nextPhase = context.StateData.CanMainPhaseTwo ? BattleAction.ToMainPhaseTwo : BattleAction.ToEndPhase;
            ConsoleUtils.PrintColored($" -> Seq: [None] | Reason: [No direct attackers available] | Action: [{nextPhase}]");

            return context.StateData.CanMainPhaseTwo
                ? BattleAction.ToMainPhaseTwo
                : BattleAction.ToEndPhase;
        }

        // 3. Enemy has monsters -> Evaluate best target options
        // Log all possible combat pairs before choosing the best one
        ConsoleUtils.PrintColored($" --- Evaluating Combat Options ({attackableCards.Count} Attackers vs {enemyMonsters.Count} Defenders) ---");
        var len = 0;
        foreach (var a in attackableCards)
        {
            foreach (var b in enemyMonsters)
            {
                if (a.ID == b.ID)
                {
                    len++;
                    break;
                }
            }
        }
        var aiVsAi = len == attackableCards.Count && attackableCards.Count == enemyMonsters.Count;
        if (!aiVsAi)
        {
            ConsoleUtils.PrintColored("======Attackable Cards======");
            foreach (var item in attackableCards)
            {
                ConsoleUtils.PrintColored($"Card: [{GetKaibaCardName(item)}] | Attack: [{item.Attack}]");
            }
            ConsoleUtils.PrintColored("============================");
            ConsoleUtils.PrintColored("======Enemy Monsters======");
            foreach (var item in enemyMonsters)
            {
                ConsoleUtils.PrintColored($"Card: [{GetKaibaCardName(item)}] | Attack: [{item.Attack}]");
            }
            ConsoleUtils.PrintColored("============================");
        }
        var battleOptions = attackableCards
            .SelectMany(attacker => enemyMonsters
                // .Where(target => CheckTarget(target, attacker))
                .Select(target => new { Attacker = attacker, Target = target }))
            .ToList();

        for (int i = 0; i < battleOptions.Count; i++)
        {
            var option = battleOptions[i];
            int overkill = option.Attacker.Attack - option.Target.Attack;
            ConsoleUtils.PrintColored($"    Option [{i}] -> Attacker: [{GetKaibaCardName(option.Attacker)}] ({option.Attacker.Attack} ATK) vs Target: [{GetKaibaCardName(option.Target)}] ({option.Target.Attack} ATK) | Margin: [{overkill}]");
        }

        var bestAttack = battleOptions
            .Where(option => CheckTarget(option.Target, option.Attacker))
            .OrderByDescending(o => o.Target.Attack)
            .ThenBy(o => o.Attacker.Attack == o.Target.Attack)
            .ThenBy(o => o.Attacker.Attack)
            .FirstOrDefault();
        var aiBestAttack = attackableCards
            .FirstOrDefault();

        if (bestAttack != null &&
            attackableCardArray.TryGetByIDWithZone(
                bestAttack.Attacker.ID,
                bestAttack.Attacker.CurrentCoordinates.Sequence,
                out _,
                out var attackerIndex))
        {
            _selectedBattleTarget = bestAttack.Target;
            _selectedBattleAttackerAttack = bestAttack.Attacker.Attack;
            
            // Added comprehensive console info for Target Attack
            ConsoleUtils.PrintColored($" -> Seq: [{bestAttack.Attacker.CurrentCoordinates.Sequence}] | Name: [{GetKaibaCardName(bestAttack.Attacker)}] | Action: [Attack Monster] | Target: [{GetKaibaCardName(bestAttack.Target)}] | Target ATK: [{bestAttack.Target.Attack}]");
            return attackableCardArray[attackerIndex].DeclareAttack();
        }
        else if (aiVsAi &&
            attackableCardArray.TryGetByIDWithZone(
                aiBestAttack.ID,
                aiBestAttack.CurrentCoordinates.Sequence,
                out _,
                out var _attackerIndex))
        {
            ConsoleUtils.PrintColored($" -> Seq: [{aiBestAttack.CurrentCoordinates.Sequence}] | Name: [{GetKaibaCardName(aiBestAttack)}] | Action: [Attack Monster] | Target ATK: [{aiBestAttack}]");
            return attackableCardArray[_attackerIndex].DeclareAttack();
        }

        // 4. Enemy monsters are too strong to safely attack
        var fallbackPhase = context.StateData.CanMainPhaseTwo ? BattleAction.ToMainPhaseTwo : BattleAction.ToEndPhase;
        ConsoleUtils.PrintColored($" -> Seq: [None] | Reason: [No advantageous attacks found] | Action: [{fallbackPhase}]");

        return context.StateData.CanMainPhaseTwo
            ? BattleAction.ToMainPhaseTwo
            : BattleAction.ToEndPhase;
    }


    public override async Task<IdleCmdResponse> OnIdleCmdAsync(
        DecisionContext<IdleCmdData> context,
        CancellationToken ct = default)
    {
        return await SelectScoredIdleMoveAsync(context, ct);
    }

    protected async Task<IdleCmdResponse> SelectScoredIdleMoveAsync(
        DecisionContext<IdleCmdData> context,
        CancellationToken ct = default,
        IEnumerable<KaibaScoredMove>? specializedMoves = null)
    {
        await PrepareContextAsync(context, ct);
        _pendingTributeSummonAttack = 0;

        var state = context.StateData;
        
        // 1. Gather all state information from game engine
        var ownMonsters = await GetBotMonsterCardsAsync();
        var hand = await GetBotHandCardsAsync();
        var enemyMonsters = await GetEnemyMonsterCardsAsync(true);
        var allEnemyMonsters = await GetEnemyMonsterCardsAsync();
        var enemySpells = await GetEnemySpellCardsAsync();
        bool hasOpponentSpell = await CurrentField.GetPlayerSpellsCount() > 0;
        bool hasRoyalDecreeInField = await HasCardInBotSpellZoneAsync(KaibaCardId.RoyalDecree);
        var spells = await CurrentField.GetCardsInSpellTrapZone();
        var graveyard = await CurrentField.GetCardsInGrave();
        var lifePoints = await CurrentField.GetBotLifePoints();

        // 2. Filter summon systems
        var summonable = (state.SummonableCards ?? Array.Empty<DynamicCard>())
            .OrderByDescending(card => card.Attack)
            .ToArray();
        var specialSummonable = (state.SpecialSummonableCards ?? Array.Empty<DynamicCard>())
            .Where(card => card.Attack >= MinimumTributeSummonAttack)
            .ToArray();
        var revivable = graveyard
            .Where(card => card.HasType(CardType.Monster) && card.Attack >= StrongSummonAttack)
            .ToList();

        // 3. Initialize our Dynamic 2D List (Rows = Action Types, Columns = Dynamic Choices)
        var movesMatrix = new List<List<KaibaScoredMove>>();

        // Create a row pool for historical or passed specialized responses
        var specialRow = specializedMoves?.ToList() ?? new List<KaibaScoredMove>();
        movesMatrix.Add(specialRow);

        // Dynamic Row Allocation for categorized moves
        var normalSummonRow = new List<KaibaScoredMove>();
        var toFaceUpDefenseRow = new List<KaibaScoredMove>();
        var flipSummonRow = new List<KaibaScoredMove>();
        var specialSummonRow = new List<KaibaScoredMove>();
        var monsterSetRow = new List<KaibaScoredMove>();
        var spellTrapSetRow = new List<KaibaScoredMove>();
        var activationRow = new List<KaibaScoredMove>();
        var phaseControlRow = new List<KaibaScoredMove>();

        // Helper guard clause to check for duplicates across any row already populated
        bool IsShadowed(IdleCmdResponse response) => movesMatrix.Any(row => 
            row.Any(move => move.IsLegal && move.Response.Action == response.Action && move.Response.Index == response.Index));

        // Heuristic processing values
        bool hasSummonable = summonable.Count() > 0;
        bool isDefenseless = ownMonsters.Count() == 0;
        bool hasAdvantage = IsHasAdvantage(ownMonsters, enemyMonsters);
        bool hasStrongSummon = hand.Concat(summonable)
            .Any(card => card.Attack >= StrongSummonAttack);
        var strongSummons = ownMonsters
            .Where(monster => monster.Attack >= StrongSummonAttack);
        bool hasStrongSummoned = strongSummons.FirstOrDefault().IsFaceup();
        bool hasAceMonsterSummonable = summonable
            .Any(IsAceMonsters);
        bool weakOrEmptyMonsterField = ownMonsters.Count == 0 ||
                                    ownMonsters.All(card => card.Attack <= WeakSummonAttack);
        var ownMonsterWithHighestAttack = ownMonsters
            .Select(card => card.Attack)
            .DefaultIfEmpty(0)
            .Max();
        var strongestMonster = ownMonsters
            .FirstOrDefault(card => card.Attack == ownMonsterWithHighestAttack);
        _preferredEquip = strongestMonster.ID;
        var threateningEnemy = enemyMonsters
            .OrderByDescending(card => card.Attack)
            .FirstOrDefault();
        var hasStrongerMonster = ownMonsterWithHighestAttack > threateningEnemy.Attack;
        bool hasTributeMonster = hand.Any(c => c.Level >= 5 && c.Attack >= MinimumTributeSummonAttack);
        var minTributeMonster = hand
            .Where(card => card.Level >= 5)
            .OrderBy(card => card.Level)
            .FirstOrDefault();
        var minTributeCount = GetTributeCount(minTributeMonster);

        var flipEffectMonsters = state.ReposableCards
            .Where(
                card => CanFlipSummon(card, hasStrongerMonster, hasAceMonsterSummonable) ||
                hasAdvantage && (CardPosition)card.Position == CardPosition.FaceUpDefence
            )
            .ToArray();
        var toFaceUpDefense = state.ReposableCards
            .Where(card => card.Attack < StrongSummonAttack && 
            (CardPosition)card.Position == CardPosition.FaceUpAttack &&
            card.IsSpecialSummoned)
            .ToArray();
        var monsterSettable = hasSummonable 
            ? [.. hand.Where(IsKaibaDefensiveMonster), .. state.MonsterSetableCards ?? []] 
            : Array.Empty<DynamicCard>();
        var monsterOnDefense = ownMonsters
            .Where(monster => monster.IsDefense());
        bool isFirstTurn = context.Header.CurrentTurn <= 1;

        state.SummonableCards = summonable;
        state.SpecialSummonableCards = specialSummonable;
        state.ReposableCards = [..flipEffectMonsters, ..toFaceUpDefense];
        state.MonsterSetableCards = monsterSettable;

        // --- ROW 1: Card Activations ---
        foreach (var card in state.ActivableCards ?? Array.Empty<DynamicCard>())
        {
            var activations = card.ActionActivateIndex ?? Array.Empty<ActivationCardIndex>();
            for (int activation = 0; activation < activations.Length; activation++)
            {
                var response = card.MainActivateFromIndex(activation);
                if (IsShadowed(response))
                    continue;

                int score = ScoreMainActivation(
                    card,
                    ownMonsters,
                    enemyMonsters,
                    allEnemyMonsters,
                    enemySpells,
                    spells,
                    revivable,
                    minTributeCount,
                    hasTributeMonster,
                    threateningEnemy.Attack >= MinimumTributeSummonAttack,
                    isFirstTurn,
                    hasSummonable,
                    hasStrongSummoned,
                    isDefenseless,
                    hasAdvantage);
                if (score != (int)CardScore.Discourage)
                    activationRow.Add(new KaibaScoredMove(
                        $"Activate {GetKaibaCardName(card)}",
                        card,
                        response,
                        score,
                        () => ConfigureMainActivation(card)));
            }
        }
        if (hand.Count > 6)
        {
            var spellCards = (state.SpellSetableCards ?? Array.Empty<DynamicCard>())
                .Where(card => CanDiscardToSet(card, hasStrongerMonster, weakOrEmptyMonsterField))
                .ToArray();
            foreach (var card in spellCards)
            {
                var response = card.SetSpellTrap();
                if (!IsShadowed(response))
                {
                    spellTrapSetRow.Add(new KaibaScoredMove(
                        $"Discard to Set {GetKaibaCardName(card)}",
                        card,
                        response,
                        300)); // High priority to avoid discarding
                }
            }
        }
        movesMatrix.Add(activationRow);

        // --- ROW 2: Special Summons ---
        foreach (var card in specialSummonable)
        {
            var response = card.SpecialSummon();
            if (!IsShadowed(response))
                specialSummonRow.Add(new KaibaScoredMove(
                    $"Special Summon {GetKaibaCardName(card)} ({card.Attack} ATK)",
                    card,
                    response,
                    100));
        }
        movesMatrix.Add(specialSummonRow);

        // --- ROW 3: Monster Sets ---
        foreach (var card in monsterSettable)
        {
            var response = card.SetMonster();
            if (IsShadowed(response))
                continue;
            int score = ScoreSettableMonster(
                card,
                weakOrEmptyMonsterField,
                hasStrongSummon,
                !hasStrongerMonster
            );
            monsterSetRow.Add(new KaibaScoredMove($"Set Monster {GetKaibaCardName(card)}", card, response, score));
        }
        if (!hasAceMonsterSummonable && isDefenseless)
        {
            movesMatrix.Add(monsterSetRow);
        }

        foreach (var card in flipEffectMonsters)
        {
            var response = card.Reposition();
            if (IsShadowed(response))
                continue;
            flipSummonRow.Add(new KaibaScoredMove(
                $"Flip Summon Effect {GetKaibaCardName(card)}",
                card,
                response,
                (int)CardScore.Moderate,
                () => TrackTributeSummon(card)));
        }
        movesMatrix.Add(flipSummonRow);

        // --- ROW 4: Normal Summons ---
        foreach (var card in summonable)
        {
            var response = card.NormalSummon();
            if (!IsShadowed(response))
            {
                var score = card.Attack >= StrongSummonAttack
                    ? (int)CardScore.High
                    : _preferredTributeCardId > 0 && card.Level >= 5 && card.Attack >= MinimumTributeSummonAttack
                        ? (int)CardScore.Excellent
                    : (int)CardScore.Discourage;
                if (score == (int)CardScore.Discourage && hasStrongerMonster && !hasStrongSummon)
                    score = (int)CardScore.Low;
                if (IsAceMonsters(card))
                    score = (int)CardScore.Perfect;

                if (score != (int)CardScore.Discourage)
                    normalSummonRow.Add(new KaibaScoredMove(
                        $"Normal Summon {GetKaibaCardName(card)} ({card.Attack} ATK)",
                        card,
                        response,
                        score,
                        () => TrackTributeSummon(card)));
            }
        }
        movesMatrix.Add(normalSummonRow);

        foreach (var card in toFaceUpDefense)
        {
            var response = card.Reposition();
            if (IsShadowed(response))
                continue;
            if (card.IsDefense())
                continue;
            toFaceUpDefenseRow.Add(new KaibaScoredMove(
                $"Face-Up Defense {GetKaibaCardName(card)} ({card.Defense} DEF)",
                card,
                response,
                (int)CardScore.Moderate,
                () => TrackTributeSummon(card)));
        }
        movesMatrix.Add(toFaceUpDefenseRow);

        // --- ROW 5: Spell/Trap Sets ---
        foreach (var card in state.SpellSetableCards ?? Array.Empty<DynamicCard>())
        {
            var response = card.SetSpellTrap();
            if (card.HasType(CardType.Trap) && hasRoyalDecreeInField)
                continue;
            if (IsShadowed(response))
                continue;
            if (spells.Any(spell => spell.ID == card.ID))
                continue;
            int score;
            if (card.IsCode(KaibaCardId.MirrorForce, KaibaCardId.NegateAttack, KaibaCardId.MaggicJammer))
                score = (int)CardScore.Moderate;
            else if (card.IsCode(KaibaCardId.CrushCardVirus) &&
                    ownMonsters.Any(IsCrushCardVirusTribute))
                score = 500;
            else if (weakOrEmptyMonsterField &&
                    (card.HasType(CardType.Trap) ||
                    card.IsCode(KaibaCardId.MysticalSpaceTyphoon, KaibaCardId.Shrink)))
                score = 50;
            else if (weakOrEmptyMonsterField && card.IsCode(KaibaCardId.TributeToTheDoomed) && enemyMonsters.Count > 0)
                score = (int)CardScore.High;
            else
                score = -1;
            spellTrapSetRow.Add(new KaibaScoredMove($"Set Spell/Trap {GetKaibaCardName(card)}", card, response, score));
        }
        movesMatrix.Add(spellTrapSetRow);

        if (state.CanBattlePhase)
        {
            var battleScore = (int)CardScore.Low;
            if (weakOrEmptyMonsterField)
                battleScore = (int)CardScore.Discourage;
            if (hasStrongerMonster && !IsKaibaDefensiveMonster(strongestMonster))
                battleScore = (int)CardScore.Perfect;
            phaseControlRow.Add(new KaibaScoredMove("Enter Battle Phase", null, new IdleCmdResponse(MainAction.ToBattlePhase), battleScore));
        }
        movesMatrix.Add(phaseControlRow);

        // Create Permutations (Flatten all matrix choices into a continuous calculation stream)
        var allPermutationPaths = new List<KaibaMoveSequence>();
        GenerateDynamicPermutations(movesMatrix, 0, new List<KaibaScoredMove>(), allPermutationPaths, false, state.CanBattlePhase, state.CanEndPhase, state.CanShuffleHand);

        // Sort the paths by their accumulated total sequence score
        var bestPath = allPermutationPaths.MaxBy(p => p.TotalScore);

        PreviewCards(summonable, "SUMMONABLE");
        PreviewCards(monsterSettable, "SETTABLE");
        PreviewCards(ownMonsters, "OWN MONSTERS", true);
        PreviewCards(enemyMonsters, "ENEMY MONSTERS", true);
        PreviewCards(spells, "SPELLS ON FIELD");
        PreviewCards(state.ActivableCards, "ACTIVABLE CARDS");
        PreviewCards(hand, "KAIBA HANDS");
        PreviewCards(state.ReposableCards, "REPOSABLE CARDS");
        ConsoleUtils.PrintColored("=== KAIBA TURN SEQUENCE PERMUTATIONS ===");
        var z = 1;
        foreach (var path in allPermutationPaths.OrderByDescending(p => p.TotalScore).Take(5)) // Show top 5 paths
        {
            ConsoleUtils.PrintColored($" -> Path {z}: [{path.PathName}] | Total Sequence Score: {path.TotalScore} pts");
            var x = 1;
            foreach (var seq in path.Sequence)
            {
                ConsoleUtils.PrintColored($" -> Seq: [{x}] | Name: [{seq.Name}] | Score: [{seq.Score}]");
                x++;
            }
            z++;
        }
        ConsoleUtils.PrintColored("========================================");

        if (bestPath == null || bestPath.Sequence.Count == 0)
        {
            return new IdleCmdResponse(MainAction.ToEndPhase);
        }

        // Grab the immediate FIRST move required to execute this optimal path
        var selectedMove = bestPath.Sequence.First();
        
        ConsoleUtils.PrintColored($"[KAIBA SCORE] Selected optimal pathway sequence starting with: {selectedMove.Name} (Path Score: {bestPath.TotalScore} pts).");

        selectedMove.OnSelected?.Invoke();
        return selectedMove.Response;
    }

    private static void GenerateDynamicPermutations(
        List<List<KaibaScoredMove>> matrix, 
        int rowIndex, 
        List<KaibaScoredMove> currentPath, 
        List<KaibaMoveSequence> results,
        bool hasSummonedInPath,
        bool canBattlePhase,
        bool canEndPhase,
        bool canShuffleHand)
    {
        // 🛑 BASE CASE: We reached the end of all standard card category rows.
        // Now, we evaluate the turn state to build the final Phase Control options.
        if (rowIndex == matrix.Count)
        {
            var localPhaseControlRow = new List<KaibaScoredMove>();

            if (canBattlePhase)
            {
                // 🎯 YOUR RULE: If a monster was summoned at any point in this path, reward it with 100 points
                int battleScore = hasSummonedInPath ? 100 : 0;
                localPhaseControlRow.Add(new KaibaScoredMove("Enter Battle Phase", null, new IdleCmdResponse(MainAction.ToBattlePhase), battleScore));
            }
            if (canEndPhase)
            {
                localPhaseControlRow.Add(new KaibaScoredMove("End Turn", null, new IdleCmdResponse(MainAction.ToEndPhase), 0));
            }
            if (canShuffleHand)
            {
                localPhaseControlRow.Add(new KaibaScoredMove("Shuffle Hand", null, new IdleCmdResponse(MainAction.ShuffleHand), 0));
            }

            // Evaluate choices inside the final dynamic phase row
            // Option A: Just finish the path without any explicit phase control action
            SavePermutationPath(currentPath, results);

            // Option B: Run through the dynamically compiled system choices
            foreach (var phaseMove in localPhaseControlRow)
            {
                currentPath.Add(phaseMove);
                SavePermutationPath(currentPath, results);
                currentPath.RemoveAt(currentPath.Count - 1);
            }
            return;
        }

        // --- RECURSION BRANCHES ---

        // Option 1: Skip this current action category row entirely
        GenerateDynamicPermutations(matrix, rowIndex + 1, currentPath, results, hasSummonedInPath, canBattlePhase, canEndPhase, canShuffleHand);

        // Option 2: Choose a move from this row and step forward
        foreach (var move in matrix[rowIndex])
        {
            // Detect if this specific move is a Normal or Special Summon action
            bool isSummonAction = move.Response.Action == MainAction.Summon || 
                                move.Response.Action == MainAction.SpSummon;

            currentPath.Add(move);
            
            // Pass the updated summon status down to the next row frame
            GenerateDynamicPermutations(
                matrix, 
                rowIndex + 1, 
                currentPath, 
                results,
                hasSummonedInPath || isSummonAction, // If true before OR true now, it stays true
                canBattlePhase, 
                canEndPhase, 
                canShuffleHand
            );

            currentPath.RemoveAt(currentPath.Count - 1); // Backtrack
        }
    }

    private static void SavePermutationPath(List<KaibaScoredMove> path, List<KaibaMoveSequence> results)
    {
        if (path.Count > 0)
        {
            string pathName = string.Join(" -> ", path.Select(m => m.Name));
            int totalScore = path.Sum(m => m.Score);
            results.Add(new KaibaMoveSequence(pathName, new List<KaibaScoredMove>(path), totalScore));
        }
    }


    public override async Task<int> OnSelectChainAsync(
        DecisionContext<SelectChainData> context,
        CancellationToken ct = default)
    {
        await PrepareContextAsync(context, ct);
        RegisterChainDescriptions(context.StateData.Descriptions ?? Array.Empty<int>());

        var selectable = context.StateData.SelectableCards ?? Array.Empty<DynamicCard>();
        var enemyMonsters = await GetEnemyMonsterCardsAsync(true);
        var ownMonsters = await GetBotMonsterCardsAsync();
        var hand = await GetBotHandCardsAsync();
        var field = await CurrentField.GetBotField();
        var phase = (DuelPhase)context.Header.CurrentPhase;
        bool aiTurn = context.Header.CurrentPlayer != 1;
        bool opponentTurn = !aiTurn;
        bool underDirectAttack = field.OwnSide.UnderAttack && !field.OwnSide.HasBattlingMonster;
        bool mainPhase = phase == DuelPhase.Main1 && aiTurn;
        bool opponentEndPhase = phase == DuelPhase.End && opponentTurn;
        bool opponentBattle = opponentTurn &&
                              (phase is DuelPhase.BattleStart or DuelPhase.BattleStep);
        var chainOptions = new List<(int Index, int Score, DynamicCard Card, string Name)>();
        bool hasStrongSummon = ownMonsters.Any(monster => monster.Attack >= StrongSummonAttack);
        bool hasDisruptMonster = ownMonsters.Any(IsDisruptMonsters);
        bool hasOpponentSpell = await CurrentField.GetPlayerSpellsCount() > 0;
        ConsoleUtils.PrintColored($"===OnSelectChainAsync===");
        ConsoleUtils.PrintColored("======Enemy Monsters======");
        foreach (var item in enemyMonsters)
        {
            ConsoleUtils.PrintColored($"Card: [{GetKaibaCardName(item)}] | Attack: [{item.Attack}]");
        }
        ConsoleUtils.PrintColored("============================");
        ConsoleUtils.PrintColored("======Selectable======");
        foreach (var item in selectable)
        {
            ConsoleUtils.PrintColored($"Card: [{GetKaibaCardName(item)}] | Attack: [{item.Attack}]");
        }
        ConsoleUtils.PrintColored("============================");
        ConsoleUtils.PrintColored($"opponentTurn [{opponentTurn}] | phase [{phase}] | enemyMonsters: [{enemyMonsters.Count}]");

        for (int i = 0; i < selectable.Length; i++)
        {
            var card = selectable[i];
            int score = -100;

            if (card.IsCode(KaibaCardId.TorrentialTribute) && enemyMonsters.Count > 0 && !hasStrongSummon && !hasDisruptMonster)
            {
                score = 200;
            }
            else if (card.IsCode(KaibaCardId.RingOfDestruction) && opponentTurn && enemyMonsters.Count > 0)
                    // (phase is DuelPhase.Main1 or DuelPhase.BattleStart or DuelPhase.BattleStep or DuelPhase.Battle))
            {
                score = 250;
            }
            else if (card.IsCode(KaibaCardId.MirrorForce) && opponentBattle)
            {
                score = 300;
            }
            else if (card.IsCode(KaibaCardId.NegateAttack) && opponentBattle)
            {
                score = 200;
            }
            else if (card.IsCode(KaibaCardId.FairyBox))
            {
                if (opponentBattle)
                    score = 300;
                else if (phase == DuelPhase.Standby)
                    score = field.OwnSide.LifePoints > 500 ? 150 : -999;
            }
            else if (card.IsCode(KaibaCardId.CallOfTheHaunted) && (underDirectAttack || opponentEndPhase))
            {
                score = 150;
            }
            else if (card.IsCode(KaibaCardId.PrematureBurial, KaibaCardId.PrematureBurial2) && mainPhase)
            {
                score = (int)CardScore.High;
            }
            else if (card.IsCode(KaibaCardId.GiantGerm))
            {
                score = 300;
            }
            else if (card.IsCode(KaibaCardId.BarrelDragon))
            {
                score = 200;
            }
            else if (card.IsCode(KaibaCardId.SpiritRyu))
            {
                score = 100 * hand.Count(handCard => handCard.HasRace(CardRace.Dragon));
            }
            else if ((card.IsCode(KaibaCardId.ImperialOrder) || card.IsCode(KaibaCardId.RoyalDecree)) && aiTurn)
            {
                score = 400;
            }
            else if (card.IsCode(KaibaCardId.CrushCardVirus) && ownMonsters.Any(IsCrushCardVirusTribute))
            {
                score = 500;
            }
            else if (card.IsCode(KaibaCardId.MaggicJammer) && opponentBattle)
            {
                score = (int)CardScore.Perfect;
            }

            chainOptions.Add((i, score, card, $"Chain {GetKaibaCardName(card)}"));
        }

        if (!context.StateData.Forced)
            chainOptions.Add((-1, 0, default, "Pass Chain"));

        foreach (var option in chainOptions)
            ConsoleUtils.PrintColored($"[KAIBA SCORE] {option.Name}: {option.Score}");

        if (chainOptions.Count == 0)
            return await base.OnSelectChainAsync(context, ct);

        var selected = chainOptions
            .OrderByDescending(option => option.Score)
            .ThenBy(option => option.Index < 0 ? int.MaxValue : option.Index)
            .FirstOrDefault();
        ConsoleUtils.PrintColored($"[KAIBA SCORE] Selected {selected.Name} ({selected.Score} pts).");

        if (selected.Index < 0)
            return context.StateData.Forced ? 0 : -1;

        if (selected.Card.IsCode(KaibaCardId.RingOfDestruction))
            SetPendingAction(
                PendingActionKind.TargetSelection,
                KaibaCardId.RingOfDestruction,
                MainAction.Activate,
                contextCode: (int)KaibaPendingAction.RingOfDestruction);
        else if (selected.Card.IsCode(KaibaCardId.CallOfTheHaunted))
            SetPendingAction(
                PendingActionKind.TargetSelection,
                KaibaCardId.CallOfTheHaunted,
                MainAction.Activate,
                contextCode: (int)KaibaPendingAction.CallOfTheHaunted);
        else if (selected.Card.IsCode(KaibaCardId.PrematureBurial, KaibaCardId.PrematureBurial2))
            SetPendingAction(
                PendingActionKind.TargetSelection,
                selected.Card.ID,
                MainAction.Activate,
                contextCode: (int)KaibaPendingAction.PrematureBurial);
        else if (selected.Card.IsCode(KaibaCardId.GiantGerm))
            SetPendingAction(
                PendingActionKind.TargetSelection,
                KaibaCardId.GiantGerm,
                MainAction.Activate,
                contextCode: (int)KaibaPendingAction.GiantGerm);
        else if (selected.Card.IsCode(KaibaCardId.CrushCardVirus))
            SetPendingAction(
                PendingActionKind.TargetSelection,
                KaibaCardId.CrushCardVirus,
                MainAction.Activate,
                contextCode: (int)KaibaPendingAction.CrushCardVirus);
        else if (selected.Card.IsCode(KaibaCardId.MaggicJammer))
            SetPendingAction(
                PendingActionKind.TargetSelection,
                KaibaCardId.MaggicJammer,
                MainAction.Activate,
                contextCode: (int)KaibaPendingAction.MaggicJammer);

        return selected.Index;
    }

    public override async Task<IList<byte>?> OnSelectCardAsync(
        DecisionContext<SelectCardData> context,
        CancellationToken ct = default)
    {
        await PrepareContextAsync(context, ct);
        var state = context.StateData;
        var enemyMonsters = await GetEnemyMonsterCardsAsync(true);
        var ownMonsters = await GetBotMonsterCardsAsync();
        var allEnemyMonsters = await GetEnemyMonsterCardsAsync();
        var enemySpells = await GetEnemySpellCardsAsync();
        var selectable = state.SelectableCards ?? Array.Empty<DynamicCard>();
        var pendingAction = (KaibaPendingAction)PendingAction.ContextCode;
        var hand = await GetBotHandCardsAsync();

        var DHitName = Enum.GetName((DHint)state.SelectionHint);

        ConsoleUtils.PrintColored($"===OnSelectCard | PendingAction [{GetPendingAction()}] | SelectionHint [{DHitName}] | Card [{GetKaibaCardName(PendingAction.CardId)}]===");
        ConsoleUtils.PrintColored($"=== Minimum [{state.Minimum}] | Maximum [{state.Maximum}] | SelectableCount [{(int)state.SelectableCount}] ===");
        foreach (dynamic card in selectable)
        {
            ConsoleUtils.PrintColored($" Card [{GetKaibaCardName(card)}] | Owner [{card.Owner}]");
        }
        ConsoleUtils.PrintColored("====================================");

        if (state.SelectionHint == (int)DHint.ATTACKTARGET)
        {
            if (_selectedBattleTarget.HasValue &&
                selectable.TryGetByIDWithZone(
                    _selectedBattleTarget.Value.ID,
                    _selectedBattleTarget.Value.CurrentCoordinates.Sequence,
                    out _,
                    out var targetIndex))
            {
                return new[] { (byte)targetIndex };
            }

            var favorableTarget = selectable
                .Select((card, index) => new { Card = card, Index = index })
                .Where(item => item.Card.Attack < _selectedBattleAttackerAttack)
                .OrderByDescending(item => item.Card.Attack)
                .FirstOrDefault();

            if (favorableTarget != null)
                return new[] { (byte)favorableTarget.Index };
        }

        if (state.SelectionHint == (int)DHint.DISCARD &&
            pendingAction is KaibaPendingAction.TributeToDoomed or KaibaPendingAction.GracefulCharity)
        {
            var selection = selectable
                .Select((card, index) => new { Card = card, Index = index })
                .Where(item => !IsKaibaFusionCoreCard(item.Card))
                .OrderBy(item => item.Card.Attack)
                .Take(state.Minimum)
                .Select(item => (byte)item.Index)
                .ToArray();

            if (selection.Length >= state.Minimum)
            {
                if (pendingAction == KaibaPendingAction.TributeToDoomed)
                    SetPendingAction(
                        PendingActionKind.TargetSelection,
                        KaibaCardId.TributeToTheDoomed,
                        MainAction.Activate,
                        contextCode: (int)KaibaPendingAction.TributeToDoomed);
                else
                    ClearPendingAction();
                return selection;
            }
        }

        if (state.SelectionHint is (int)DHint.TARGET or (int)DHint.DESTROY)
        {
            var targets = SelectKaibaTarget(
                selectable, 
                pendingAction, 
                ownMonsters,
                enemySpells,
                state.Maximum
            );
            
            if (targets.Length > 0)
            {
                if (pendingAction is KaibaPendingAction.ChangeOfHeart or
                    KaibaPendingAction.SnatchSteal or
                    KaibaPendingAction.SoulExchange)
                {
                    _preferredTributeCardId = selectable[targets[0]].ID;
                }

                ClearPendingAction();
                return targets;
            }
            ClearPendingAction();
        }

        if (pendingAction is KaibaPendingAction.CallOfTheHaunted 
            or KaibaPendingAction.MonsterReborn &&
            state.SelectionHint is (int)DHint.SPSUMMON or (int)DHint.TOFIELD)
        {
            var reviveTarget = selectable
                .Select((card, index) => new { Card = card, Index = index })
                .OrderByDescending(item => item.Card.Attack)
                .FirstOrDefault();

            if (reviveTarget != null)
            {
                ClearPendingAction();
                return new[] { (byte)reviveTarget.Index };
            }
        }

        // KaibaPendingAction.PrematureBurial or GiantGerm
        if (state.SelectionHint is (int)DHint.SPSUMMON)
        {
            var spSummonTarget = selectable
                .Select((card, index) => new { Card = card, Index = index })
                .OrderByDescending(item => item.Card.Attack)
                .Take(Math.Min(state.Maximum, selectable.Length))
                .Select(item => (byte)item.Index)
                .ToArray();

            if (spSummonTarget.Length >= state.Minimum)
            {
                ClearPendingAction();
                return spSummonTarget;
            }
        }

        if (PendingContextIs((int)KaibaPendingAction.SearchThunderDragon) &&
            state.SelectionHint == (int)DHint.ATOHAND)
        {
            var selection = selectable
                .Select((card, index) => new { Card = card, Index = index })
                .Where(item => item.Card.IsCode(KaibaCardId.ThunderDragon))
                .Take(Math.Min(state.Maximum, selectable.Length))
                .Select(item => (byte)item.Index)
                .ToArray();

            if (selection.Length >= state.Minimum)
            {
                ClearPendingAction();
                return selection;
            }
        }

        // Sangan
        if (state.SelectionHint == (int)DHint.ATOHAND)
        {
            return GetPreferredSelection(selectable, hand, GetSanganPreferredOnHand, "SANGAN PREFERRED", state);
        }

        //Penguin Solider
        if (state.SelectionHint == (int)DHint.RTOHAND)
        {
            return GetPreferredSelection(selectable, hand, GetPenguinPreferredOnHand, "PENGUIN PREFERRED", state);
        }

        if (pendingAction is KaibaPendingAction.AxeOfDespair)
        {
            var equipTarget = selectable
                .Select((card, index) => new { Card = card, Index = index })
                .FirstOrDefault(item => item.Card.ID == _preferredEquip);

            if (equipTarget != null)
            {
                ClearPendingAction();
                return new[] { (byte)equipTarget.Index };
            }
        }

        return await base.OnSelectCardAsync(context, ct);
    }

    private byte[]? GetPreferredSelection(
        DynamicCard[] selectable, 
        List<DynamicCard> hand, 
        Func<DynamicCard, List<DynamicCard>, int> scoreFunc,
        string title,
        SelectCardData state
    )
    {
        var preferredSelection = selectable
            .Select((card, index) => new { Card = card, Index = index, Score = scoreFunc(card, hand) })
            .OrderByDescending(item => item.Score)
            .Where(item => item.Score > (int)CardScore.Discourage);
        PreviewCards(preferredSelection.Select(item => item.Card).ToArray(), title);
        var selection = preferredSelection
            .Take(Math.Min(state.Maximum, selectable.Length))
            .Select(item => (byte)item.Index)
            .ToArray();

        if (selection.Length >= state.Minimum)
        {
            ClearPendingAction();
            return selection;
        }
        return null;
    }

    public override async Task<bool> OnSelectEffectYnAsync(
        DecisionContext<SelectEffectYnData> context,
        CancellationToken ct = default)
    {
        await PrepareContextAsync(context, ct);

        var state = context.StateData;

        ConsoleUtils.PrintColored($"===OnSelectEffectYnAsync | EffectCard [{GetKaibaCardName(state.EffectCard)}] ===");

        if (state.EffectCard.IsCode(KaibaCardId.FairyBox))
        {
            var field = await CurrentField.GetBotField();
            return field.OwnSide.LifePoints > 500;
        }

        if (state.EffectCard.IsCode(
            KaibaCardId.GiantGerm,
            KaibaCardId.CallOfTheHaunted,
            KaibaCardId.TorrentialTribute,
            KaibaCardId.RingOfDestruction,
            KaibaCardId.PrematureBurial,
            KaibaCardId.PrematureBurial2,
            KaibaCardId.MaggicJammer
            ) || CanFlipSummon(state.EffectCard))
        {
            return true;
        }

        return await base.OnSelectEffectYnAsync(context, ct);
    }

    public override async Task<CardPosition> OnSelectPositionAsync(
        DecisionContext<SelectPositionData> context,
        CancellationToken ct = default)
    {
        await PrepareContextAsync(context, ct);

        var state = context.StateData;

        ConsoleUtils.PrintColored($"===OnSelectPositionAsync | CardID [{GetKaibaCardName(state.CardID)}] ===");

        if (state.CardID is KaibaCardId.CyberJar or 
            KaibaCardId.GiantGerm or 
            KaibaCardId.PenguinSoldier or
            KaibaCardId.ManEaterBug)
        {
            return PickPosition(context.StateData, CardPosition.FaceDownDefence);
        }

        var bestEnemy = await GetBestEnemyMonster(true);
        if (bestEnemy.ID > 0 && bestEnemy.Attack > GetExpectedAttack(context.StateData.CardID))
            return PickPosition(context.StateData, CardPosition.FaceDownDefence);

        if (state.CardID is KaibaCardId.VorseRaider or 
            KaibaCardId.BattleOx or 
            KaibaCardId.LaJinn or
            KaibaCardId.RyuKishinPowered)
            return CardPosition.FaceUpAttack;

        // return await base.OnSelectPositionAsync(context, ct);
        return PickPosition(context.StateData, CardPosition.FaceDownDefence);
    }

    public override async Task<IList<byte>?> OnSelectTributeAsync(
        DecisionContext<SelectTributeData> context,
        CancellationToken ct = default)
    {
        await PrepareContextAsync(context, ct);

        var state = context.StateData;
        var selectable = state.SelectableCards ?? Array.Empty<DynamicCard>();
        ConsoleUtils.PrintColored($"===OnSelectTributeAsync | PendingAction [{GetPendingAction()}] | Card [{GetKaibaCardName(PendingAction.CardId)}] ===");
        ConsoleUtils.PrintColored($"=== Minimum [{state.Minimum}] | Maximum [{state.Maximum}] | SelectableCount [{(int)state.SelectableCount}] ===");
        PreviewCards(selectable, "SELECTABLE");

        if (PendingAction.ContextCode == (int)KaibaPendingAction.CrushCardVirus)
        {
            var tributes = selectable
                .Select((card, index) => new { Card = card, Index = index })
                .Where(item => item.Card.IsCode(KaibaCardId.SaggiTheDarkClown, KaibaCardId.GiantGerm))
                .Take(state.Minimum)
                .Select(item => (byte)item.Index)
                .ToArray();

            if (tributes.Length >= state.Minimum)
            {
                ClearPendingAction();
                return tributes;
            }

            ClearPendingAction();
        }

        var selection = selectable
            .Select((card, index) => new { Card = card, Index = index })
            .Where(item => _pendingTributeSummonAttack == 0 ||
                           item.Card.Attack < _pendingTributeSummonAttack)
            .OrderBy(item => item.Card.ID == _preferredTributeCardId ? 0 : 1)
            .ThenBy(item => item.Card.Attack)
            .Take(Math.Min(state.Minimum, state.Maximum))
            .Select(item => (byte)item.Index)
            .ToArray();

        if (selection.Length >= state.Minimum)
        {
            _preferredTributeCardId = 0;
            _pendingTributeSummonAttack = 0;
            return selection;
        }

        return await base.OnSelectTributeAsync(context, ct);
    }

    public static int GetTributeCount(DynamicCard card)
    {
        if (card.Level <= 4)
            return 0;
        return card.Level <= 6 ? 1 : 2;
    }

    public static bool CanNormalSummon(DynamicCard card, IReadOnlyCollection<DynamicCard> ownMonsters, int tributeCount = 0)
    {
        if (card.Level <= 4)
            return true;
        int requiredTributes = GetTributeCount(card) - tributeCount;
        int lowerAttackTributes = ownMonsters.Count(monster => monster.Attack < card.Attack && monster.Level <= 4);
        return lowerAttackTributes >= requiredTributes;
    }

    private static bool CanFlipSummon(DynamicCard card, bool hasStrongerMonster = false, bool hasAceMonsterSummonable = false)
    {
        if (card.IsCode(KaibaCardId.MagicianOfFaith))
            return true;
        if (card.IsCode(
                KaibaCardId.CyberJar,
                KaibaCardId.PenguinSoldier,
                KaibaCardId.ManEaterBug
            ) && !hasStrongerMonster && !hasAceMonsterSummonable)
        {
            return true;
        }
        return false;
    }

    private static bool CanDiscardToSet(
        DynamicCard card,
        bool hasStrongerMonster,
        bool weakOrEmptyMonsterField
    )
    {
        if (card.IsCode(
            KaibaCardId.TorrentialTribute,
            KaibaCardId.RingOfDestruction
        ))
        {
            return !hasStrongerMonster && weakOrEmptyMonsterField;
        }

        if (card.IsCode(
            KaibaCardId.Polymerization,
            KaibaCardId.MonsterReborn
        ))
        {
            return false;
        }


        return true;
    }

    private static bool IsKaibaDefensiveMonster(DynamicCard card) =>
        card.IsCode(
            KaibaCardId.GiantGerm,
            KaibaCardId.SaggiTheDarkClown,
            KaibaCardId.CyberJar,
            KaibaCardId.Sangan,
            KaibaCardId.PenguinSoldier,
            KaibaCardId.ManEaterBug
        );

    private static bool CheckTarget(DynamicCard target, DynamicCard attacker)
    {
        var position = (CardPosition)target.Position;
        if (position == CardPosition.FaceUpAttack)
        {
            return target.Attack < attacker.Attack || target.Attack == attacker.Attack;
        }
        return target.Defense < attacker.Attack;
    }

    private static string GetKaibaCardName(DynamicCard card)
    {
        return GetKaibaCardName(card.ID);
    }

    private static string GetKaibaCardName(int cardID)
    {
        string cardName = cardID.ToString();
        if (CardNames.TryGetValue(cardID, out string? name))
        {
            cardName = name;
        }
        return cardName;
    }

    private static int ScoreMainActivation(
        DynamicCard card,
        IReadOnlyCollection<DynamicCard> ownMonsters,
        IReadOnlyCollection<DynamicCard> enemyMonsters,
        IReadOnlyCollection<DynamicCard> allEnemyMonsters,
        IReadOnlyCollection<DynamicCard> enemySpells,
        IEnumerable<DynamicCard> spells,
        IReadOnlyCollection<DynamicCard> revivable,
        int minTributeCount,
        bool hasTributeMonster,
        bool hasThreateningEnemy,
        bool isFirstTurn,
        bool hasSummonable,
        bool hasStrongSummoned,
        bool isDefenseless,
        bool hasAdvantage)
    {
        bool hasDisruptMonster = ownMonsters.Any(IsDisruptMonsters);
        if (card.IsCode(KaibaCardId.TorrentialTribute) && !hasDisruptMonster)
            return (int)CardScore.Perfect;

        if (card.IsCode(KaibaCardId.ThunderDragon))
            return (int)CardScore.Perfect;

        if (card.IsCode(KaibaCardId.PotOfGreed, KaibaCardId.GracefulCharity, KaibaCardId.UpstartGoblin))
            return (int)CardScore.Perfect;

        if (card.IsCode(KaibaCardId.MysticalSpaceTyphoon))
            return enemySpells.Count > 0 ? (int)CardScore.Perfect : (int)CardScore.Discourage;

        if (card.IsCode(KaibaCardId.HeavyStorm))
            return spells.Count() < enemySpells.Count ? (int)CardScore.Perfect : (int)CardScore.Discourage;

        if (isDefenseless && card.IsCode(KaibaCardId.MonsterReborn))
            return hasThreateningEnemy ? (int)CardScore.High : (int)CardScore.Moderate;

        if (hasThreateningEnemy && card.IsCode(KaibaCardId.TributeToTheDoomed, KaibaCardId.Raigeki, KaibaCardId.DarkHole))
            return hasThreateningEnemy ? (int)CardScore.High : (int)CardScore.Discourage;

        if (hasTributeMonster && card.IsCode(KaibaCardId.SoulExchange, KaibaCardId.SnatchSteal, KaibaCardId.ChangeOfHeart))
            return enemyMonsters.Count >= (minTributeCount - ownMonsters.Count(monster => monster.Level <= 4))
                ? (int)CardScore.Perfect : (int)CardScore.Discourage;

        if (hasAdvantage && card.IsCode(KaibaCardId.SnatchSteal))
            return enemyMonsters.Count > 0 ? (int)CardScore.High : (int)CardScore.Discourage;

        if (card.IsCode(KaibaCardId.NoblemanOfCrossout))
            return allEnemyMonsters.Any(monster => !monster.IsFaceup() && monster.IsDefense()) ? 250 : -1;

        if (card.IsCode(KaibaCardId.DelinquentDuo))
            return isFirstTurn ? 450 : -1;

        if (card.IsCode(KaibaCardId.TheForcefulSentry, KaibaCardId.Confiscation))
            return 350;

        if (card.IsCode(KaibaCardId.BarrelDragon))
            return 200;
        
        if (card.IsCode(KaibaCardId.PrematureBurial, KaibaCardId.PrematureBurial2))
            return hasSummonable && hasTributeMonster ? (int)CardScore.Perfect : 
            revivable.Count() > 0 ? (int)CardScore.High : (int)CardScore.Discourage;
        
        if (card.IsCode(KaibaCardId.AxeOfDespair) && hasStrongSummoned)
            return (int)CardScore.Perfect;

        return -1;
    }

    private void ConfigureMainActivation(DynamicCard card)
    {
        if (card.IsCode(KaibaCardId.GracefulCharity))
        {
            SetPendingAction(
                PendingActionKind.Discard,
                card.ID,
                MainAction.Activate,
                contextCode: (int)KaibaPendingAction.GracefulCharity);
        }
        else if (card.IsCode(KaibaCardId.TributeToTheDoomed))
        {
            SetPendingAction(
                PendingActionKind.Discard,
                card.ID,
                MainAction.Activate,
                contextCode: (int)KaibaPendingAction.TributeToDoomed);
        }
        else if (card.IsCode(KaibaCardId.MysticalSpaceTyphoon))
        {
            SetPendingAction(
                PendingActionKind.TargetSelection,
                card.ID,
                MainAction.Activate,
                contextCode: (int)KaibaPendingAction.MysticalSpaceTyphoon);
        }
        else if (card.IsCode(KaibaCardId.ChangeOfHeart))
        {
            SetPendingAction(
                PendingActionKind.TargetSelection,
                card.ID,
                MainAction.Activate,
                contextCode: (int)KaibaPendingAction.ChangeOfHeart);
        }
        else if (card.IsCode(KaibaCardId.SnatchSteal))
        {
            SetPendingAction(
                PendingActionKind.TargetSelection,
                card.ID,
                MainAction.Activate,
                contextCode: (int)KaibaPendingAction.SnatchSteal);
        }
        else if (card.IsCode(KaibaCardId.SoulExchange))
        {
            SetPendingAction(
                PendingActionKind.TargetSelection,
                card.ID,
                MainAction.Activate,
                contextCode: (int)KaibaPendingAction.SoulExchange);
        }
        else if (card.IsCode(KaibaCardId.NoblemanOfCrossout))
        {
            SetPendingAction(
                PendingActionKind.TargetSelection,
                card.ID,
                MainAction.Activate,
                contextCode: (int)KaibaPendingAction.NoblemanOfCrossout);
        }
        else if (card.IsCode(KaibaCardId.BarrelDragon))
        {
            SetPendingAction(
                PendingActionKind.TargetSelection,
                card.ID,
                MainAction.Activate,
                contextCode: (int)KaibaPendingAction.BarrelDragon);
        }
        else if (card.IsCode(KaibaCardId.ThunderDragon))
        {
            SetPendingAction(
                PendingActionKind.Search,
                card.ID,
                MainAction.Activate,
                contextCode: (int)KaibaPendingAction.SearchThunderDragon);
        }
        else if (card.IsCode(KaibaCardId.MonsterReborn))
        {
            SetPendingAction(
                PendingActionKind.Revive,
                card.ID,
                MainAction.Activate,
                contextCode: (int)KaibaPendingAction.MonsterReborn);
        }
        else if (card.IsCode(KaibaCardId.AxeOfDespair))
        {
            SetPendingAction(
                PendingActionKind.TargetSelection,
                card.ID,
                MainAction.Activate,
                contextCode: (int)KaibaPendingAction.AxeOfDespair);
        }
    }

    private void TrackTributeSummon(DynamicCard card)
    {
        if (card.ID > 0 && card.Level >= 5) {
            _pendingTributeSummonAttack = card.Attack;
        }
    }

    private static byte[] SelectKaibaTarget(
        IReadOnlyList<DynamicCard> selectable,
        KaibaPendingAction pendingAction,
        IReadOnlyList<DynamicCard> ownMonsters,
        IReadOnlyList<DynamicCard> enemySpells,
        int Maximum
    )
    {
        var candidates = selectable
            .Select((card, index) => (Card: card, Index: index))
            .Where(item => !IsKaibaDefensiveMonster(item.Card));
            
        var ownMonsterWithHighestAttack = ownMonsters
            .Select(card => card.Attack)
            .DefaultIfEmpty(0)
            .Max();
        
        var enemySpellsOnBoard = enemySpells
            .Select(card => (Card: card, Score: card.IsFaceup() ? (int)CardScore.Perfect : (int)CardScore.Low))
            .OrderByDescending(item => item.Score)
            .Select(item => item.Card)
            .LastOrDefault();

        if (pendingAction == KaibaPendingAction.MysticalSpaceTyphoon)
            candidates = candidates.Where(item => enemySpellsOnBoard.ID == item.Card.ID);

        if (pendingAction is KaibaPendingAction.ChangeOfHeart or
            KaibaPendingAction.SnatchSteal or
            KaibaPendingAction.SoulExchange or
            KaibaPendingAction.RingOfDestruction or
            KaibaPendingAction.NoblemanOfCrossout or
            KaibaPendingAction.TributeToDoomed or
            KaibaPendingAction.CallOfTheHaunted or
            KaibaPendingAction.PrematureBurial)
        {
            candidates = candidates.Where(item => IsOwnedByPlayer(item.Card));
        }

        if (pendingAction == KaibaPendingAction.RingOfDestruction)
            candidates = candidates.Where(item => item.Card.IsFaceup() && IsOwnedByPlayer(item.Card));

        if (pendingAction == KaibaPendingAction.NoblemanOfCrossout)
            candidates = candidates.Where(item => !item.Card.IsFaceup() && item.Card.IsDefense() && IsOwnedByPlayer(item.Card));

        if (pendingAction == KaibaPendingAction.CallOfTheHaunted)
            candidates = candidates.Where(item => IsOwnedByAI(item.Card));
        else if (pendingAction != KaibaPendingAction.RingOfDestruction)
            candidates = candidates.Where(item => IsOwnedByPlayer(item.Card));

        var sorted = candidates
            .OrderByDescending(item => item.Card.IsFaceup())
            .ThenByDescending(item => item.Card.Attack)
            .ToList();
        
        var targets = sorted
            .Take(Math.Min(Maximum, selectable.Count));

        PrintCandidates(sorted, "Sorted");
        PrintCandidates(targets.ToList(), "Targets");

        return targets.Select(item => (byte)item.Index).ToArray();
    }

    private static void PrintCandidates(List<(DynamicCard Card, int Index)> candidates, string title)
    {
        ConsoleUtils.PrintColored($"===Candidates [{title}]===");
        foreach (var target in candidates)
        {
            ConsoleUtils.PrintColored($"Target [{GetKaibaCardName(target.Card)}] | Owner [{target.Card.Owner}] | Attack [{target.Card.Attack}]");
        }
    }

    public static bool IsOwnedByAI(DynamicCard card) =>
        card.Owner == 0;
    
    public static bool IsOwnedByPlayer(DynamicCard card) =>
        !IsOwnedByAI(card);

    private static bool IsKaibaFusionCoreCard(DynamicCard card) =>
        card.IsCode(
            KaibaCardId.BlueEyesWhiteDragon,
            KaibaCardId.ThunderDragon,
            KaibaCardId.VersagoTheDestroyer,
            KaibaCardId.GoddessWithTheThirdEye);

    private sealed class KaibaMatrixActionScorer : IActionScorer
    {
        public ScoredAction Score<TState>(DecisionContext<TState> context, CandidateAction candidate)
        {
            int score = candidate.IsLegal &&
                        candidate.Metadata.TryGetValue("kaiba-score", out var value) &&
                        value is int matrixScore
                ? matrixScore
                : int.MinValue;

            string reason = candidate.Metadata.TryGetValue("kaiba-name", out var name) &&
                            name is string moveName
                ? moveName
                : "Kaiba heuristic";

            return new ScoredAction
            {
                Action = candidate,
                Score = score,
                Reasons = new[] { reason }
            };
        }
    }

    private int GetSanganPreferredOnHand(DynamicCard card, List<DynamicCard>? hand)
    {
        bool hasFusionSpell = hand?.Count > 0 && hand.Any(card => card.IsCode(KaibaCardId.Polymerization));
        int bewdCount = hand?.Count > 0 ? hand.Count(card => card.IsCode(KaibaCardId.BlueEyesWhiteDragon)) : 0;
        int thunderDragonCount = hand?.Count > 0 ? hand.Count(card => card.IsCode(KaibaCardId.ThunderDragon)) : 0;
        bool hasTributeMonster = hand?.Count > 0 && hand.Any(c => c.Level >= 5 && c.Attack >= MinimumTributeSummonAttack);

        bool isSubFusionMat = card.IsCode(KaibaCardId.VersagoTheDestroyer, KaibaCardId.GoddessWithTheThirdEye);
        bool isDisruptMonsters = IsDisruptMonsters(card);
        
        if (hasFusionSpell && isSubFusionMat && (bewdCount >= 2 || thunderDragonCount >= 1))
            return (int)CardScore.Perfect;
        if (hasTributeMonster && card.IsCode(KaibaCardId.GiantGerm))
            return (int)CardScore.Excellent;
        if (isDisruptMonsters)
            return (int)CardScore.High;

        return (int)CardScore.Discourage;
    }

    private int GetPenguinPreferredOnHand(DynamicCard card, List<DynamicCard>? _hand)
    {
        if (IsKaibaDefensiveMonster(card))
            return (int)CardScore.Discourage;
        return card.Attack;
    }

    private static bool IsDisruptMonsters(DynamicCard card) => card.IsCode(
            KaibaCardId.CyberJar,
            KaibaCardId.PenguinSoldier,
            KaibaCardId.ManEaterBug
        );

    public void PreviewCards(List<DynamicCard>? cards, string title, bool showExtraInfo = false)
    {
        if (cards != null)
            PreviewCards(cards.ToArray(), title, showExtraInfo);
    }

    public void PreviewCards(IEnumerable<DynamicCard>? cards, string title, bool showExtraInfo = false)
    {
        if (cards != null)
            PreviewCards(cards.ToArray(), title, showExtraInfo);
    }

    public void PreviewCards(DynamicCard[]? cards, string title, bool showExtraInfo = false)
    {
        if (cards == null) return;
        var y = 1;
        ConsoleUtils.PrintColored($"============= {title} ===============");
        foreach (var card in cards)
        {
            bool isMonster = card.HasType(CardType.Monster);
            string pos = card.IsAttack() ? "Attack" : "Defense";
            string extraInfo = isMonster && showExtraInfo ? $" | Position [{pos}]" : "";
            ConsoleUtils.PrintColored($" -> {y} Card: [{GetKaibaCardName(card)}] {extraInfo}");
            y++;
        }
        ConsoleUtils.PrintColored("========================================");
    }

    public int ScoreSettableMonster(DynamicCard card, bool weakOrEmptyMonsterField, bool hasStrongSummon, bool hasThreateningEnemy)
    {
        bool isDisruptMonsters = IsDisruptMonsters(card);
        if (hasThreateningEnemy && isDisruptMonsters)
            return (int)CardScore.Excellent;
        if (isDisruptMonsters)
            return (int)CardScore.Discourage;
        if (weakOrEmptyMonsterField && !hasStrongSummon)
            return (int)CardScore.High;
        if (weakOrEmptyMonsterField && card.Attack < StrongSummonAttack)
            return (int)CardScore.Low;
        return (int)CardScore.Discourage;
    }

    private static CardPosition PickPosition(SelectPositionData state, CardPosition preferred)
    {
        var positions = state.Positions ?? Array.Empty<byte>();
        if (positions.Contains((byte)preferred))
            return preferred;

        return positions.Length > 0 ? (CardPosition)positions[0] : CardPosition.FaceUpAttack;
    }

    private static int GetExpectedAttack(int cardId)
    {
        return cardId switch
        {
            KaibaCardId.VorseRaider => 1900,
            KaibaCardId.LaJinn => 1800,
            _ => 0
        };
    }

    private bool IsCrushCardVirusTribute(DynamicCard monster)
    {
        return (monster.IsFaceup() || monster.IsFacedown()) && 
            monster.IsCode(KaibaCardId.GiantGerm, KaibaCardId.SaggiTheDarkClown, KaibaCardId.WallOfIllusion);
    }

    private string GetPendingAction()
    {
        return Enum.GetName((KaibaPendingAction)PendingAction.ContextCode) ?? "";
    }

    private bool IsAceMonsters(DynamicCard monster)
    {
        return monster.IsCode(KaibaCardId.BlueEyesWhiteDragon, KaibaCardId.SummonedSkull, KaibaCardId.BarrelDragon);
    }

    private bool IsHasAdvantage(List<DynamicCard> ownMonsters, List<DynamicCard> enemyMonsters)
    {
        bool hasAdvantage = false;
        var ownMonstersRankByAttack = ownMonsters.OrderByDescending(m => m.Attack).ToList();
        var enemyMonstersRankByAttack = enemyMonsters.OrderByDescending(m => m.Attack).ToList();

        var defeatedIndices = new HashSet<int>(); 

        foreach (var attacker in ownMonstersRankByAttack)
        {
            for (int i = 0; i < enemyMonstersRankByAttack.Count; i++)
            {
                if (defeatedIndices.Contains(i))
                    continue;

                var target = enemyMonstersRankByAttack[i];
                if (attacker.Attack > target.Attack) 
                {
                    defeatedIndices.Add(i);
                    break;
                }
            }
        }

        if (defeatedIndices.Count >= enemyMonstersRankByAttack.Count)
        {
            hasAdvantage = true;
        }
        return hasAdvantage;
    }
}
