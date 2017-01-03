using FullSerializer;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Spell.Graph
{
    public abstract class Node
    {
        // ----------------------------------------------------------------------------------------
        public struct Pin
        {
            public int index;
            public Node node;
            public Vector2 center;
            public Rect rect;
            public Color color;
            public FieldInfo field;

            public override bool Equals(object obj)
            {
                var b = (Pin)obj;
                return this == b;
            }

            public override int GetHashCode()
            {
                return index;
            }

            public static bool operator ==(Pin a, Pin b)
            {
                return a.index == b.index && a.node == b.node;
            }

            public static bool operator !=(Pin a, Pin b)
            {
                return a.index != b.index || a.node != b.node;
            }
        }

        public Vector2 rectPosition;

        [fsIgnore]
        public Vector2 rectSize;

        [fsIgnore]
        public List<Pin> pins = new List<Pin>();
    }
}
