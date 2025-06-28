using System.Collections.Generic;
using UnityEngine;
using Evolution.Combat;

namespace Evolution.Data
{
    [CreateAssetMenu(fileName = "AbilityDatabase", menuName = "Evolution/Ability Database")]
    public class AbilityDatabase : ScriptableObject
    {
        public List<Ability> Abilities = new();
    }
}
