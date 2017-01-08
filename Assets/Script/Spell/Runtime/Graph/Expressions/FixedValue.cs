using System;
using UnityEngine;

namespace Spell.Graph
{
    public class FixedValue<T> : Expression<T>
    {
        public T Value;
        public bool isAttached = true;

        public FixedValue()
        {
            Value = default(T);
        }

        public FixedValue(T value)
        {
            Value = value;
        }

        public override T Evaluate()
        {
            return Value;
        }

        public override bool IsFixedValue { get { return true; } }

        public override bool IsAttached { get { return isAttached; } set { isAttached = value; } }

        public override object BoxedValue
        {
            get { return Value; }
            set { Value = (T)value; }
        }
    }

    [NodeMenuItem("Expression")]
    public class FixedBool : FixedValue<bool>
    {
        public FixedBool() : base() {}
        public FixedBool(bool value) : base(value) {}
    }

    [NodeMenuItem("Expression")]
    public class FixedVector3 : FixedValue<Vector3>
    {
        public FixedVector3() : base() {}
        public FixedVector3(Vector3 value) : base(value) {}
    }

    [NodeMenuItem("Expression")]
    public class Add : Expression<float>
    {
        public Expression<float> A = new FixedFloat();
        public Expression<float> B = new FixedFloat();

        public override float Evaluate()
        {
            return A.Evaluate() + B.Evaluate();
        }
    }

    [NodeMenuItem("Expression")]
    public class Sub : Expression<float>
    {
        public Expression<float> A = new FixedFloat();
        public Expression<float> B = new FixedFloat();

        public override float Evaluate()
        {
            return A.Evaluate() - B.Evaluate();
        }
    }

    [Name("Vector3")]
    [NodeMenuItem("Expression")]
    public class Vector3Expression : Expression<Vector3>
    {
        public Expression<float> X = new FixedFloat();
        public Expression<float> Y = new FixedFloat();
        public Expression<float> Z = new FixedFloat();

        public override Vector3 Evaluate()
        {
            return new Vector3(X.Evaluate(), Y.Evaluate(), Z.Evaluate());
        }
    }

    [NodeMenuItem("Expression")]
    public class FixedInt : FixedValue<int>
    {
        public FixedInt() : base() { }
        public FixedInt(int value) : base(value) { }
    }

    [NodeMenuItem("Expression")]
    public class FixedFloat : FixedValue<float>
    {
        public FixedFloat() : base() {}
        public FixedFloat(float value) : base(value) {}
    }

    [NodeMenuItem("Expression")]
    public class FixedGameObject : FixedValue<GameObject>
    {
        public FixedGameObject() : base(null)
        {
        }
    }

    [NodeMenuItem("Expression")]
    public class FixedObject : FixedValue<ObjectNode>
    {
        public FixedObject() : base() { }
        public FixedObject(ObjectNode value) : base(value) { }
    }

    public enum DamageType
    {
        Physical,
        Magical,
    }

    [NodeMenuItem("Expression")]
    public class FixedDamageType : FixedValue<DamageType>
    {
        public FixedDamageType() : base() { }
        public FixedDamageType(DamageType value) : base(value) { }
    }

    public enum CastTargetType
    {
        Target,
        Ground,
    }

    [NodeMenuItem("Expression")]
    public class FixedCastTargetType : FixedValue<CastTargetType>
    {
        public FixedCastTargetType() : base() { }
        public FixedCastTargetType(CastTargetType value) : base(value) { }
    }

    public enum Alliance
    {
        Friendly = 1,
        Enemy = 2,
        All = 4,
    }
}
