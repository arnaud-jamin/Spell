using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Shape")]
    public class Box : Shape
    {
        public Expression<float> Width = new FloatValue(1);
        public Expression<float> Height = new FloatValue(1);
        public Expression<float> Rotation = new FloatValue();

        public override int GetTouchingColliders(Collider[] colliders, int mask, QueryTriggerInteraction queryTriggerInteraction)
        {
            var position = Position.Evaluate();
            var rotation = Rotation.Evaluate();
            var width = Width.Evaluate();
            var height = Height.Evaluate();
            return Physics.OverlapBoxNonAlloc(position, new Vector3(width, 1, height), colliders, Quaternion.Euler(0, rotation, 0), mask, queryTriggerInteraction);
        }
    }
}
