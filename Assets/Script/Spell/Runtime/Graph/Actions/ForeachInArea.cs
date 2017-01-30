using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Action")]
    public class ForeachInArea : Action
    {
        private static Collider[] s_colliders = new Collider[256];

        public Shape Shape = null;
        public Expression<AllianceType> Alliance = new Alliance(AllianceType.Enemies);
        public List<Action> Actions = new List<Action>();

        public override void Execute()
        {
            if (Shape == null)
                return;

            int count = Shape.GetTouchingColliders(s_colliders, 0xFFFFFF, QueryTriggerInteraction.Ignore);

            for (var i = 0; i < count; ++i)
            {
                var collider = s_colliders[i];

                for (int j = 0; j < Actions.Count; ++j)
                {
                    Actions[j].Execute();
                }
            }
        }

    }
}
