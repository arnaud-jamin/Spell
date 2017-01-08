using FullSerializer;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spell.Graph
{
    public class NodeTypeInfo
    {
        public static Dictionary<Type, NodeTypeInfo> s_cache = new Dictionary<Type, NodeTypeInfo>();

        public Color color;
        public string name;
        public string menuPath;

        public static NodeTypeInfo Get(Type type)
        {
            NodeTypeInfo info;
            if (s_cache.TryGetValue(type, out info))
            {
                return info;
            }

            info = new NodeTypeInfo();

            var nodeColorAttribute = Attribute.GetCustomAttribute(type, typeof(NodeColorAttribute)) as NodeColorAttribute;
            info.color = nodeColorAttribute != null ? nodeColorAttribute.Color : new Color(0.2f, 0.2f, 0.2f);

            var nameAttribute = Attribute.GetCustomAttribute(type, typeof(NameAttribute)) as NameAttribute;
            info.name = nameAttribute != null ? nameAttribute.Name : type.Name;

            var nodeMenuItemAttribute = Attribute.GetCustomAttribute(type, typeof(NodeMenuItemAttribute)) as NodeMenuItemAttribute;
            info.menuPath = nodeMenuItemAttribute != null ? nodeMenuItemAttribute.MenuPath + "/" + info.name : info.name;

            s_cache[type] = info;

            return info;
        }
    }
}
