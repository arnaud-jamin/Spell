using System;
using UnityEngine;

namespace Spell
{
    [CreateAssetMenu(fileName = "StatsSettings", menuName = "Spell/Stats Settings")]
    public class StatsSettings : ScriptableObject
    {
        public float DamagePerStrength = 1;
        public float DamagePerAgility = 1;
        public float DamagePerIntelligence = 1;

        public float HealthPerStrength = 20;
        public float HealthRegenPerStrength = 0.03f;
        
        public float ArmorPerAgility = 0.14f;
        public float AttackSpeedPerAgility = 1.0f;

        public float ManaPerIntelligence = 12;
        public float ManaRegenPerIntelligence = 0.04f;
        public float SpellDamagePerIntelligence = 0.0625f;
    }
}
