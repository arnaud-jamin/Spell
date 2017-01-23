using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Spell.Graph
{
    public enum ParameterSide
    {
        Right,
        Left,
    }

    public interface IParameter
    {
        string Name { get; }
        Color Color { get; }
        ParameterSide Side { get; }
        INode ConnectedNode { get; }
        bool IsList { get; }
        IList List { get; }
        Vector2 Size { get; }

        bool CanConnectToParameter(IParameter parameter);
        bool ConnectToParameter(IParameter parameter);
        bool CanConnectToNode(INode node);
        bool ConnectToNode(INode node);
        void Disconnect(IGraph graph, INode node);
    }
}
