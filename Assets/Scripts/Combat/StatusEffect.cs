using System;
using UnityEngine;

namespace Evolution.Combat
{
    [Serializable]
    public class StatusEffect
    {
        public string EffectName;
        public int Remaining;
        public int DamagePerTurn;
        public int HealPerTurn;
        public float SpeedUp;
        public float SpeedDown;
    }
}
