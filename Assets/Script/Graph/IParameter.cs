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

    public interface IParameterInfo
    {
        string Name { get; }
        Color Color { get; }
        ParameterSide Side { get; }
        INode ConnectedNode { get; }
        bool IsList { get; }
        IList List { get; }
        Vector2 Size { get; }

        bool CanConnectToParameter(IParameterInfo parameter);
        bool ConnectToParameter(IParameterInfo parameter);
        bool CanConnectToNode(INode node);
        bool ConnectToNode(INode node);
        void Disconnect(IGraph graph, INode node);
    }
}
