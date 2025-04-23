using OmegaBotWrapper.Enum;
using OmegaBotWrapper.Struct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmegaBotWrapper.Game.AI.Actions
{
    public class DefaultActions
    {
        /// <summary>
        /// Decide whether to declare attack between attacker and defender.
        /// Can be overrided to update the RealPower of attacker for cards like Honest.
        /// </summary>
        /// <param name="attacker">Card that attack.</param>
        /// <param name="defender">Card that defend.</param>
        /// <returns>false if the attack shouldn't be done.</returns>
        public async Task<bool> OnPreBattleBetween(DynamicCard attacker, DynamicCard defender)
        {
            //await GameField.Initialize();

            var fieldInfo = DuelBotWrapper.GetPlayerFieldInfo();

            CardInfo Bot = fieldInfo.Result.EnemySide;

            CardInfo Player = fieldInfo.Result.OwnSide;

            Console.WriteLine($"OnPreBattleBetween called!");

            if (!attacker.IsMonsterHasPreventActivationEffectInBattle())
            {
                if (defender.IsMonsterInvincible() && defender.IsDefense())
                    return false;

                if (defender.IsMonsterDangerous())
                {
                    bool canIgnoreIt = !attacker.IsDisabled() && (
                        attacker.IsCode(CardId.UltimateConductorTytanno) && defender.IsDefense() ||
                        attacker.IsCode(CardId.ElShaddollConstruct) && defender.IsSpecialSummoned ||
                        attacker.IsCode(CardId.AllyOfJusticeCatastor) && !defender.HasAttribute(CardAttribute.Dark));
                    if (!canIgnoreIt)
                        return false;
                }

                foreach (DynamicCard equip in defender.EquipCards)
                {
                    if (equip.IsCode(CardId.MoonMirrorShield) && !equip.IsDisabled())
                    {
                        return false;
                    }
                }

                if (!defender.IsDisabled())
                {
                    if (defender.IsCode(CardId.MekkKnightCrusadiaAstram) && defender.IsAttack() && attacker.IsSpecialSummoned)
                        return false;

                    if (defender.IsCode(CardId.CrystalWingSynchroDragon) && defender.IsAttack() && attacker.Level >= 5)
                        return false;

                    if (defender.IsCode(CardId.AllyOfJusticeCatastor) && !attacker.HasAttribute(CardAttribute.Dark))
                        return false;

                    if (defender.IsCode(CardId.NumberS39UtopiaTheLightning) && defender.IsAttack() && defender.HasXyzMaterial(2, CardId.Number39Utopia))
                        defender.Attack = 5000;

                    if (defender.IsCode(CardId.VampireFraeulein))
                        defender.Attack += (Bot.LifePoints > 3000) ? 3000 : (Player.LifePoints - 100);

                    if (defender.IsCode(CardId.InjectionFairyLily) && Player.LifePoints > 2000)
                        defender.Attack += 3000;
                }
            }

            if (!defender.IsMonsterHasPreventActivationEffectInBattle())
            {
                if (attacker.IsCode(CardId.NumberS39UtopiaTheLightning) && !attacker.IsDisabled() && attacker.HasXyzMaterial(2, CardId.Number39Utopia))
                    attacker.Attack = 5000;

                if (attacker.IsCode(CardId.EaterOfMillions) && !attacker.IsDisabled())
                    attacker.Attack = 9999;

                if (attacker.IsMonsterInvincible())
                    attacker.Attack = 9999;

                foreach (DynamicCard equip in attacker.EquipCards)
                {
                    if (equip.IsCode(CardId.MoonMirrorShield) && !equip.IsDisabled())
                    {
                        attacker.Attack = defender.Attack + 100;
                    }
                }
            }

            IEnumerable<DynamicCard> EnemiesMonstersZone = null;// await GameField.GetMonsterInLocation(CardLocation.MonsterZone);

            if (EnemiesMonstersZone.TryGetByID(CardId.MekkKnightCrusadiaAstram, out _) && !defender.IsCode(CardId.MekkKnightCrusadiaAstram))
                return false;

            if (EnemiesMonstersZone.TryGetByID(CardId.DupeFrog, out _) && !defender.IsCode(CardId.DupeFrog))
                return false;

            if (EnemiesMonstersZone.TryGetByID(CardId.MaraudingCaptain, out _) && !defender.IsCode(CardId.MaraudingCaptain) && defender.Race.Equals(System.Enum.GetValues(CardRace.Warrior.GetType())))
                return false;

            if (defender.IsCode(CardId.UltimayaTzolkin) && !defender.IsDisabled() && EnemiesMonstersZone.Any(monster => !monster.Equals(defender) && monster.HasType(CardType.Synchro)))
                return false;

            if (EnemiesMonstersZone.Any(monster => !monster.Equals(defender) && monster.IsCode(CardId.HamonLordofStrikingThunder) && !monster.IsDisabled() && monster.IsDefense()))
                return false;

            if (defender.OwnTargets.Any(card => card.IsCode(CardId.PhantomKnightsFogBlade) && !card.IsDisabled()))
                return false;

            return true;
        }
    }
}
