
using System;
using System.Reflection;

namespace Spell.Graph
{
    public class Parameter : IParameter
    {
        public FieldInfo fieldInfo;

        public virtual Type ValueType { get { return null; } }
        public virtual object BoxedValue { get { return null; } set { } }

    }
}
