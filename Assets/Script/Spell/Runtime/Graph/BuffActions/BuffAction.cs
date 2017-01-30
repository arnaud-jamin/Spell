using UnityEngine;

namespace Spell.Graph
{
    public abstract class BuffAction : Node
    {
        protected GameObject m_owner;

        public virtual void OnStart() { }
        public virtual void OnUpdate() { }
        public virtual void OnStop() { }
    }
}
