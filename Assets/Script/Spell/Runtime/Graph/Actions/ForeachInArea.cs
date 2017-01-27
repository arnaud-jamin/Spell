using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Action")]
    public class ForeachInArea : ActionNode
    {
        private static Collider[] s_colliders = new Collider[256];

        public ForeachInArea()
        {
            var Shape = AddInValue<Shape>("Shape", null);
            var Loop = AddOutAction("Loop");
            var Finished = AddOutAction("Finished");
            var Iterator = AddOutValue<GameObject>("Iterator", null);

            AddInAction("In", () =>
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
            });
        }
    }
}
