using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Action")]
    public class ForeachInArea : Node
    {
        private static Collider[] s_colliders = new Collider[256];

        public InAction In = new InAction();
        public InValue<Shape> Shape = new InValue<Shape>();

        public OutAction Loop = new OutAction();
        public OutAction Finished = new OutAction();
        public OutValue<GameObject> Iterator = new OutValue<GameObject>();

        public ForeachInArea()
        {
            In.Action = Execute;
        }

        private void Execute()
        {
            var shape = Shape.Value;
            if (shape == null)
                return;

            var count = shape.GetTouchingColliders(s_colliders, 0xFFFFFF, QueryTriggerInteraction.Ignore);
            for (var i = 0; i < count; ++i)
            {
                var collider = s_colliders[i];
                Iterator.Value = collider.attachedRigidbody.gameObject;
                Loop.Execute();
            }

            Finished.Execute();
        }
    }
}
