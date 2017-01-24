using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Spell.Graph
{
    public interface INode
    {
        Vector2 GraphPosition { get; set; }
        string VariableName { get; set; }
        Type PrimitiveType { get; }
        object PrimitiveValue { get; set; }
        List<IParameterInfo> GetParameters();
    }
}
