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

        Color GetNodeColor(int nodeIndex);
        Color GetParameterColor(ParameterIndex p);
        Vector2 GetParameterSize(ParameterIndex p);
        void DrawField(ParameterIndex p, Rect rect);
        
        INode CreateNode(Type type);
        INode CreateFixedValue(Type valueType);

        void Clear();
        void Save();
        void Load();
        int GetInstanceID();

        List<NodeTypeInfo> GetAllNodes();
        List<NodeTypeInfo> GetAssignableNodes(IParameterInfo parameter);

        bool CanConnectParameters(ParameterIndex p1, ParameterIndex p2);
        bool ConnectParameters(ParameterIndex p1, ParameterIndex p2);
        void Disconnect(ParameterIndex p1, ParameterIndex p2);
    }
}