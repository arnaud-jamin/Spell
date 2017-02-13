using FullSerializer;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Spell.Graph
{
    public class NodeInfo
    {
        public int index;
        public Node node;
        public Rect rect;
        public List<NodePin> pins = new List<NodePin>();
        public NodeTypeInfo derivedTypeInfo;
        public BaseTypeInfo baseTypeInfo;
        public Vector2 connectionPosition;
        public float fieldValueMaxWidth;
        public float fieldNameMaxWidth;
        public bool isMultiSided;
    }

    public class NodePin
    {
        public int index;
        public NodeInfo nodeInfo;
        public List<NodeConnection> connections;
        public FieldInfo field;
        public Type type;
        public BaseTypeInfo typeInfo;
        public bool isList;
        public bool isAttached;
        public Vector2 fieldPosition;
        public Rect pinLocalRect;
        public Rect pinGlobalRect;
        public bool hasPin;
        public FieldInfo controlledField;
        public object controlledFieldOwner;
    }

    public class NodeConnection
    {
        public NodePin pin;
        public int index;
        public NodeInfo connectedNodeInfo;
    }
}
