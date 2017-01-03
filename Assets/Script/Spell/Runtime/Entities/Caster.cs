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
        public Character source;
        public Character target;
        public Ability ability;
    }

    public class Caster : Component
    {
        private List<GameObject> m_summoned = new List<GameObject>();

        public List<GameObject> Summoned { get { return m_summoned; } }

        public void Initialize()
        {
        }

        public void Update()
        {
        }

        internal void Cast(int v, CastParam castParam)
        {
        }
    }
}
