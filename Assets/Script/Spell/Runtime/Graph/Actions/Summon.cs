using System;
using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Action")]
    public class Summon : Action
    {
        public Expression<ObjectNode> Object = new FixedObject();
        public Expression<Vector3> Position = new FixedVector3();
        public Expression<float> Rotation = new FixedFloat();

        public override void Execute()
        {
        }
    }
}
