using UnityEngine;

namespace Evolution.Combat
{
    [CreateAssetMenu(fileName = "NewAbility", menuName = "Evolution/Ability")]
    public class Ability : ScriptableObject
    {
        public string Name;
        public int Damage;
        public int Heal;
        public StatusEffect Effect;
        public float Cooldown;
        public bool TargetSelf;
    }
}
