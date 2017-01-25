using System;
using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Action")]
    public class If : Node
    {
        public If()
        {
            var Condition = AddInValue("Condition", false);
            var True = AddOutAction("True");
            var False = AddOutAction("False");
            var inAction = AddInAction("In", () =>
            {
                if (Condition.Value)
                {
                    True.Execute();
                }
                else
                {
                    False.Execute();
                }
            });
        }
    }
}
