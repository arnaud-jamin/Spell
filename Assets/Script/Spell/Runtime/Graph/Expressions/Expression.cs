using System;

namespace Spell.Graph
{
    public abstract class Expression : Node
    {
    }

    public abstract class Expression<T> : Expression
    {
        public abstract T Evaluate();

        public override Type PrimitiveType { get { return typeof(T); } }
    }
}
