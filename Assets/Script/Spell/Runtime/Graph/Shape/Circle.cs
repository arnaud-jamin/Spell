using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Shape")]
    public class Circle : Node
    {
        private CircleShape m_circleShape = new CircleShape();

        public Circle()
        {
            var Position = AddInValue("Position", Vector3.zero);
            var Radius = AddInValue("Radius", 1.0f);
            var Shape = AddOutValue("Shape", null, () =>
            {
                m_circleShape.Position = Position.Value;
                m_circleShape.Radius = Radius.Value;
                return m_circleShape;
            });
        }
    }
}
