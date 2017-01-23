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
        public INode node;
        public Rect rect;
        public List<NodePin> parameters = new List<NodePin>();
        public NodeTypeInfo derivedTypeInfo;
        public BaseTypeInfo baseTypeInfo;
        public Vector2 connectionPosition;
        public float fieldValueMaxWidth;
        public float fieldNameMaxWidth;
        public bool isMultiSided;
        public Color color;
    }

    public class NodePin
    {
        public int index;
        public NodeInfo nodeInfo;
        public List<NodeConnection> connections;
        public bool isAttached;
        public Vector2 fieldPosition;
        public Rect pinLocalRect;
        public Rect pinGlobalRect;
        public IParameter parameter;
    }

    public class NodeConnection
    {
        public NodePin parameter;
        public int index;
        public int connectedNode = -1;
        public int connectedParameter = -1;
    }
}
