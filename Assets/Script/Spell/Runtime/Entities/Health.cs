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
        private float m_currentLife = 100;

        [SerializeField]
        private float m_maxLife = 100;

        //---------------------------------------------------------------------------------------
        public float CurrentLife { get { return m_currentLife; } set { m_currentLife = value; } }
        public float MaxLife { get { return m_maxLife; } set { m_maxLife = value; } }
        public bool IsInvincible { get { return m_isInvincible; } set { ChangeInvincible(value); } }
        public float LifeRatio { get { return m_maxLife > 0 ? m_currentLife / (float)m_maxLife : 0; } }
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
            m_currentLife = Mathf.Clamp(m_currentLife, 0, m_maxLife);
            healthEvent.affectedAmount = m_currentLife - previousLife;

            healthEvent.target = gameObject;
            healthEvent.newHealth = m_currentLife;

            if (modifier.amount > 0)
            {
                GameManager.CombatLogManager.Log(CombatLog.Heal(healthEvent));
            }
            else
            {
                GameManager.CombatLogManager.Log(CombatLog.Damage(healthEvent));
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
            GameManager.CombatLogManager.Log(CombatLog.Death(healthEvent));
        }

        //---------------------------------------------------------------------------------------
        private void ChangeInvincible(bool value)
        {
            if ((value) && m_isDead)
            {
                Modify(new Modifier { amount = m_maxLife, canResurrect = true, ability = null, source = null });
            }

            m_isInvincible = value;
        }

        //---------------------------------------------------------------------------------------
        public bool CanBeHealed(bool canResurect = false)
        {
            if (m_currentLife == m_maxLife)
                return false;

            if (m_isDead && canResurect == false)
                return false;

            return true;
        }


    }
}
