using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Shape")]
    public class Circle : Node
    {
        public override Color Color { get { return Graph.ShapeColor; } }

        private CircleShape m_circleShape = new CircleShape();

        public Circle()
        {
            var position = AddInValue("Position", Vector3.zero);
            var radius = AddInValue("Radius", 1.0f);

            AddOutValue<Shape>("Shape", null, () =>
            {
                m_circleShape.Position = position.Value;
                m_circleShape.Radius = radius.Value;
                return m_circleShape;
            });
        }
    }
}
