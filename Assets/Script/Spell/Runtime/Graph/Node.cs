using FullSerializer;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Spell.Graph
{
    public abstract class Node : INode
    {
        [fsIgnore]
        private GameManager m_gameManager;

        [fsIgnore]
        private Graph m_graph;

        public GameManager GameManager { get { return m_gameManager; } }
        public Graph Graph { get { return m_graph; } }
        public Vector2 GraphPosition { get; set; }
        public string VariableName { get; set; }
        public virtual Type ValueType { get { return null; } }
        public virtual object BoxedValue { get { return null; }  set { } }
        public virtual Type BoxedValueType { get { return null; } }

        public void Initialize(GameManager gameManager, Graph graph)
        {
            m_gameManager = gameManager;
            m_graph = graph;
        }

        public List<FieldInfo> GetFields()
        {
            var fields = new List<FieldInfo>();
            var allFields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            for (int i = 0; i < allFields.Length; ++i)
            {
                var field = allFields[i];
                if (typeof(INode).IsAssignableFrom(field.FieldType))
                {
                    fields.Add(field);
                }
                else if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var listItemType = field.FieldType.GetGenericArguments()[0];
                    if (typeof(INode).IsAssignableFrom(listItemType))
                    {
                        fields.Add(field);
                    }
                }
            }

            return fields;
        }
    }
}
