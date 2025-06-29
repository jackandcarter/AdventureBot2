using UnityEngine;

namespace Evolution.Data
{
    [CreateAssetMenu(fileName = "StatDefinition", menuName = "Evolution/Stat Definition")]
    public class StatDefinition : ScriptableObject
    {
        public string Name;
        [TextArea]
        public string Description;
        public float DefaultValue;
    }
}
