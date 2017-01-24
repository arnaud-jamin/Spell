using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Shape")]
    public class Circle : Node
    {
        public InValue<Vector3> Position = new InValue<Vector3>();
        public InValue<float> Radius = new InValue<float>(1.0f);
        public OutValue<CircleShape> Shape = new OutValue<CircleShape>();

        private CircleShape m_circleShape = new CircleShape();

        public Circle()
        {
            Shape.Func = () =>
            {
                m_circleShape.Position = Position.Value;
                m_circleShape.Radius = Radius.Value;
                return m_circleShape;
            };
        }
    }
}
