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

    [NodeMenuItem("Values")]
    public class Curve : ExpressionValue<AnimationCurve>
    {
        public Curve()
        {
            Value = new AnimationCurve();
        }
    }

    [NodeMenuItem("Values")]
    public class CurveValue : Expression<float>
    {
        public Expression<float> Time = new FloatValue(0);
        public Expression<AnimationCurve> Curve = new Curve();

        public override float Evaluate()
        {
            return Curve.Evaluate().Evaluate(Time.Evaluate());
        }
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

    public enum RotationType
    {
        CasterRotation,
        TargetRotation,
    }

    public enum ObjectProperty
    {
        Level,
        Rotation,
        Height,
    }


    [NodeMenuItem("Object")]
    public class ObjectFloatProperty : Expression<float>
    {
        public Expression<TargetType> Object = new ExpressionValue<TargetType>();
        public Expression<ObjectProperty> Property = new ExpressionValue<ObjectProperty>();

        public override float Evaluate()
        {
            return 0;
        }
    }

    [NodeMenuItem("Values")]
    public class Position : Expression<Vector3>
    {
        public PositionType Type = PositionType.CasterPosition;

        public override object BoxedValue { get { return Type; } set { Type = (PositionType)value; } }
        public override Type BoxedValueType { get { return typeof(PositionType); } }

        public override Vector3 Evaluate()
        {
            return Vector3.zero;
        }
    }


    [NodeMenuItem("Values")]
    public class Rotation : Expression<float>
    {
        public RotationType Type = RotationType.CasterRotation;

        public override object BoxedValue { get { return Type; } set { Type = (RotationType)value; } }
        public override Type BoxedValueType { get { return typeof(RotationType); } }

        public override float Evaluate()
        {
            return 0;
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

    public enum IteratorType
    {
        Iterator1,
        Iterator2,
        Iterator3,
    }

    public enum TargetType
    {
        Caster,
        CastTarget,
        Iterator1,
        Iterator2,
        Iterator3,
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
