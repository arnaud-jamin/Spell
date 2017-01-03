using UnityEngine;

namespace Spell.Graph
{
    [NodeColor(0.5f, 0.2f, 0.2f)]
    public abstract class Action : Node
    {
        protected GameObject m_owner;

        public abstract void Execute();
    }
}
