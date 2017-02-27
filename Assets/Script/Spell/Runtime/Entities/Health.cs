using System;
using Spell.Graph;
using UnityEngine;

namespace Spell
{
    public class Health : MonoBehaviour
    {
        //-----------------------------------------------------------------------------------------
        [Serializable]
        public class References
        {
            [AutoFind]
            public Stats stats;
        }

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
        public struct HealthEvent
        {
            public Unit source;
            public Unit target;
            public Ability ability;
            public float amount;
            public float affectedAmount;
            public float oldHealth;
            public float newHealth;
        }

        //---------------------------------------------------------------------------------------
        private Unit m_unit;
        private Stat m_maxLife;

        //---------------------------------------------------------------------------------------
        [SerializeField]
        private References m_references = null;

        [SerializeField]
        private bool m_isDead = false;

        [SerializeField]
        private bool m_isInvincible = false;

        [SerializeField]
        private float m_currentLife = 100;

        //---------------------------------------------------------------------------------------
        public float CurrentLife { get { return m_currentLife; } set { m_currentLife = value; } }
        public float MaxLife { get { return m_maxLife.Value; } }
        public bool IsInvincible { get { return m_isInvincible; } set { ChangeInvincible(value); } }
        public float LifeRatio { get { return m_maxLife.Value > 0 ? m_currentLife / m_maxLife.Value : 0; } }
        public bool IsDead { get { return m_isDead; } }
        
        //---------------------------------------------------------------------------------------
        public void Initialize(Unit unit)
        {
            m_unit = unit;
            m_maxLife = m_references.stats.GetStat(StatType.Health);
        }

        //---------------------------------------------------------------------------------------
        public void Modify(Modifier modifier)
        {
            var healthEvent = new HealthEvent();
            healthEvent.source = modifier.source;
            healthEvent.ability = modifier.ability;
            healthEvent.oldHealth = m_currentLife;
            healthEvent.newHealth = m_currentLife;
            healthEvent.affectedAmount = 0;

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

            var previousLife = m_currentLife;
            m_currentLife += modifier.amount;
            m_currentLife = Mathf.Clamp(m_currentLife, 0, m_maxLife.Value);
            healthEvent.affectedAmount = m_currentLife - previousLife;

            healthEvent.target = m_unit;
            healthEvent.newHealth = m_currentLife;

            if (modifier.amount > 0)
            {
                GameManager.Instance.CombatLogManager.Log(CombatLog.Heal(healthEvent));
            }
            else
            {
                GameManager.Instance.CombatLogManager.Log(CombatLog.Damage(healthEvent));
            }

            if (m_currentLife == 0)
            {
                Die(healthEvent);
            }
        }

        //---------------------------------------------------------------------------------------
        private void Die(HealthEvent healthEvent)
        {
            m_isDead = true;
            GameManager.Instance.CombatLogManager.Log(CombatLog.Death(healthEvent));
        }

        //---------------------------------------------------------------------------------------
        private void ChangeInvincible(bool value)
        {
            if ((value) && m_isDead)
            {
                Modify(new Modifier { amount = m_maxLife.Value, canResurrect = true, ability = null, source = null });
            }

            m_isInvincible = value;
        }

        //---------------------------------------------------------------------------------------
        public bool CanBeHealed(bool canResurect = false)
        {
            if (m_currentLife == m_maxLife.Value)
                return false;

            if (m_isDead && canResurect == false)
                return false;

            return true;
        }
    }
}
