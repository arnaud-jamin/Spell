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
        Type ValueType { get; }
        object BoxedValue { get; set; }
        List<FieldInfo> GetFields();
    }
}
