using System.Collections.Generic;
using UnityEngine;
using Evolution.Data;
using Evolution.Combat;

namespace Evolution.Core
{
    /// <summary>
    /// Runtime model representing a player. Tracks current stats,
    /// experience and abilities. Stats are stored by name so they
    /// correspond with entries in the StatsDatabase.
    /// </summary>
    [System.Serializable]
    public class Player
    {
        public int Id;
        public string ClassName;
        public int Level = 1;
        public float Experience;
        public float CurrentHp;
        public Dictionary<string, float> Stats = new();
        public List<Ability> Abilities = new();

        public float GetStat(string name)
        {
            Stats.TryGetValue(name, out float val);
            return val;
        }

        public void SetStat(string name, float value)
        {
            Stats[name] = value;
        }

        public void AddExperience(float amount, StatsDatabase db)
        {
            Experience += amount;
            while (Experience >= RequiredForNextLevel())
            {
                Experience -= RequiredForNextLevel();
                LevelUp(db);
            }
        }

        private float RequiredForNextLevel()
        {
            // very basic curve: 100 xp per level
            return Level * 100f;
        }

        private void LevelUp(StatsDatabase db)
        {
            Level++;
            if (db != null)
            {
                foreach (var def in db.Stats)
                {
                    if (def == null) continue;
                    if (Stats.ContainsKey(def.Name))
                        Stats[def.Name] += def.DefaultValue;
                    else
                        Stats[def.Name] = def.DefaultValue;
                }
            }
            // heal to full on level up
            CurrentHp = GetStat("MaxHp");
        }

        public Combatant ToCombatant()
        {
            var c = new Combatant
            {
                Id = Id,
                Hp = Mathf.RoundToInt(CurrentHp > 0 ? CurrentHp : GetStat("MaxHp")),
                MaxHp = Mathf.RoundToInt(GetStat("MaxHp")),
                Attack = Mathf.RoundToInt(GetStat("Attack")),
                Defense = Mathf.RoundToInt(GetStat("Defense")),
                Speed = GetStat("Speed"),
                Abilities = new List<Ability>(Abilities)
            };
            return c;
        }
    }
}
