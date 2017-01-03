using FullSerializer;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spell.Graph
{
    public class NodeInfo
    {
        public static Dictionary<Type, NodeInfo> s_cache = new Dictionary<Type, NodeInfo>();

        public Color color;
        public string menuPath;

        public static NodeInfo GetNodeInfo(Type type)
        {
            NodeInfo info;
            if (s_cache.TryGetValue(type, out info))
            {
                return info;
            }

            info = new NodeInfo();

            var nodeColorAttribute = Attribute.GetCustomAttribute(type, typeof(NodeColorAttribute)) as NodeColorAttribute;
            info.color = nodeColorAttribute != null ? nodeColorAttribute.Color : new Color(0.2f, 0.2f, 0.2f);

            var nodeMenuItemAttribute = Attribute.GetCustomAttribute(type, typeof(NodeMenuItemAttribute)) as NodeMenuItemAttribute;
            info.menuPath = nodeMenuItemAttribute != null ? nodeMenuItemAttribute.MenuPath : type.Name;

            s_cache[type] = info;

            return info;
        }
    }
}
