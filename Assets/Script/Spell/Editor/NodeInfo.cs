using FullSerializer;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spell.Graph
{
    public class NodeInfo
    {
        public INode node;
        public Rect rect;
        public List<NodePin> pins = new List<NodePin>();
        public NodeTypeInfo derivedTypeInfo;
        public BaseTypeInfo baseTypeInfo;
        public Vector2 connectionPosition;
        public float fieldValueMaxWidth;
        public float fieldNameMaxWidth;
        public bool isMultiSided;

        public NodeInfo(INode node)
        {
            this.node = node;
            derivedTypeInfo = NodeTypeInfo.GetNodeInfo(node.GetType());
        }
    }
}
