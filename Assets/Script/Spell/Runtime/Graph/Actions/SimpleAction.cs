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

            AddInAction("In", () =>
            {
                Execute();
                outAction.Execute();
            });
        }
    }
}
