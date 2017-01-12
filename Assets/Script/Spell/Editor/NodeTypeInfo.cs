using FullSerializer;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spell.Graph
{
    public class NodeTypeInfo
    {
        public static Dictionary<Type, NodeTypeInfo> s_cache = new Dictionary<Type, NodeTypeInfo>();

        public Type type;
        public string name;
        public string menuPath;
        public bool excludeFromMenu;

        public static NodeTypeInfo GetNodeInfo(Type type)
        {
            NodeTypeInfo info;
            if (s_cache.TryGetValue(type, out info))
            {
                return info;
            }

            info = new NodeTypeInfo();
            info.type = type;

            var nameAttribute = Attribute.GetCustomAttribute(type, typeof(NameAttribute), true) as NameAttribute;
            info.name = nameAttribute != null ? nameAttribute.Name : type.Name;

            var nodeMenuItemAttribute = Attribute.GetCustomAttribute(type, typeof(NodeMenuItemAttribute), true) as NodeMenuItemAttribute;
            info.menuPath = nodeMenuItemAttribute != null ? nodeMenuItemAttribute.MenuPath : string.Empty;

            var excludeFromMenuAttribute = Attribute.GetCustomAttribute(type, typeof(ExcludeFromMenuAttribute), true) as ExcludeFromMenuAttribute;
            info.excludeFromMenu = excludeFromMenuAttribute != null;

            s_cache[type] = info;

            return info;
        }
    }
}
