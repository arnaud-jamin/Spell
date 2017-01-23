using FullSerializer;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Spell.Graph
{
    public struct BaseTypeInfo
    {
        public Color color;
    }

    public interface IGraph 
    {
        INode Root { get; set; }
        Type RootType { get; }
        List<INode> Nodes { get; }
        Vector2 ViewOffset { get; set; }
        float ViewZoom { get; set; }

        Color GetNodeColor(INode node);
        Color GetParameterColor(IParameter parameter);
        Vector2 GetPrimitiveNodeSize(INode node);
        void DrawField(INode node, Rect rect);

        INode CreateNode(Type type);
        INode CreateFixedValue(Type valueType);

        void Clear();
        void Save();
        void Load();
        int GetInstanceID();

        List<NodeTypeInfo> GetAllNodes();
        List<NodeTypeInfo> GetAssignableNodes(IParameter parameter);

        bool CanConnectParameters(IParameter p1, IParameter p2);
        void ConnectParameters(IParameter p1, IParameter p2);
    }
}