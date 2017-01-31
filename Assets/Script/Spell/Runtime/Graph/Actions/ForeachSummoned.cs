using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Action")]
    public class ForeachSummoned : Action
    {
        public Expression<TargetType> Summoner = new ExpressionValue<TargetType>();
        public List<Action> Actions = new List<Action>();
        public ExpressionValue<IteratorType> Iterator = new ExpressionValue<IteratorType>();

        public override void Execute()
        {
            var summoner = GameManager.GetTarget(Summoner.Evaluate());
            if (summoner == null)
                return;

            var caster = summoner.GetComponent<Spell.Caster>();
            if (caster == null)
                return;

            for (var i = 0; i < caster.Summoned.Count; ++i)
            {
                var summoned = caster.Summoned[i];
                Graph.SetIteratorValue(Iterator.Evaluate(), summoned);

                for (int j = 0; j < Actions.Count; ++j)
                {
                    Actions[j].Execute();
                }
            }
        }
    }
}
