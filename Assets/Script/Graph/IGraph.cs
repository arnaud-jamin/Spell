using FullSerializer;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Spell.Graph
{
    public enum NodeSide
    {
        Right,
        Left,
    }

    public struct BaseTypeInfo
    {
        public Color color;
        public NodeSide side;
    }
}