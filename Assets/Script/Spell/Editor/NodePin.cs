using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Spell.Graph
{
    public struct NodePin
    {
        public INode ownerNode;
        public int indexInOwner;
        public FieldInfo field;
        public Type type;
        public BaseTypeInfo typeInfo;
        public bool isList;
        public bool isAttached;
        public List<NodeInfo> connectedNodesInfos;
        public Vector2 fieldPosition;
        public Rect pinLocalRect;
        public Rect pinGlobalRect;

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
            return a.ownerNode == b.ownerNode && a.indexInOwner == b.indexInOwner;
        }

        public static bool operator !=(NodePin a, NodePin b)
        {
            return a.ownerNode != b.ownerNode || a.indexInOwner != b.indexInOwner;
        }
    }

    public struct NodeConnection
    {
        public NodePin pin;
        public int index;
        public INode connectedNode;

        public override bool Equals(object obj)
        {
            var b = (NodeConnection)obj;
            return this == b;
        }

        public override int GetHashCode()
        {
            return pin.GetHashCode();
        }

        public static bool operator ==(NodeConnection a, NodeConnection b)
        {
            return a.pin == b.pin && a.connectedNode == b.connectedNode;
        }

        public static bool operator !=(NodeConnection a, NodeConnection b)
        {
            return a.pin != b.pin || a.connectedNode != b.connectedNode;
        }
    }
}