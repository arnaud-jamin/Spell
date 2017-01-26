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

    public struct ParameterIndex
    {
        public ParameterIndex(int nodeIndex, int parameterIndex)
        {
            this.nodeIndex = nodeIndex;
            this.parameterIndex = parameterIndex;
        }

        public int nodeIndex;
        public int parameterIndex;
    }

    public interface IParameterInfo
    {
        string Name { get; }
        ParameterSide Side { get; }
        INode ConnectedNode { get; }
        bool IsList { get; }
        IList List { get; }
    }
}
