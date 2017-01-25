using System;
using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Shape")]
    public class Box : Node
    {
        private BoxShape m_boxShape = new BoxShape();

        public Box()
        {
            var position = AddInValue("Position", Vector3.zero);
            var rotation = AddInValue("Rotation", 1.0f);
            var width = AddInValue("Width", 1.0f);
            var height = AddInValue("Height", 1.0f);
            var shape = AddOutValue("Shape", null, () =>
            {
                m_boxShape.Position = position.Value;
                m_boxShape.Rotation = rotation.Value;
                m_boxShape.Width = width.Value;
                m_boxShape.Height = height.Value;
                return m_boxShape;
            });
        }
    }
}
