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

        private List<NodeParameter> m_parameters = new List<NodeParameter>();

        protected InValue<T> AddInValue<T>(string name, T defaultValue)
        {
            var inValue = new InValue<T>(name, defaultValue);
            m_parameters.Add(inValue);
            return inValue;
        }

        protected OutValue<T> AddOutValue<T>(string name, T defaultValue)
        {
            var outValue = new OutValue<T>(name, defaultValue);
            m_parameters.Add(outValue);
            return outValue;
        }

        protected OutValue<T> AddOutValue<T>(string name, T defaultValue, Func<T> func)
        {
            var outValue = new OutValue<T>(name, defaultValue, func);
            m_parameters.Add(outValue);
            return outValue;
        }

        protected InAction AddInAction(string name, Action action)
        {
            var inAction = new InAction(name, action);
            m_parameters.Add(inAction);
            return inAction;
        }

        protected OutAction AddOutAction(string name)
        {
            var outAction  = new OutAction(name);
            m_parameters.Add(outAction);
            return outAction;
        }

        public List<IParameterInfo> GetParameters()
        {
            var results = new List<IParameterInfo>();
            var fieldsInfos = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            for (int i = 0; i < fieldsInfos.Length; ++i)
            {
                var fieldInfo = fieldsInfos[i];

                if (typeof(Action).IsAssignableFrom(fieldInfo.FieldType))
                {
                    results.Add(new ParameterInfo(this, fieldInfo));
                }
                else if (typeof(INode).IsAssignableFrom(fieldInfo.FieldType))
                {
                    results.Add(new ParameterInfo(this, fieldInfo));
                }
                else if (fieldInfo.FieldType.IsGenericType && fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var listItemType = fieldInfo.FieldType.GetGenericArguments()[0];
                    if (typeof(INode).IsAssignableFrom(listItemType))
                    {
                        results.Add(new ParameterInfo(this, fieldInfo));
                    }
                }
            }

            foreach (var parameter in m_parameters)
            {
                results.Add(new ParameterInfo(this, parameter));
            }

            return results;
        }
    }
}
