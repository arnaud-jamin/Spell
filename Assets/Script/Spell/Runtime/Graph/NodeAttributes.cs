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

    [AttributeUsage(AttributeTargets.Class)]
    public class NodeColorAttribute : Attribute
    {
        public Color Color;

        public NodeColorAttribute(float r, float g, float b)
        {
            Color = new Color(r, g, b);
        }
    }
}