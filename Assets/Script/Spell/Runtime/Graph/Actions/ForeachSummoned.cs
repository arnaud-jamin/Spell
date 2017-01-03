using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Action/Composite/For Each Summoned")]
    public class ForeachSummoned : Action
    {
        public Expression<GameObject> Source = new FixedGameObject();
        public Action Action = null;
        public FixedGameObject Selection = new FixedGameObject();

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
                Action.Execute();
            }
        }
    }
}
