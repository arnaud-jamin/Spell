using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Object/Field")]
    public class CircleField : Field
    {
        public Expression<float> Radius = new FloatValue();
    }
}
