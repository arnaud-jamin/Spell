using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Buff Actions")]
    public class Tick : BuffAction
    {
        public Expression<float> Time = new FloatValue(0.5f);
        public List<Action> Actions = new List<Action>();

        public override void OnUpdate()
        {
            for (int i = 0; i < Actions.Count; ++i)
            {
                Actions[i].Execute();
            }
        }
    }
}
