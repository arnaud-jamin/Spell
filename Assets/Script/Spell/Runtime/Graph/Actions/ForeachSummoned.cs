using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Action")]
    public class ForeachSummoned : Action
    {
        public ExpressionValue<TargetType> Summoner = new ExpressionValue<TargetType>();
        public List<Action> Actions = new List<Action>();
        public ExpressionValue<IteratorType> Iterator = new ExpressionValue<IteratorType>();

        public override void Execute()
        {
            var summoner = GameManager.GetTarget(Summoner.Evaluate());
            if (summoner == null)
                return;

            for (var i = 0; i < summoner.Summoned.Count; ++i)
            {
                var summoned = summoner.Summoned[i];
                Graph.SetIteratorValue(Iterator.Evaluate(), summoned);

                for (int j = 0; j < Actions.Count; ++j)
                {
                    Actions[j].Execute();
                }
            }
        }
    }
}
