using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spell.Graph
{
    public enum StatModifierMode
    {
        Base,
        Additive,
        Multiplicative,
    }

    public class StatModifier : BuffAction
    {
        public Expression<StatType> Stat = new StatTypeValue(StatType.Damage);
        public ExpressionValue<StatModifierMode> Mode = new ExpressionValue<StatModifierMode>(StatModifierMode.Additive);
        public Expression<float> Value = new FloatValue(100.0f);

        public override void OnStart()
        {
        }

        public override void OnStop()
        {
        }
    }
}
