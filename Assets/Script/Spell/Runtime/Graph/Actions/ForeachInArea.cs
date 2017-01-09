using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Action/Composite")]
    public class ForeachInArea : Action
    {
        private static Collider[] s_colliders = new Collider[256];

        public Shape Shape = null;
        public Action Action = null;
        public GameObjectValue Selection = new GameObjectValue();

        public override void Execute()
        {
            if (Shape == null)
                return;

            int count = Shape.GetTouchingColliders(s_colliders, 0xFFFFFF, QueryTriggerInteraction.Ignore);

            for (var i = 0; i < count; ++i)
            {
                var collider = s_colliders[i];
                Selection.Value = collider.attachedRigidbody.gameObject;
                Action.Execute();
            }
        }

    }
}
