using System;
using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Vector")]
    [Name("Make Vector3")]
    public class Vector3Expression : Expression<Vector3>
    {
        public Expression<float> X = new FloatValue();
        public Expression<float> Y = new FloatValue();
        public Expression<float> Z = new FloatValue();

        public override Vector3 Evaluate()
        {
            return new Vector3(X.Evaluate(), Y.Evaluate(), Z.Evaluate());
        }
    }
}
