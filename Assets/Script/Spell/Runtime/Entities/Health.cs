using System;
using Spell.Graph;
using UnityEngine;

namespace Spell
{
    public class Health : MonoBehaviour
    {
        //---------------------------------------------------------------------------------------
        public struct Modifier
        {
            public GameObject source;
            public Ability ability;
            public float amount;
            public bool canResurrect;
            public DamageType damageType;
        }

        //---------------------------------------------------------------------------------------
        public struct HealthEvent
        {
            public GameObject source;
            public GameObject target;
            public Ability ability;
            public float amount;
            public float affectedAmount;
            public float oldHealth;
            public float newHealth;
        }

        //---------------------------------------------------------------------------------------
        [SerializeField]
        private bool m_isDead = false;

        [SerializeField]
        private bool m_isInvincible = false;

        [SerializeField]
        private float m_currentHealth = 100;

        [SerializeField]
        private float m_maxHealth = 100;

        //---------------------------------------------------------------------------------------
        public float CurrentHealth { get { return m_currentHealth; } set { m_currentHealth = value; } }
        public float MaxHealth { get { return m_maxHealth; } set { m_maxHealth = value; } }
        public bool IsInvincible { get { return m_isInvincible; } set { ChangeInvincible(value); } }
        public float HealthRatio { get { return m_maxHealth > 0 ? m_currentHealth / (float)m_maxHealth : 0; } }
        public bool IsDead { get { return m_isDead; } }
        
        //---------------------------------------------------------------------------------------
        public void Awake()
        {
        }

        //---------------------------------------------------------------------------------------
        public void Modify(Modifier modifier)
        {
            var healthEvent = new HealthEvent();
            healthEvent.source = modifier.source;
            healthEvent.ability = modifier.ability;
            healthEvent.oldHealth = m_currentHealth;
            healthEvent.newHealth = m_currentHealth;
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

            var previousHeath = m_currentHealth;
            m_currentHealth += modifier.amount;
            m_currentHealth = Mathf.Clamp(m_currentHealth, 0, m_maxHealth);
            healthEvent.affectedAmount = m_currentHealth - previousHeath;

            healthEvent.target = gameObject;
            healthEvent.newHealth = m_currentHealth;

            if (modifier.amount > 0)
            {
                GameManager.CombatLogManager.Log(CombatLog.Heal(healthEvent));
            }
            else
            {
                GameManager.CombatLogManager.Log(CombatLog.Damage(healthEvent));
            }

            if (m_currentHealth == 0)
            {
                Die(healthEvent);
            }
        }

        //---------------------------------------------------------------------------------------
        private void Die(HealthEvent healthEvent)
        {
            m_isDead = true;
            GameManager.CombatLogManager.Log(CombatLog.Death(healthEvent));
        }

        //---------------------------------------------------------------------------------------
        private void ChangeInvincible(bool value)
        {
            if ((value) && m_isDead)
            {
                Modify(new Modifier { amount = m_maxHealth, canResurrect = true, ability = null, source = null });
            }

            m_isInvincible = value;
        }

        //---------------------------------------------------------------------------------------
        public bool CanBeHealed(bool canResurect = false)
        {
            if (m_currentHealth == m_maxHealth)
                return false;

            if (m_isDead && canResurect == false)
                return false;

            return true;
        }


    }
}
