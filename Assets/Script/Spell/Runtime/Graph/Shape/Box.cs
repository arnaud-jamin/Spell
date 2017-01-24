using System;
using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Shape")]
    public class Box : Node
    {
        public InValue<Vector3> Position = new InValue<Vector3>();
        public InValue<float> Rotation = new InValue<float>();
        public InValue<float> Width = new InValue<float>(1.0f);
        public InValue<float> Height = new InValue<float>(1.0f);
        public OutValue<BoxShape> Shape = new OutValue<BoxShape>();

        private BoxShape m_boxShape = new BoxShape();

        public Box()
        {
            Shape.Func = () =>
            {
                m_boxShape.Position = Position.Value;
                m_boxShape.Rotation = Rotation.Value;
                m_boxShape.Width = Width.Value;
                m_boxShape.Height = Height.Value;
                return m_boxShape;
            };
        }
    }
}
