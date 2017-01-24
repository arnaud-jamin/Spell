using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Spell.Graph
{
    public abstract class Node : INode
    {
        protected GameObject m_owner;

        public Vector2 GraphPosition { get; set; }
        public string VariableName { get; set; }
        public virtual Type PrimitiveType { get { return null; } }
        public virtual object PrimitiveValue { get { return null; }  set { } }

        public List<IParameterInfo> GetParameters()
        {
            var parameters = new List<IParameterInfo>();
            var fieldsInfos = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            for (int i = 0; i < fieldsInfos.Length; ++i)
            {
                var fieldInfo = fieldsInfos[i];

                if (typeof(NodeParameter).IsAssignableFrom(fieldInfo.FieldType))
                {
                    parameters.Add(new ParameterInfo(this, fieldInfo));
                }
                else if (typeof(Action).IsAssignableFrom(fieldInfo.FieldType))
                {
                    parameters.Add(new ParameterInfo(this, fieldInfo));
                }
                else if (typeof(INode).IsAssignableFrom(fieldInfo.FieldType))
                {
                    parameters.Add(new ParameterInfo(this, fieldInfo));
                }
                else if (fieldInfo.FieldType.IsGenericType && fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var listItemType = fieldInfo.FieldType.GetGenericArguments()[0];
                    if (typeof(INode).IsAssignableFrom(listItemType))
                    {
                        parameters.Add(new ParameterInfo(this, fieldInfo));
                    }
                }
            }

            return parameters;
        }
    }
}
