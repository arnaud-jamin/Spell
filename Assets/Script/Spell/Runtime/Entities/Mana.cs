using System;
using Spell.Graph;
using UnityEngine;

namespace Spell
{
    public class Mana 
    {
        //---------------------------------------------------------------------------------------
        public struct Modifier
        {
            public Unit source;
            public Ability ability;
            public float amount;
            public bool canResurrect;
            public DamageType damageType;
        }

        //---------------------------------------------------------------------------------------
        public struct EventArgs
        {
            public Unit source;
            public Unit target;
            public Ability ability;
            public float amount;
            public float affectedAmount;
            public float oldValue;
            public float newValue;
        }

        //---------------------------------------------------------------------------------------
        private Unit m_unit;
        private Stat m_maxValue;
        private bool m_isDead = false;
        private bool m_isInvincible = false;
        private float m_currentValue = 100;

        //---------------------------------------------------------------------------------------
        public float CurrentValue { get { return m_currentValue; } set { m_currentValue = value; } }
        public float MaxValue { get { return m_maxValue.Value; } }
        public bool IsInvincible { get { return m_isInvincible; } set { ChangeInvincible(value); } }
        public float Ratio { get { return m_maxValue.Value > 0 ? m_currentValue / m_maxValue.Value : 0; } }
        public bool IsDead { get { return m_isDead; } }
        
        //---------------------------------------------------------------------------------------
        public Mana(Unit unit)
        {
            m_unit = unit;
            m_maxValue = unit.Stats.GetStat(StatType.Health);
            m_currentValue = m_maxValue.Value;
        }

        //---------------------------------------------------------------------------------------
        public void Modify(Modifier modifier)
        {
            var eventArgs = new EventArgs();
            eventArgs.source = modifier.source;
            eventArgs.ability = modifier.ability;
            eventArgs.oldValue = m_currentValue;
            eventArgs.newValue = m_currentValue;
            eventArgs.affectedAmount = 0;

            if (modifier.amount == 0)
                return;

            if (modifier.amount > 0)
            {
                if (CanBeHealed(modifier.canResurrect) == false)
                    return;

                m_isDead = false;
            }
            else if (modifier.amount < 0)
            {
                if (m_isDead || IsInvincible)
                    return;
            }

            var previousValue = m_currentValue;
            m_currentValue += modifier.amount;
            m_currentValue = Mathf.Clamp(m_currentValue, 0, m_maxValue.Value);
            eventArgs.affectedAmount = m_currentValue - previousValue;

            eventArgs.target = m_unit;
            eventArgs.newValue = m_currentValue;

            if (m_currentValue == 0)
            {
                Die(eventArgs);
            }
        }

        //---------------------------------------------------------------------------------------
        private void Die(EventArgs args)
        {
            m_isDead = true;
        }

        //---------------------------------------------------------------------------------------
        private void ChangeInvincible(bool value)
        {
            if ((value) && m_isDead)
            {
                Modify(new Modifier { amount = m_maxValue.Value, canResurrect = true, ability = null, source = null });
            }

            m_isInvincible = value;
        }

        //---------------------------------------------------------------------------------------
        public bool CanBeHealed(bool canResurect = false)
        {
            if (m_currentValue == m_maxValue.Value)
                return false;

            if (m_isDead && canResurect == false)
                return false;

            return true;
        }
    }
}
