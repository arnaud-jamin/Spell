using System;
using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Math")]
    public class Add : Expression<float>
    {
        public Expression<float> A = new FloatValue();
        public Expression<float> B = new FloatValue();

        public override float Evaluate()
        {
            return A.Evaluate() + B.Evaluate();
        }
    }

    [NodeMenuItem("Math")]
    public class Sub : Expression<float>
    {
        public Expression<float> A = new FloatValue();
        public Expression<float> B = new FloatValue();

        public override float Evaluate()
        {
            return A.Evaluate() - B.Evaluate();
        }
    }

    [NodeMenuItem("Math")]
    public class Mult : Expression<float>
    {
        public Expression<float> A = new FloatValue();
        public Expression<float> B = new FloatValue();

        public override float Evaluate()
        {
            return A.Evaluate() * B.Evaluate();
        }
    }

    [NodeMenuItem("Math")]
    public class Div : Expression<float>
    {
        public Expression<float> A = new FloatValue();
        public Expression<float> B = new FloatValue(1);

        public override float Evaluate()
        {
            return A.Evaluate() / B.Evaluate();
        }
    }
}
