using System;
using UnityEngine;

namespace Spell.Graph
{
    public abstract class SimpleAction : Node
    {
        public InAction In = new InAction();
        public OutAction Out = new OutAction();

        public abstract void Execute();

        public SimpleAction()
        {
            In.Action = () =>
            {
                Execute();
                Out.Execute();
            };
        }
    }
}
