using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Spell.Graph
{
    public struct NodePin
    {
        public int index;
        public INode node;
        public Vector2 center;
        public Rect rect;
        public Color color;
        public FieldInfo field;
        internal INode connectedNode;

        public override bool Equals(object obj)
        {
            var b = (NodePin)obj;
            return this == b;
        }

        public override int GetHashCode()
        {
            return index;
        }

        public static bool operator ==(NodePin a, NodePin b)
        {
            return a.index == b.index && a.node == b.node;
        }

        public static bool operator !=(NodePin a, NodePin b)
        {
            return a.index != b.index || a.node != b.node;
        }
    }
}