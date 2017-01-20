using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Spell.Graph
{
    public interface IParameter
    {
        Type ValueType { get; }
        object BoxedValue { get; set; }
    }
}
