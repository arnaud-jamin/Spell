using System;

namespace Spell.Graph
{
    [NodeColor(0.2f, 0.2f, 0.5f)]
    public abstract class Expression : Node
    {
    }

    public abstract class Expression<T> : Expression
    {
        public abstract T Evaluate();
    }
}
