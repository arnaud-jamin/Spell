using UnityEngine;

namespace Spell.Graph
{
    public abstract class Shape : Node
    {
        public Expression<Vector3> Position = new FixedVector3();
        public Expression<float> Rotation = new FixedFloat();

        public abstract int GetTouchingColliders(Collider[] colliders, int mask, QueryTriggerInteraction queryTriggerInteraction);
    }
}
