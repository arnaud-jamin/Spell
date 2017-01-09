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
    }
}
