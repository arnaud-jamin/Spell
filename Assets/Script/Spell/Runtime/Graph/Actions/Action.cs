using UnityEngine;

namespace Spell.Graph
{
    public abstract class Action : Node
    {
        protected GameObject m_owner;

        public abstract void Execute();
    }
}
