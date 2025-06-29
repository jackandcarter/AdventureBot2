using UnityEngine;

namespace Evolution.Combat
{
    public static class CombatFormulas
    {
        public static int CalculateDamage(Combatant attacker, Combatant defender, Ability ability)
        {
            if (ability == null) return 0;
            float atk = attacker.Attack + attacker.GetAttackBonus();
            float def = defender.Defense + defender.GetDefenseBonus();
            float multiplier = 1f;
            if (ability.Element != Element.None)
            {
                if (defender.Resistances != null && defender.Resistances.TryGetValue(ability.Element, out float m))
                    multiplier = m;
            }
            float raw = (atk + ability.Damage) * multiplier - def;
            return Mathf.Max(Mathf.RoundToInt(raw), 0);
        }

        public static int CalculateHealing(Combatant healer, Ability ability)
        {
            if (ability == null) return 0;
            float heal = ability.Heal;
            return Mathf.Max(Mathf.RoundToInt(heal), 0);
        }

        public static StatusEffect BuildStatusEffect(StatusEffect effect)
        {
            if (effect == null) return null;
            return new StatusEffect
            {
                EffectName = effect.EffectName,
                Remaining = effect.Remaining,
                DamagePerTurn = effect.DamagePerTurn,
                HealPerTurn = effect.HealPerTurn,
                SpeedUp = effect.SpeedUp,
                SpeedDown = effect.SpeedDown,
                AttackBonus = effect.AttackBonus,
                DefenseBonus = effect.DefenseBonus
            };
        }
    }
}
