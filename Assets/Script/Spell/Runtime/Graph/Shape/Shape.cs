using UnityEngine;

namespace Spell.Graph
{
    [NodeColor(0.2f, 0.5f, 0.2f)]
    public abstract class Shape : Node
    {
        public Expression<Vector3> Position = new FixedVector3();
        public Expression<float> Rotation = new FixedFloat();

        public abstract int GetTouchingColliders(Collider[] colliders, int mask, QueryTriggerInteraction queryTriggerInteraction);
    }
}
