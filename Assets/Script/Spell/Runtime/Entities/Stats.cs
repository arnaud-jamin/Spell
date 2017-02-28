using System;
using System.Collections.Generic;
using Spell.Graph;
using UnityEngine;

namespace Spell
{
    public enum PrimaryAttribute
    {
        Strength,
        Agility,
        Intelligence
    }

    public class Stats
    {
        // ----------------------------------------------------------------------------------------
        private Graph.Unit m_archetype = null;
        private int m_level = 1;
        private int m_experience = 0;
        private Stat m_strength;
        private Stat m_agility;
        private Stat m_intelligence;
        private Stat[] m_stats;

        //---------------------------------------------------------------------------------------
        public int Experience { get { return m_experience; } }

        //---------------------------------------------------------------------------------------
        public Stats(Graph.Unit archetype)
        {
            m_archetype = archetype;
            CreateStats();
        }

        // ----------------------------------------------------------------------------------------
        public Stat GetStat(StatType type)
        {
            return m_stats[(int)type];
        }

        // ----------------------------------------------------------------------------------------
        private void GainXp(int experience)
        {
            m_experience += experience;
        }

        // ----------------------------------------------------------------------------------------
        private void CreateStats()
        {
            int count = Utils.EnumCount<StatType>();
            m_stats = new Stat[count];

            m_strength      = AddStat(StatType.Strength);
            m_agility       = AddStat(StatType.Agility);
            m_intelligence  = AddStat(StatType.Intelligence);

            AddStat(StatType.DamageMin);
            AddStat(StatType.DamageMax);
            AddStat(StatType.Health);
            AddStat(StatType.HealthRegen);
            AddStat(StatType.Armor);
            AddStat(StatType.AttackSpeed);
            AddStat(StatType.Mana);
            AddStat(StatType.ManaRegen);
            AddStat(StatType.SpellDamage);

            AddStat(StatType.MoveSpeed, m_archetype.MoveSpeed);
            AddStat(StatType.TurnRate, m_archetype.TurnRate);
            AddStat(StatType.AttackRange, m_archetype.AttackRange);
            AddStat(StatType.ProjectileSpeed, m_archetype.ProjectileSpeed);
            AddStat(StatType.CollisionSize, m_archetype.CollisionSize);
            AddStat(StatType.VisionRange, m_archetype.VisionRange);
            AddStat(StatType.MagicResistance, m_archetype.MagicResistance);
        }

        // ----------------------------------------------------------------------------------------
        private Stat AddStat(StatType type, float value = 0)
        {
            var stat = new Stat(type, value);
            m_stats[(int)type] = stat;
            return stat;
        }

        // ----------------------------------------------------------------------------------------
        void OnLevelChanged()
        {
            m_strength.SetInitialValue(m_archetype.BaseStrength + m_archetype.StrengthGain * m_level);
            m_agility.SetInitialValue(m_archetype.BaseAgility + m_archetype.AgilityGain * m_level);
            m_intelligence.SetInitialValue(m_archetype.BaseIntelligence + m_archetype.IntelligenceGain * m_level);
        }

        // ----------------------------------------------------------------------------------------
        void OnStrengthChanged()
        {
            GetStat(StatType.Health).SetInitialValue(m_archetype.BaseHealth + m_strength.Value * Settings.Stats.HealthPerStrength);
            GetStat(StatType.HealthRegen).SetInitialValue(m_archetype.BaseHealthRegen + m_strength.Value * Settings.Stats.HealthRegenPerStrength);

            if (m_archetype.PrimaryAttribute == PrimaryAttribute.Strength)
            {
                GetStat(StatType.DamageMin).SetInitialValue(m_archetype.BaseAttackDamageMin + m_strength.Value * Settings.Stats.DamagePerStrength);
                GetStat(StatType.DamageMax).SetInitialValue(m_archetype.BaseAttackDamageMax + m_strength.Value * Settings.Stats.DamagePerStrength);
            }
        }

        // ----------------------------------------------------------------------------------------
        void OnAgilityChanged()
        {
            GetStat(StatType.Armor).SetInitialValue(m_archetype.BaseArmor + m_agility.Value * Settings.Stats.ArmorPerAgility);
            GetStat(StatType.AttackSpeed).SetInitialValue(m_agility.Value * Settings.Stats.AttackSpeedPerAgility);

            if (m_archetype.PrimaryAttribute == PrimaryAttribute.Agility)
            {
                GetStat(StatType.DamageMin).SetInitialValue(m_archetype.BaseAttackDamageMin + m_agility.Value * Settings.Stats.DamagePerAgility);
                GetStat(StatType.DamageMax).SetInitialValue(m_archetype.BaseAttackDamageMax + m_agility.Value * Settings.Stats.DamagePerAgility);
            }
        }

        // ----------------------------------------------------------------------------------------
        void OnIntelligenceChanged()
        {
            GetStat(StatType.Mana).SetInitialValue(m_intelligence.Value * Settings.Stats.ManaPerIntelligence);
            GetStat(StatType.ManaRegen).SetInitialValue(m_intelligence.Value * Settings.Stats.ManaRegenPerIntelligence);
            GetStat(StatType.SpellDamage).SetInitialValue(m_intelligence.Value * Settings.Stats.SpellDamagePerIntelligence);

            if (m_archetype.PrimaryAttribute == PrimaryAttribute.Intelligence)
            {
                GetStat(StatType.DamageMin).SetInitialValue(m_archetype.BaseAttackDamageMin + m_intelligence.Value * Settings.Stats.DamagePerIntelligence);
                GetStat(StatType.DamageMax).SetInitialValue(m_archetype.BaseAttackDamageMax + m_intelligence.Value * Settings.Stats.DamagePerIntelligence);
            }
        }
    }
}
