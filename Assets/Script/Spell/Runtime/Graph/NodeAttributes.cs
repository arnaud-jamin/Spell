using System;
using UnityEngine;

namespace Spell.Graph
{
    public class ExcludeFromMenuAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class NodeMenuItemAttribute : Attribute
    {
        public string MenuPath;

        public NodeMenuItemAttribute(String menuPath)
        {
            MenuPath = menuPath;
        }
    }

    public class ParameterSideAttribute : Attribute
    {
        public ParameterSide Side;

        public ParameterSideAttribute(ParameterSide type)
        {
            Side = type;
        }
    }

    public class NameAttribute : Attribute
    {
        public string Name;

        public NameAttribute(string name)
        {
            Name = name;
        }
    }
}