using System;
using UnityEngine;

namespace Spell.Graph
{
    public abstract class SimpleAction : Node
    {
        public abstract void Execute();

        public SimpleAction()
        {
            var outAction = AddOutAction("Out");
            var inAction = AddInAction("In", () =>
            {
                Execute();
                outAction.Execute();
            });
        }
    }
}
