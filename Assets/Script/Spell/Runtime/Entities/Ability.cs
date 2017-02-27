using UnityEngine;

namespace Spell
{
    public class Ability 
    {
        public struct CastEvent
        {
            public Ability ability;
            public Unit source;
            public Unit target;
        }

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
