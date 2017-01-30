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

    [Flags]
    public enum AllianceType
    {
        Self,
        Allies,
        Enemies,
        Neutral,
    }

    [NodeMenuItem("Values")]
    public class Alliance : ExpressionValue<AllianceType>
    {
        public Alliance() : base() { }
        public Alliance(AllianceType value) : base(value) { }
    }

    public enum PositionType
    {
        CasterPosition,
        TargetPosition,
    }

    [NodeMenuItem("Values")]
    public class Position : Expression<Vector3>
    {
        public ExpressionValue<PositionType> Type = new ExpressionValue<PositionType>();

        public override Vector3 Evaluate()
        {
            return Vector3.zero;
        }
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

    public enum StatType
    {
        MaxHealth,
        Damage,
        MoveSpeed,
        AttackSpeed,
        Armor,
    }

    [NodeMenuItem("Values")]
    [Name("StatType")]
    public class StatTypeValue : ExpressionValue<StatType>
    {
        public StatTypeValue() : base() { }
        public StatTypeValue(StatType value) : base(value) { }
    }

}
