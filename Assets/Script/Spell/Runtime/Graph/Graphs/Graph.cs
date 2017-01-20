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
        public BaseTypeInfo GetTypeInfo(Type type)
        {
            if (typeof(Expression<float>).IsAssignableFrom(type))
            {
                return new BaseTypeInfo() { color = new Color(0.2f, 0.2f, 0.5f), side = NodeSide.Right };
            }
            else if (typeof(Expression<int>).IsAssignableFrom(type))
            {
                return new BaseTypeInfo() { color = new Color(0.8f, 0.8f, 0.0f), side = NodeSide.Right };
            }
            else if (typeof(Action).IsAssignableFrom(type))
            {
                return new BaseTypeInfo() { color = new Color(0.5f, 0.2f, 0.2f), side = NodeSide.Right };
            }
            else if (typeof(Shape).IsAssignableFrom(type))
            {
                return new BaseTypeInfo() { color = new Color(0.2f, 0.5f, 0.2f), side = NodeSide.Right };
            }

            return new BaseTypeInfo() { color = new Color(0.2f, 0.2f, 0.2f), side = NodeSide.Right };
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
        public List<NodeTypeInfo> GetNodesTypeInfos(Type baseType)
        {
            // TODO: collapse things like 'Action/' and 'Action/Select' to '' and 'Select'

            var assembly = Assembly.GetAssembly(baseType);
            var allTypes = assembly.GetTypes();
            var nodeInfos = allTypes.Where(t => baseType.IsAssignableFrom(t) && t.IsAbstract == false && t.IsInterface == false)
                                    .Select(t => NodeTypeInfo.GetNodeInfo(t))
                                    .Where(t => t.excludeFromMenu == false)
                                    .OrderBy(t => t.menuPath + "/" + t.name).ToList();
            return nodeInfos;
        }

        // ----------------------------------------------------------------------------------------
        public bool CanConnectParameters(IParameter p1, IParameter p2)
        {
            if (p1 != null)
                return true;

            return false;
        }

        // ----------------------------------------------------------------------------------------
        public void ConnectParameters(IParameter p1, IParameter p2)
        {
        }
    }
}