using System;
using UnityEngine;

namespace Spell.Graph
{
    public interface INode
    {
        Vector2 GraphPosition { get; set; }
        bool IsFixedValue { get; }
        bool IsAttached { get; set; }
        Type ValueType { get; }
        object BoxedValue { get; set; }
    }
}
