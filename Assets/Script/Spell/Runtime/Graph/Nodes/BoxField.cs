using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Object/Field/BoxField")]
    public class BoxField : Field
    {
        public Expression<float> Width = new FixedFloat();
        public Expression<float> Height = new FixedFloat();
    }
}
