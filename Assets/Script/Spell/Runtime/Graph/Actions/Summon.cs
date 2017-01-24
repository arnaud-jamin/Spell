using System;
using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Action")]
    public class Summon : SimpleAction
    {
        public Expression<ObjectNode> Object = new ObjectValue();
        public Expression<Vector3> Position = new Vector3Value();
        public Expression<float> Rotation = new FloatValue();

        public override void Execute()
        {
        }
    }
}
