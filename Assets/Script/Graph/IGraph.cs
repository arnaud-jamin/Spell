using FullSerializer;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Spell.Graph
{
    public interface IGraph 
    {
        List<INode> Nodes { get; }
        Vector2 ViewOffset { get; set; }
        float ViewZoom { get; set; }

        INode CreateNode(Type type);
        INode CreateFixedValue(Type valueType);
        void DestroyNode(INode node);
        INode DisconnectField(INode node, FieldInfo field);

        void Clear();
        void Save();
        void Load();
    }
}