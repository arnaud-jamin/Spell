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
        public Color GetColor(Type type)
        {
            return GetTypeColor(type);
        }

        // ----------------------------------------------------------------------------------------
        public Color GetNodeColor(INode node)
        {
            return GetTypeColor(node.GetType());
        }

        // ----------------------------------------------------------------------------------------
        public Color GetParameterColor(int nodeIndex, int parameterIndex)
        {
            var parameter = GetParameter(nodeIndex, parameterIndex);
            if (parameter == null)
                return new Color(0.2f, 0.2f, 0.2f);

            return GetTypeColor(parameter.GetType());
        }

        // ----------------------------------------------------------------------------------------
        public Vector2 GetParameterSize(int nodeIndex, int parameterIndex)
        {
            var parameter = GetParameter(nodeIndex, parameterIndex);

            var inValue = parameter as InValue;
            if (inValue != null)
            {
                return GetFieldSize(inValue.ValueType);
            }

            var outValue = parameter as InValue;
            if (outValue != null)
            {
                return GetFieldSize(outValue.ValueType);
            }

            return new Vector2(0, 16);
        }

        // ----------------------------------------------------------------------------------------
        public static Color GetTypeColor(Type type)
        {
            if (typeof(InValue<float>).IsAssignableFrom(type))      return new Color(0.2f, 0.2f, 0.5f);
            if (typeof(OutValue<float>).IsAssignableFrom(type))     return new Color(0.2f, 0.2f, 0.5f);

            if (typeof(InValue<int>).IsAssignableFrom(type))        return new Color(0.8f, 0.8f, 0.0f);
            if (typeof(OutValue<int>).IsAssignableFrom(type))       return new Color(0.8f, 0.8f, 0.0f);

            if (typeof(InValue<Shape>).IsAssignableFrom(type))      return new Color(0.2f, 0.5f, 0.2f);
            if (typeof(OutValue<Shape>).IsAssignableFrom(type))     return new Color(0.2f, 0.5f, 0.2f);

            if (typeof(BaseAction).IsAssignableFrom(type))          return new Color(0.5f, 0.2f, 0.2f);

            return new Color(0.2f, 0.2f, 0.2f);
        }

        // ----------------------------------------------------------------------------------------
        public static Vector2 GetFieldSize(Type type)
        {
            if (type == null)               return new Vector2(100, 16);
            if (type == typeof(float))      return new Vector2(75, 16);
            if (type == typeof(bool))       return new Vector2(75, 16);
            if (type == typeof(int))        return new Vector2(75, 16);
            if (type == typeof(Vector3))    return new Vector2(150, 16);
            if (type == typeof(GameObject)) return new Vector2(100, 16);
            if (type.IsEnum)                return new Vector2(75, 16);
            return new Vector2(100, 16);
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
        public List<NodeTypeInfo> GetAssignableNodes(IParameterInfo parameter)
        {
            return GetAssignableNodes((parameter as ParameterInfo).FieldInfo.FieldType);
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
                                    .Select(t => NodeTypeInfo.GetNodeInfo(t))
                                    .Where(t => t.excludeFromMenu == false)
                                    .OrderBy(t => t.menuPath + "/" + t.name).ToList();
            return nodeInfos;
        }

        // ----------------------------------------------------------------------------------------
        public bool CanConnectParameters(IParameterInfo p1, IParameterInfo p2)
        {
            if (p1 != null)
                return true;

            return false;
        }

        // ----------------------------------------------------------------------------------------
        public void ConnectParameters(IParameterInfo p1, IParameterInfo p2)
        {
        }

        // ----------------------------------------------------------------------------------------
        private BaseParameter GetParameter(int nodeIndex, int parameterIndex)
        {
            if (nodeIndex < 0 && nodeIndex >= m_nodes.Count)
                return null;

            var node = m_nodes[nodeIndex] as Node;
            if (node == null)
                return null;

            if (parameterIndex < 0 && parameterIndex >= node.Parameters.Count)
                return null;

            var parameter = node.Parameters[parameterIndex];
            return parameter;
        }

        // ----------------------------------------------------------------------------------------
        public void DrawField(int nodeIndex, int parameterIndex, Rect rect)
        {
            var parameter = GetParameter(nodeIndex, parameterIndex);
            if (parameter == null)
                return;

            parameter.DrawField(rect);
        }
    }
}