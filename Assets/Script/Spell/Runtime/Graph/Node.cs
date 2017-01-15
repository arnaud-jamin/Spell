using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Spell.Graph
{
    public abstract class Node : INode
    {
        public Vector2 GraphPosition { get; set; }
        public string VariableName { get; set; }
        public virtual bool IsFixedValue { get { return false; } }
        public virtual Type ValueType { get { return null; } }
        public virtual object BoxedValue { get { return null; }  set { } }

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
