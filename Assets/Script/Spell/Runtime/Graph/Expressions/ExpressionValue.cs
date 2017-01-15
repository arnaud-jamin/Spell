using System;
using UnityEngine;

namespace Spell.Graph
{
    [ExcludeFromMenu]
    public class ExpressionValue<T> : Expression<T>
    {
        public T Value;

        public ExpressionValue()
        {
            Value = default(T);
        }

        public ExpressionValue(T value)
        {
            Value = value;
        }

        public override T Evaluate()
        {
            return Value;
        }

        public override bool IsFixedValue { get { return true; } }

        public override object BoxedValue
        {
            get { return Value; }
            set { Value = (T)value; }
        }
    }

    [NodeMenuItem("Values")]
    [Name("Bool")]
    public class BoolValue : ExpressionValue<bool>
    {
        public BoolValue() : base() {}
        public BoolValue(bool value) : base(value) {}
    }

    [NodeMenuItem("Values")]
    [Name("Int")]
    public class IntValue : ExpressionValue<int>
    {
        public IntValue() : base() { }
        public IntValue(int value) : base(value) { }
    }

    [NodeMenuItem("Values")]
    [Name("Float")]
    public class FloatValue : ExpressionValue<float>
    {
        public FloatValue() : base() { }
        public FloatValue(float value) : base(value) { }
    }

    [NodeMenuItem("Values")]
    [Name("Text")]
    public class TextValue : ExpressionValue<string>
    {
        public TextValue() : base() { }
        public TextValue(string value) : base(value) { }
    }

    [NodeMenuItem("Values")]
    [Name("Sprite")]
    public class SpriteValue : ExpressionValue<Sprite>
    {
        public SpriteValue() : base() { }
        public SpriteValue(Sprite value) : base(value) { }
    }

    [NodeMenuItem("Values")]
    [Name("Vector3")]
    public class Vector3Value : ExpressionValue<Vector3>
    {
        public Vector3Value() : base() { }
        public Vector3Value(Vector3 value) : base(value) { }
    }

    [NodeMenuItem("Values")]
    [Name("Color")]
    public class ColorValue : ExpressionValue<Color>
    {
        public ColorValue() : base() { }
        public ColorValue(Color value) : base(value) { }
    }

    [NodeMenuItem("Values")]
    [Name("GameObject")]
    public class GameObjectValue : ExpressionValue<GameObject>
    {
        public GameObjectValue() : base(null)
        {
        }
    }

    [NodeMenuItem("Values")]
    [Name("Object")]
    public class ObjectValue : ExpressionValue<ObjectNode>
    {
        public ObjectValue() : base() { }
        public ObjectValue(ObjectNode value) : base(value) { }
    }

    public enum DamageType
    {
        Physical,
        Magical,
    }

    [NodeMenuItem("Values")]
    [Name("DamageType")]
    public class DamageTypeValue : ExpressionValue<DamageType>
    {
        public DamageTypeValue() : base() { }
        public DamageTypeValue(DamageType value) : base(value) { }
    }

    public enum CastTargetType
    {
        Target,
        Ground,
    }

    [NodeMenuItem("Values")]
    [Name("CastTargetType")]
    public class CastTargetTypeValue : ExpressionValue<CastTargetType>
    {
        public CastTargetTypeValue() : base() { }
        public CastTargetTypeValue(CastTargetType value) : base(value) { }
    }

    public enum Alliance
    {
        Friendly = 1,
        Enemy = 2,
        All = 4,
    }
}
