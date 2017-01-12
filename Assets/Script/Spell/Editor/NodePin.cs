using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Spell.Graph
{
    public struct NodePin
    {
        public INode valueNode;
        public INode ownerNode;
        public int indexInOwner;
        public FieldInfo field;
        public BaseTypeInfo baseTypeInfo;
        public bool isAttached;
        public List<NodeInfo> connectedNodes;
        public Vector2 center;
        public Rect rect;

        public override bool Equals(object obj)
        {
            var b = (NodePin)obj;
            return this == b;
        }

        public override int GetHashCode()
        {
            return indexInOwner;
        }

        public static bool operator ==(NodePin a, NodePin b)
        {
            return a.valueNode == b.valueNode;
        }

        public static bool operator !=(NodePin a, NodePin b)
        {
            return a.valueNode != b.valueNode;
        }
    }
}