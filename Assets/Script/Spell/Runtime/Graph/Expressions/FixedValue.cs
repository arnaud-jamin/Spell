using System;
using UnityEngine;

namespace Spell.Graph
{
    public abstract class FixedValue<T> : Expression<T>
    {
        public T Value;

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
    }

    [NodeMenuItem("Expression/FixedBool")]
    public class FixedBool : FixedValue<bool>
    {
        public FixedBool() : base() {}
        public FixedBool(bool value) : base(value) {}
    }

    [NodeMenuItem("Expression/FixedVector3")]
    public class FixedVector3 : FixedValue<Vector3>
    {
        public FixedVector3() : base() {}
        public FixedVector3(Vector3 value) : base(value) {}
    }

    [NodeMenuItem("Expression/FixedInt")]
    public class FixedInt : FixedValue<int>
    {
        public FixedInt() : base() { }
        public FixedInt(int value) : base(value) { }
    }

    [NodeMenuItem("Expression/FixedFloat")]
    public class FixedFloat : FixedValue<float>
    {
        public FixedFloat() : base() {}
        public FixedFloat(float value) : base(value) {}
    }

    [NodeMenuItem("Expression/FixedGameObject")]
    public class FixedGameObject : FixedValue<GameObject>
    {
        public FixedGameObject() : base(null)
        {
        }
    }

    [NodeMenuItem("Expression/Summonable")]
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

    [NodeMenuItem("Expression/FixedDamageType")]
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

    [NodeMenuItem("Expression/FixedCastTargetType")]
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
