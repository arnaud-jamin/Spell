using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Action")]
    public class If : Action
    {
        public Expression<bool> Condition = null;
        public List<Action> True = null;
        public List<Action> False = null;

        public override void Execute()
        {
            if (Condition.Evaluate())
            {
                for (int j = 0; j < True.Count; ++j)
                {
                    True[j].Execute();
                }
            }
            else
            {
                for (int j = 0; j < False.Count; ++j)
                {
                    False[j].Execute();
                }
            }
        }

    }
}
