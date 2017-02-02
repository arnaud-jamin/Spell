using UnityEngine;

namespace Spell.Graph
{
    public abstract class Shape : Node
    {
        public Expression<Vector3> Position = new Vector3Value();

        public abstract int GetTouchingColliders(Collider[] colliders, int mask, QueryTriggerInteraction queryTriggerInteraction);

        public abstract Collider CreateCollider(GameObject gameObject);

        public abstract void UpdateCollider(Collider collider);
    }
}
