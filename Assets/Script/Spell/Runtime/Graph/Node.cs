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
            var inValue = new InValue<T> { Node = this, Name = name, DefaultValue = defaultValue };
            inValue.Node = this;
            m_inValues.Add(inValue);
            RefreshParameterList();
            return inValue;
        }

        protected OutValue<T> AddOutValue<T>(string name, Func<T> func = null)
        {
            var outValue = new OutValue<T> { Node = this, Name = name, Func = func };
            outValue.Node = this;
            m_outValues.Add(outValue);
            RefreshParameterList();
            return outValue;
        }

        protected InAction AddInAction(string name, Action action)
        {
            var inAction = new InAction { Node = this, Name = name, Action = action };
            inAction.Node = this;
            m_inActions.Add(inAction);
            RefreshParameterList();
            return inAction;
        }

        protected OutAction AddOutAction(string name)
        {
            var outAction  = new OutAction { Node = this, Name = name };
            outAction.Node = this;
            m_outActions.Add(outAction);
            RefreshParameterList();
            return outAction;
        }

        private void RefreshParameterList()
        {
            m_parameters.Clear();

            for (int i = 0; i < m_inActions.Count; ++i)
            {
                m_inActions[i].Index = m_parameters.Count;
                m_parameters.Add(m_inActions[i]);
            }

            for (int i = 0; i < m_inValues.Count; ++i)
            {
                m_inValues[i].Index = m_parameters.Count;
                m_parameters.Add(m_inValues[i]);
            }

            for (int i = 0; i < m_outActions.Count; ++i)
            {
                m_outActions[i].Index = m_parameters.Count;
                m_parameters.Add(m_outActions[i]);
            }

            for (int i = 0; i < m_outValues.Count; ++i)
            {
                m_outValues[i].Index = m_parameters.Count;
                m_parameters.Add(m_outValues[i]);
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
