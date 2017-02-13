using FullSerializer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Spell.Graph
{
    public class Graph : ScriptableObject
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
        protected Node m_root = null;

        [SerializeField]
        protected List<Node> m_nodes = new List<Node>();

        [SerializeField]
        private Vector2 m_viewOffset = new Vector2(0, 0);

        [SerializeField]
        private float m_viewZoom = 1.0f;

        // ----------------------------------------------------------------------------------------
        public Node Root { get { return m_root; } set { m_root = value; } }
        public List<Node> Nodes { get { return m_nodes; } }
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
            else if (typeof(BuffAction).IsAssignableFrom(type))
            {
                return new BaseTypeInfo() { color = new Color(0.4f, 0.0f, 0.3f), side = NodeSide.Right };
            }
            else if (typeof(Shape).IsAssignableFrom(type))
            {
                return new BaseTypeInfo() { color = new Color(0.2f, 0.5f, 0.2f), side = NodeSide.Right };
            }

            return new BaseTypeInfo() { color = new Color(0.2f, 0.2f, 0.2f), side = NodeSide.Right };
        }

        // ----------------------------------------------------------------------------------------
        public Node CreateNode(Type type)
        {
            var node = Activator.CreateInstance(type) as Node;
            node.GraphPosition = m_viewOffset + new Vector2(200, 200);
            m_nodes.Add(node);
            return node;
        }

        // ----------------------------------------------------------------------------------------
        public Node CreateFixedValue(Type valueType)
        {
            var fixedExpressionType = typeof(ExpressionValue<>);
            Type[] typeArgs = { valueType };
            var genericType = fixedExpressionType.MakeGenericType(typeArgs);
            var node = Activator.CreateInstance(genericType) as Node;
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
            try
            {
                fsData data;
                var serializer = CreateSerializer();
                serializer.TrySerialize(GetType(), this, out data).AssertSuccessWithoutWarnings();
                //m_json = fsJsonPrinter.CompressedJson(data);
                m_json = fsJsonPrinter.PrettyJson(data);
                EditorUtility.SetDirty(this);
            }
            catch
            {
            }
        }

        // ----------------------------------------------------------------------------------------
        public void Load()
        {
            try
            {
                var data = fsJsonParser.Parse(m_json);
                var obj = (object)this;
                var serializer = CreateSerializer();
                serializer.TryDeserialize(data, GetType(), ref obj).AssertSuccessWithoutWarnings();
            }
            catch
            {
            }
        }

        // ----------------------------------------------------------------------------------------
        fsSerializer CreateSerializer()
        {
            var serializer = new fsSerializer();
            serializer.AddConverter(new UnityObjectConverter());
            return serializer;
        }

        // ----------------------------------------------------------------------------------------
        public void SetIteratorValue(IteratorType iterator, GameObject gameObject)
        {
        }

        // ----------------------------------------------------------------------------------------
        public Color GetTypeColor(Type type)
        {
            if (typeof(Expression<float>).IsAssignableFrom(type))   return new Color(0.2f, 0.2f, 0.5f);
            if (typeof(Expression<int>).IsAssignableFrom(type))     return new Color(0.8f, 0.8f, 0.0f);
            if (typeof(Action).IsAssignableFrom(type))              return new Color(0.5f, 0.2f, 0.2f);
            if (typeof(BuffAction).IsAssignableFrom(type))          return new Color(0.4f, 0.0f, 0.3f);
            if (typeof(Shape).IsAssignableFrom(type))               return new Color(0.2f, 0.5f, 0.2f);
            return new Color(0.2f, 0.2f, 0.2f);
        }

        // ----------------------------------------------------------------------------------------
        


    }
}