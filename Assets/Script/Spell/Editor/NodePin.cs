using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Spell.Graph
{
    public struct NodePin
    {
        public INode node;
        public INode parentNode;
        public int indexInParent;
        public FieldInfo field;
        public BaseTypeInfo baseTypeInfo;
        public bool isAttached;
        public NodeInfo detachedNodeInfo;
        public Vector2 center;
        public Rect rect;

        public override bool Equals(object obj)
        {
            var b = (NodePin)obj;
            return this == b;
        }

        public override int GetHashCode()
        {
            return indexInParent;
        }

        public static bool operator ==(NodePin a, NodePin b)
        {
            return a.node == b.node;
        }

        public static bool operator !=(NodePin a, NodePin b)
        {
            return a.node != b.node;
        }
    }
}