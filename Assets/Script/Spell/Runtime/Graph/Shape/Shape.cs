using UnityEngine;

namespace Spell.Graph
{
    public abstract class Shape : Node
    {
        public Expression<Vector3> Position = new Vector3Value();
        public Expression<float> Rotation = new FloatValue();

        public abstract int GetTouchingColliders(Collider[] colliders, int mask, QueryTriggerInteraction queryTriggerInteraction);
    }
}
