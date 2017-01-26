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
        Color GetParameterColor(int nodeIndex, int parameterIndex);
        Vector2 GetParameterSize(int nodeIndex, int parameterIndex);
        void DrawField(int nodeIndex, int parameterIndex, Rect rect);

        INode CreateNode(Type type);
        INode CreateFixedValue(Type valueType);

        void Clear();
        void Save();
        void Load();
        int GetInstanceID();

        List<NodeTypeInfo> GetAllNodes();
        List<NodeTypeInfo> GetAssignableNodes(IParameterInfo parameter);

        bool CanConnectParameters(IParameterInfo p1, IParameterInfo p2);
        void ConnectParameters(IParameterInfo p1, IParameterInfo p2);
    }
}