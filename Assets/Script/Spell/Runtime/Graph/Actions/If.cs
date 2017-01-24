using System;
using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Action")]
    public class If : Node
    {
        public InAction In = new InAction();
        public Expression<bool> Condition = new ExpressionValue<bool>(false);
        public OutAction True = new OutAction();
        public OutAction False = new OutAction();

        public If()
        {
            In.Action = Execute;
        }

        public void Execute()
        {
            if (Condition.Evaluate())
            {
                True.Execute();
            }
            else
            {
                False.Execute();
            }
        }
    }
}
