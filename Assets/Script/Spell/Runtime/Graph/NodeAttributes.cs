using System;
using UnityEngine;

namespace Spell
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeMenuItemAttribute : Attribute
    {
        public string MenuPath;

        public NodeMenuItemAttribute(String menuPath)
        {
            MenuPath = menuPath;
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