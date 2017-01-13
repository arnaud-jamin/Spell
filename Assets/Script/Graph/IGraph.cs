﻿using FullSerializer;
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
        INode Root { get; set; }
        Type RootType { get; }
        List<INode> Nodes { get; }
        Vector2 ViewOffset { get; set; }
        float ViewZoom { get; set; }

        BaseTypeInfo GetBaseTypeInfo(Type type);
        INode CreateNode(Type type);
        INode CreateFixedValue(Type valueType);

        void Clear();
        void Save();
        void Load();
    }
}