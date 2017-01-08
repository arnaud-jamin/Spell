using System;
using UnityEngine;

namespace Spell.Graph
{
    public abstract class Node : INode
    {
        public Vector2 GraphPosition { get; set; }
        public virtual bool IsFixedValue { get { return false; } }
        public virtual bool IsAttached { get; set; }
        public virtual Type ValueType { get { return null; } }
        public virtual object BoxedValue { get { return null; }  set { } }
    }
}
