using System.Collections.Generic;
using UnityEngine;

namespace Evolution.Data
{
    [CreateAssetMenu(fileName = "StatsDatabase", menuName = "Evolution/Stats Database")]
    public class StatsDatabase : ScriptableObject
    {
        public List<StatDefinition> Stats = new();
    }
}
