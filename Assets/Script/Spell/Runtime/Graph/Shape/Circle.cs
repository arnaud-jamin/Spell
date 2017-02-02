using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Shape")]
    public class Circle : Shape
    {
        public Expression<float> Radius = new FloatValue(1);

        public override int GetTouchingColliders(Collider[] colliders, int mask, QueryTriggerInteraction queryTriggerInteraction)
        {
            var position = Position.Evaluate();
            //var rotation = Rotation.Evaluate();
            var radius = Radius.Evaluate();
            return Physics.OverlapSphereNonAlloc(position, radius, colliders);
        }

        public override Collider CreateCollider(GameObject gameObject)
        {
            var collider = gameObject.AddComponent<SphereCollider>();
            collider.radius = Radius.Evaluate();
            return collider;
        }

        public override void UpdateCollider(Collider collider)
        {
            if (collider is SphereCollider)
            {
                ((SphereCollider)collider).radius = Radius.Evaluate();
            }
        }
    }
}
