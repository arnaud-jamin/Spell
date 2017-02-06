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

    public struct NewParameterIndex
    {
        public int nodeIndex;
        public int paramIndex;
    }



    public enum ParameterControlType
    {
        None,
        Bool,
        Int,
        Float,
        String,
        Enum,
        Mask,
        Curve,
        Vector2,
        Vector3,
        Color,
        UnityObject,
    }

    public struct NewNodeInfo
    {
        public int index;
        public string name;
        public Vector2 position;
        public Color color;
        public List<NewParameterInfo> parameters;
    }

    public struct NewParameterInfo
    {
        public int index;
        public int nodeIndex;
        public string name;
        public Color color;
        public bool isList;
        public bool hasPin;
        public ParameterControlType controlType;
        public Func<object> controlGetter;
        public Action<object> controlSetter;
        public List<NewConnection> connections;
    }

    public struct NewConnection
    {
        public int index;
        public int nodeIndex;
    }

    public interface IGraph 
    {
        INode Root { get; set; }
        Type RootType { get; }
        List<INode> Nodes { get; }
        Vector2 ViewOffset { get; set; }
        float ViewZoom { get; set; }

        BaseTypeInfo GetTypeInfo(Type type);
        INode CreateNode(Type type);
        INode CreateFixedValue(Type valueType);

        List<NewNodeInfo> GetNodes();

        void Clear();
        void Save();
        void Load();
        int GetInstanceID();
    }
}