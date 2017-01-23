using System;
using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Action")]
    public class If : Action
    {
        public Expression<bool> Condition = new ExpressionValue<bool>(false);

        [ParameterSide(ParameterSide.Right)]
        public Action True = null;

        [ParameterSide(ParameterSide.Right)]
        public Action False = null;

        public override void Execute()
        {
            if (Condition.Evaluate())
            {
                if (True != null)
                {
                    True.Execute();
                }
            }
            else
            {
                if (False != null)
                {
                    False.Execute();
                }
            }
        }
    }
}
