using FullSerializer;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Spell.Graph
{
    public abstract class Graph : ScriptableObject, IGraph
    {
        // ----------------------------------------------------------------------------------------
        [fsIgnore]
        private fsSerializer m_serializer = new fsSerializer();

        [SerializeField]
        [fsIgnore]
        protected string m_json;

        [SerializeField]
        protected List<INode> m_nodes = new List<INode>();

        [SerializeField]
        private Vector2 m_viewOffset = new Vector2(0, 0);

        [SerializeField]
        private float m_viewZoom = 1.0f;

        // ----------------------------------------------------------------------------------------
        public List<INode> Nodes { get { return m_nodes; } }
        public Vector2 ViewOffset { get { return m_viewOffset; } set { m_viewOffset = value; } }
        public float ViewZoom { get { return m_viewZoom; } set { m_viewZoom = value; } }

        // ----------------------------------------------------------------------------------------
        public BaseTypeInfo GetBaseTypeInfo(Type type)
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
        public void DestroyNode(INode node)
        {
            m_nodes.Remove(node);

            for (int i = 0; i < m_nodes.Count; ++i)
            {
                var n = m_nodes[i];
                var fields = n.GetFields();
                for (int j = 0; j < fields.Count; ++j)
                {
                    var field = fields[j];
                    if (field.GetValue(n) == node)
                    {
                        DisconnectField(n, field);
                    }
                }
            }
        }

        // ----------------------------------------------------------------------------------------
        public INode DisconnectField(INode node, FieldInfo field)
        {
            var fieldValue = field.GetValue(node) as INode;

            if (fieldValue.ValueType != null)
            {
                var newValue = CreateFixedValue(fieldValue.ValueType);
                field.SetValue(node, newValue);
                return newValue;
            }
            else
            {
                field.SetValue(node, null);
                return null;
            }
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
            m_json = fsJsonPrinter.PrettyJson(data);
        }

        // ----------------------------------------------------------------------------------------
        public void Load()
        {
            var data = fsJsonParser.Parse(m_json);
            var obj = (object)this;
            m_serializer.TryDeserialize(data, GetType(), ref obj).AssertSuccessWithoutWarnings();
        }

        // ----------------------------------------------------------------------------------------
        protected abstract void OnDeserialized(object deserialized);
    } 

    public abstract class Graph<T> : Graph where T : class
    {
        // ----------------------------------------------------------------------------------------
        protected override void OnDeserialized(object deserialized)
        {
            OnDeserialized(deserialized as T);
        }

        // ----------------------------------------------------------------------------------------
        protected abstract void OnDeserialized(T deserialized);
    }
}