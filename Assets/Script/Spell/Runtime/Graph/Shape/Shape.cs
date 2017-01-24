using UnityEngine;

namespace Spell.Graph
{
    public abstract class Shape
    {
        public Vector3 Position;
        public float Rotation;

        public abstract int GetTouchingColliders(Collider[] colliders, int mask, QueryTriggerInteraction queryTriggerInteraction);
    }

    public class BoxShape : Shape
    {
        public float Width;
        public float Height;

        public override int GetTouchingColliders(Collider[] colliders, int mask, QueryTriggerInteraction queryTriggerInteraction)
        {
            return Physics.OverlapBoxNonAlloc(Position, new Vector3(Width, 1, Height), colliders, Quaternion.Euler(0, Rotation, 0), mask, queryTriggerInteraction);
        }
    }

    public class CircleShape : Shape
    {
        public float Radius;

        public override int GetTouchingColliders(Collider[] colliders, int mask, QueryTriggerInteraction queryTriggerInteraction)
        {
            return Physics.OverlapSphereNonAlloc(Position, Radius, colliders);
        }
    }
}
