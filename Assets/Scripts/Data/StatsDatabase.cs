using System.Collections.Generic;
using UnityEngine;

namespace Evolution.Data
{
    [System.Serializable]
    public class StatDefinition
    {
        public string Name;
        [TextArea]
        public string Description;
        public float DefaultValue;
    }

    [CreateAssetMenu(fileName = "StatsDatabase", menuName = "Evolution/Stats Database")]
    public class StatsDatabase : ScriptableObject
    {
        public List<StatDefinition> Stats = new();
    }
}
