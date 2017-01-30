using System;
using UnityEngine;

namespace Spell.Graph
{
    [NodeMenuItem("Math")]
    public class Add : Node
    {
        public Add()
        {
            var a = AddInValue<float>("A", 0.0f);
            var b = AddInValue<float>("B", 0.0f);
            AddOutValue<float>("Result", () => { return a.Value + b.Value; });
        }
    }

    [NodeMenuItem("Math")]
    public class Sub : Node
    {
        public Sub()
        {
            var a = AddInValue<float>("A", 0.0f);
            var b = AddInValue<float>("B", 0.0f);
            AddOutValue<float>("Result", () => { return a.Value - b.Value; });
        }
    }

    [NodeMenuItem("Math")]
    public class Mult : Node
    {
        public Mult()
        {
            var a = AddInValue<float>("A", 0.0f);
            var b = AddInValue<float>("B", 0.0f);
            AddOutValue<float>("Result", () => { return a.Value * b.Value; });
        }
    }

    [NodeMenuItem("Math")]
    public class Div : Node
    {
        public Div()
        {
            var a = AddInValue<float>("A", 0.0f);
            var b = AddInValue<float>("B", 0.0f);
            AddOutValue<float>("Result", () => { return a.Value / b.Value; });
        }
    }

    [NodeMenuItem("Math")]
    public class GeatherThan : Node
    {
        public GeatherThan()
        {
            var a = AddInValue<float>("A", 0.0f);
            var b = AddInValue<float>("B", 0.0f);
            AddOutValue<bool>("Result", () => { return a.Value > b.Value; });
        }
    }
}
