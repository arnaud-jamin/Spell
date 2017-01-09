using FullSerializer;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Spell.Graph
{
    public enum NodeSide
    {
        Right,
        Left,
    }

    public struct BaseTypeInfo
    {
        public Color color;
        public NodeSide side;
    }

    public interface IGraph 
    {
        List<INode> Nodes { get; }
        Vector2 ViewOffset { get; set; }
        float ViewZoom { get; set; }

        BaseTypeInfo GetBaseTypeInfo(Type type);
        INode CreateNode(Type type);
        INode CreateFixedValue(Type valueType);
        void DestroyNode(INode node);
        INode DisconnectField(INode node, FieldInfo field);

        void Clear();
        void Save();
        void Load();
    }
}