using FullSerializer;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spell.Graph
{
    public class NodeInfo
    {
        public INode node;
        public Rect globalRect;
        public Rect localBackgroundRect;
        internal Rect globalBackgroundRect;
        public List<NodePin> pins = new List<NodePin>();
        public NodeTypeInfo typeInfo;

        public NodeInfo(INode node)
        {
            this.node = node;
            typeInfo = NodeTypeInfo.Get(node.GetType());
        }
    }
}
