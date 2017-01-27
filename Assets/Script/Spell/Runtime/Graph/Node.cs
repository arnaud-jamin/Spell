using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Spell.Graph
{
    public abstract class ActionNode : Node
    {
        public override Color Color { get { return Graph.ActionColor; } }
    }

    public abstract class Node : INode
    {
        protected GameObject m_owner;
        public Vector2 GraphPosition { get; set; }
        public string VariableName { get; set; }
        public virtual Type PrimitiveType { get { return null; } }
        public virtual object PrimitiveValue { get { return null; }  set { } }
        public List<BaseParameter> Parameters { get { return m_parameters; } }
        public virtual Color Color { get { return Graph.DefaultColor; } }

        protected List<BaseParameter> m_parameters = new List<BaseParameter>();
        protected List<InValue> m_inValues = new List<InValue>();
        protected List<OutValue> m_outValues = new List<OutValue>();
        protected List<InAction> m_inActions = new List<InAction>();
        protected List<OutAction> m_outActions = new List<OutAction>();

        protected InValue<T> AddInValue<T>(string name, T defaultValue)
        {
            var inValue = new InValue<T> { Name = name, DefaultValue = defaultValue };
            m_inValues.Add(inValue);
            RefreshParameterList();
            return inValue;
        }

        protected OutValue<T> AddOutValue<T>(string name, T defaultValue, Func<T> func = null)
        {
            var outValue = new OutValue<T> { Name = name, DefaultValue = defaultValue, Func = func };
            m_outValues.Add(outValue);
            RefreshParameterList();
            return outValue;
        }

        protected InAction AddInAction(string name, Action action)
        {
            var inAction = new InAction { Name = name, Action = action };
            m_inActions.Add(inAction);
            RefreshParameterList();
            return inAction;
        }

        protected OutAction AddOutAction(string name)
        {
            var outAction  = new OutAction { Name = name };
            m_outActions.Add(outAction);
            RefreshParameterList();
            return outAction;
        }

        private void RefreshParameterList()
        {
            m_parameters.Clear();

            foreach (var action in m_inActions)
            {
                m_parameters.Add(action);
            }

            foreach (var inValue in m_inValues)
            {
                m_parameters.Add(inValue);
            }

            foreach (var action in m_outActions)
            {
                m_parameters.Add(action);
            }

            foreach (var outValue in m_outValues)
            {
                m_parameters.Add(outValue);
            }
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
