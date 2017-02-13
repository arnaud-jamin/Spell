using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spell
{
    public struct CastEventArgs
    {
        public float Amount;
        public float NewLife;
        public float OldLife;
        public float IsDead;
    }

    public struct CastParam
    {
        public Vector3 castPosition;
        public Vector3 effectPosition;
        public Unit source;
        public Unit target;
        public Ability ability;
    }

    public class Caster : MonoBehaviour
    {
        //-----------------------------------------------------------------------------------------
        [Serializable]
        public class References
        {
            [AutoFind]
            public Stats stats;
        }

        //-----------------------------------------------------------------------------------------
        private Stat m_maxMana;
        private List<GameObject> m_summoned = new List<GameObject>();

        //---------------------------------------------------------------------------------------
        [SerializeField]
        private References m_references = null;

        [SerializeField]
        private float m_currentMana = 100;

        //---------------------------------------------------------------------------------------
        public List<GameObject> Summoned { get { return m_summoned; } }

        //---------------------------------------------------------------------------------------
        public void Initialize()
        {
            m_maxMana = m_references.stats.GetStat(StatType.Mana);
        }

        //---------------------------------------------------------------------------------------
        public void Update()
        {
        }

        //---------------------------------------------------------------------------------------
        public void Cast(int v, CastParam castParam)
        {
            if (m_maxMana.Value < 0)
                return;

            if (m_currentMana < 100)
                return;
        }
    }
}
