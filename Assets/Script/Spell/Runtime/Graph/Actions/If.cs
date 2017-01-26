using System;
using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Action")]
    public class If : Node
    {
        public If()
        {
            var condition = AddInValue("Condition", false);
            var trueAction = AddOutAction("True");
            var falseAction = AddOutAction("False");

            AddInAction("In", () =>
            {
                if (condition.Value)
                {
                    trueAction.Execute();
                }
                else
                {
                    falseAction.Execute();
                }
            });
        }
    }
}
