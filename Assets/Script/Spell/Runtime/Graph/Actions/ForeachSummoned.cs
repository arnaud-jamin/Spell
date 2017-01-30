using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Action")]
    public class ForeachSummoned : Action
    {
        public Expression<GameObject> Source = new GameObjectValue();
        public List<Action> Actions = new List<Action>();
        public GameObjectValue Selection = new GameObjectValue();

        public override void Execute()
        {
            var source = Source.Evaluate();
            if (source == null)
                return;

            var caster = source.GetComponent<Spell.Caster>();

            for (var i = 0; i < caster.Summoned.Count; ++i)
            {
                var summoned = caster.Summoned[i];
                Selection.Value = summoned;

                for (int j = 0; j < Actions.Count; ++j)
                {
                    Actions[j].Execute();
                }
            }
        }
    }
}
