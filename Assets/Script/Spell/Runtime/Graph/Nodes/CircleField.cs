using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Object/Field/CircleField")]
    public class CircleField : Field
    {
        public Expression<float> Radius = new FixedFloat();
    }
}
