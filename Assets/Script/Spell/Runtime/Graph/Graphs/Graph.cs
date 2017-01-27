using FullSerializer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Spell.Graph
{
    public class Graph : ScriptableObject, IGraph
    {
        // ----------------------------------------------------------------------------------------
        public static readonly Color DefaultColor = new Color(0.2f, 0.2f, 0.2f);
        public static readonly Color BoolColor = new Color(0.2f, 0.5f, 0.2f);
        public static readonly Color FloatColor = new Color(0.2f, 0.2f, 0.5f);
        public static readonly Color IntColor = new Color(0.8f, 0.8f, 0.0f);
        public static readonly Color ShapeColor = new Color(0.2f, 0.5f, 0.2f);
        public static readonly Color ActionColor = new Color(0.5f, 0.2f, 0.2f);
        public static readonly Color AbilityColor = new Color(0.5f, 0.5f, 0.8f);

        // ----------------------------------------------------------------------------------------
        private static Dictionary<Type, NodeTypeInfo> s_nodeInfoCache = new Dictionary<Type, NodeTypeInfo>();

        // ----------------------------------------------------------------------------------------
        [fsIgnore]
        private fsSerializer m_serializer = new fsSerializer();

        [SerializeField]
        [fsIgnore]
        protected string m_json;

        [SerializeField]
        private Type m_rootType = null;

        [SerializeField]
        protected INode m_root = null;

        [SerializeField]
        protected List<INode> m_nodes = new List<INode>();

        [SerializeField]
        private Vector2 m_viewOffset = new Vector2(0, 0);

        [SerializeField]
        private float m_viewZoom = 1.0f;
        
        // ----------------------------------------------------------------------------------------
        public INode Root { get { return m_root; } set { m_root = value; } }
        public List<INode> Nodes { get { return m_nodes; } }
        public Vector2 ViewOffset { get { return m_viewOffset; } set { m_viewOffset = value; } }
        public float ViewZoom { get { return m_viewZoom; } set { m_viewZoom = value; } }
        public virtual Type RootType { get { return m_rootType; } set { m_rootType = value; } }

        // ----------------------------------------------------------------------------------------
        protected void OnEnable()
        {
            Load();
        }

        // ----------------------------------------------------------------------------------------
        public Color GetNodeColor(int nodeIndex)
        {
            var node = GetNode(nodeIndex);
            return node.Color;
        }

        // ----------------------------------------------------------------------------------------
        public Color GetParameterColor(ParameterIndex p)
        {
            var parameter = GetParameter(p);
            if (parameter == null)
                return DefaultColor;
            return parameter.GetColor();
        }

        // ----------------------------------------------------------------------------------------
        public Vector2 GetParameterSize(ParameterIndex p)
        {
            var parameter = GetParameter(p);
            if (parameter == null)
                return new Vector2(0, 16);
            return parameter.GetSize();
        }

        // ----------------------------------------------------------------------------------------
        public INode CreateNode(Type type)
        {
            var node = Activator.CreateInstance(type) as INode;
            node.GraphPosition = m_viewOffset + new Vector2(200, 200);
            m_nodes.Add(node);
            return node;
        }

        // ----------------------------------------------------------------------------------------
        public INode CreateFixedValue(Type valueType)
        {
            var fixedExpressionType = typeof(ExpressionValue<>);
            Type[] typeArgs = { valueType };
            var genericType = fixedExpressionType.MakeGenericType(typeArgs);
            var node = Activator.CreateInstance(genericType) as INode;
            return node;
        }

        // ----------------------------------------------------------------------------------------
        public void Clear()
        {
            m_nodes.Clear();
            m_viewOffset = Vector2.zero;
            m_viewZoom = 1.0f;
            m_root = CreateNode(RootType);
        }

        // ----------------------------------------------------------------------------------------
        public void Save()
        {
            fsData data;
            m_serializer.TrySerialize(GetType(), this, out data).AssertSuccessWithoutWarnings();
            //m_json = fsJsonPrinter.CompressedJson(data);
            m_json = fsJsonPrinter.PrettyJson(data);
            EditorUtility.SetDirty(this);
        }

        // ----------------------------------------------------------------------------------------
        public void Load()
        {
            var data = fsJsonParser.Parse(m_json);
            var obj = (object)this;
            m_serializer.TryDeserialize(data, GetType(), ref obj).AssertSuccessWithoutWarnings();
        }

        // ----------------------------------------------------------------------------------------
        public List<NodeTypeInfo> GetAssignableNodes(ParameterIndex parameterIndex)
        {
            var parameter = GetParameter(parameterIndex);

            if (parameter is ValueParameter)
            {
                return GetAssignableNodes(parameter as ValueParameter);
            }
            else if (parameter is ActionParameter)
            {
                return GetAssignableNodes(typeof(ActionNode));
            }

            return new List<NodeTypeInfo>();
        }

        // ----------------------------------------------------------------------------------------
        public List<NodeTypeInfo> GetAllNodes()
        {
            return GetAssignableNodes(typeof(INode));
        }

        // ----------------------------------------------------------------------------------------
        private List<NodeTypeInfo> GetAssignableNodes(Type type)
        {
            // TODO: collapse things like 'Action/' and 'Action/Select' to '' and 'Select'

            var assembly = Assembly.GetAssembly(type);
            var allTypes = assembly.GetTypes();
            var nodeInfos = allTypes.Where(t => type.IsAssignableFrom(t) && t.IsAbstract == false && t.IsInterface == false)
                                    .Select(t => GetNodeInfo(t))
                                    .Where(t => t.excludeFromMenu == false)
                                    .OrderBy(t => t.menuPath + "/" + t.name).ToList();
            return nodeInfos;
        }

        // ----------------------------------------------------------------------------------------
        private List<NodeTypeInfo> GetAssignableNodes(ValueParameter inValue)
        {
            // TODO: collapse things like 'Action/' and 'Action/Select' to '' and 'Select'

            var assembly = Assembly.GetAssembly(inValue.GetType());
            var allTypes = assembly.GetTypes();
            var nodeInfos = allTypes.Where(t => t.IsAbstract == false 
                                                && t.IsInterface == false
                                                && typeof(INode).IsAssignableFrom(t)
                                                && t.GetFields().FirstOrDefault(f => f.FieldType.IsAssignableFrom(inValue.ValueType)) != null)
                                    .Select(t => GetNodeInfo(t))
                                    .Where(t => t.excludeFromMenu == false)
                                    .OrderBy(t => t.menuPath + "/" + t.name).ToList();
            return nodeInfos;
        }

        // ----------------------------------------------------------------------------------------
        public static NodeTypeInfo GetNodeInfo(Type type)
        {
            NodeTypeInfo info;
            if (s_nodeInfoCache.TryGetValue(type, out info))
                return info;

            info = new NodeTypeInfo();
            info.type = type;

            var nameAttribute = Attribute.GetCustomAttribute(type, typeof(NameAttribute), true) as NameAttribute;
            info.name = nameAttribute != null ? nameAttribute.Name : type.Name;

            var nodeMenuItemAttribute = Attribute.GetCustomAttribute(type, typeof(NodeMenuItemAttribute), true) as NodeMenuItemAttribute;
            info.menuPath = nodeMenuItemAttribute != null ? nodeMenuItemAttribute.MenuPath : string.Empty;

            var excludeFromMenuAttribute = Attribute.GetCustomAttribute(type, typeof(ExcludeFromMenuAttribute), false) as ExcludeFromMenuAttribute;
            info.excludeFromMenu = excludeFromMenuAttribute != null;

            s_nodeInfoCache[type] = info;

            return info;
        }

        // ----------------------------------------------------------------------------------------
        private Node GetNode(int nodeIndex)
        {
            if (nodeIndex < 0 && nodeIndex >= m_nodes.Count)
                return null;

            return m_nodes[nodeIndex] as Node;
        }

        // ----------------------------------------------------------------------------------------
        private BaseParameter GetParameter(ParameterIndex p)
        {
            var node = GetNode(p.nodeIndex);
            if (node == null)
                return null;

            if (p.parameterIndex < 0 && p.parameterIndex >= node.Parameters.Count)
                return null;

            var parameter = node.Parameters[p.parameterIndex];
            return parameter;
        }

        // ----------------------------------------------------------------------------------------
        public void DrawField(ParameterIndex p, Rect rect)
        {
            var parameter = GetParameter(p);
            if (parameter == null)
                return;

            parameter.DrawField(rect);
        }

        // ----------------------------------------------------------------------------------------
        public bool CanConnectParameters(ParameterIndex srcParam, ParameterIndex dstParam)
        {
            var src = GetParameter(srcParam);
            var dst = GetParameter(dstParam);

            if (src is ValueParameter && dst is ValueParameter)
            {
                if (src is InValue && dst is OutValue)
                {
                    return ((InValue)src).ValueType.IsAssignableFrom(((OutValue)dst).ValueType);
                }
                else if (src is OutValue && src is InValue)
                {
                    return ((InValue)dst).ValueType.IsAssignableFrom(((OutValue)src).ValueType);
                }
            }
            else if (src is ActionParameter && dst is ActionParameter)
            {
                if ((src is InAction && dst is OutAction) || (src is OutValue && src is InValue))
                {
                    return true;
                }
            }

            return false;
        }

        // ----------------------------------------------------------------------------------------
        public bool ConnectParameters(ParameterIndex src, ParameterIndex dst)
        {
            return true;
        }

        // ----------------------------------------------------------------------------------------
        //public bool CanConnectToNode(INode node)
        //{
        //    var currentValue = ConnectedNode;
        //    if (currentValue == node)
        //        return false;

        //    if (IsList && typeof(INode).IsAssignableFrom(m_primitiveType))
        //        return true;

        //    if (m_fieldInfo.FieldType.IsAssignableFrom(node.GetType()))
        //        return true;

        //    return false;
        //}

        // ----------------------------------------------------------------------------------------
        //public bool ConnectToNode(INode nodeToConnect)
        //{
        //    var currentValue = ConnectedNode;
        //    if (currentValue == nodeToConnect)
        //        return false;

        //    // TODO: manage cycles:
        //    //parameter.connections.Add(new NodeConnection() { connectedNode = value, index = 0, pin = parameter });
        //    //var isCreatingCycle = IsCreatingCycle(value, value);
        //    //parameter.connections.RemoveAt(parameter.connections.Count - 1);
        //    //if (isCreatingCycle)
        //    //    return;

        //    if (IsList)
        //    {
        //        if (typeof(INode).IsAssignableFrom(m_primitiveType))
        //        {
        //            var list = List;
        //            if (list.Contains(nodeToConnect) == false)
        //            {
        //                list.Add(nodeToConnect);
        //                return true;
        //            }
        //        }
        //    }
        //    else if (m_fieldInfo.FieldType.IsAssignableFrom(nodeToConnect.GetType()))
        //    {
        //        m_fieldInfo.SetValue(m_node, nodeToConnect);

        //        // When we create a connection we try to retain the value
        //        // of the old node to the new one.
        //        if (currentValue != null && currentValue.PrimitiveValue != null)
        //        {
        //            nodeToConnect.PrimitiveValue = currentValue.PrimitiveValue;
        //        }
        //    }
        //    return false;
        //}

        // ----------------------------------------------------------------------------------------
        public void Disconnect(ParameterIndex src, ParameterIndex dst)
        {
            //if (IsList)
            //{
            //    List.Remove(node);
            //}
            //else
            //{
            //    if (m_primitiveType != null)
            //    {
            //        var primitiveValue = ConnectedNode.PrimitiveValue;
            //        m_fieldInfo.SetValue(m_node, graph.CreateFixedValue(m_primitiveType));

            //        // When we delete a connection we try to retain the value
            //        // of the old node to the new one.
            //        if (primitiveValue != null)
            //        {
            //            ConnectedNode.PrimitiveValue = primitiveValue;
            //        }
            //    }
            //    else
            //    {
            //        m_fieldInfo.SetValue(m_node, null);
            //    }
            //}
        }
    }
}