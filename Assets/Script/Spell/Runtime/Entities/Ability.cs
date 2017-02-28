using UnityEngine;

namespace Spell
{
    public class Ability 
    {
        //-----------------------------------------------------------------------------------------
        public struct CastEventArgs
        {
            public Ability ability;
            public CastParam param;
        }

        //-----------------------------------------------------------------------------------------
        public struct CastParam
        {
            public int abilityIndex;
            public Unit source;
            public Unit target;
            public Vector3 castPosition;
        }

        //-----------------------------------------------------------------------------------------
        private Graph.Ability m_archetype;
        private float m_remainingCooldown;
        private float m_cooldown;

        //-----------------------------------------------------------------------------------------
        public Graph.Ability Archetype { get { return m_archetype; } }
        public float Cooldown { get { return m_cooldown; } }
        public float CooldownRatio { get { return m_remainingCooldown / m_cooldown; } }

        //-----------------------------------------------------------------------------------------
        public Ability(Graph.Ability archetype)
        {
            m_archetype = archetype;
            m_cooldown = m_archetype.Cooldown;
            m_remainingCooldown = 0;
        }

        //-----------------------------------------------------------------------------------------
        public void Cast(CastParam param)
        {
            m_remainingCooldown = m_archetype.Cooldown;
        }

        //-----------------------------------------------------------------------------------------
        public void OnFixedUpdate()
        {
            m_remainingCooldown -= Mathf.Max(0, Time.fixedDeltaTime);
        }
    }
}
