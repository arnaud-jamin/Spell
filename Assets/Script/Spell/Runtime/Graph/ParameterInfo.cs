using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Spell.Graph
{
    public class ParameterInfo : IParameterInfo
    {
        // ----------------------------------------------------------------------------------------
        private Node m_node;
        private FieldInfo m_fieldInfo;
        private ParameterSide m_side;
        private bool m_isList;
        private Type m_primitiveType;
        private string m_name;
        private BaseParameter m_param;

        // ----------------------------------------------------------------------------------------
        public string Name { get { return m_name; } }
        public ParameterSide Side { get { return m_side; } }
        public INode ConnectedNode { get { return m_fieldInfo != null ? m_fieldInfo.GetValue(m_node) as INode : null; } }
        public bool IsList { get { return m_isList; } }
        public IList List { get { return m_fieldInfo != null ? m_fieldInfo.GetValue(m_node) as IList : null; } }

        // ----------------------------------------------------------------------------------------
        public FieldInfo FieldInfo { get { return m_fieldInfo;  } }
        public Type PrimitiveType { get { return m_primitiveType; } }
        public BaseParameter Parameter { get { return m_param; } }

        // ----------------------------------------------------------------------------------------
        public ParameterInfo(Node node, FieldInfo fieldInfo)
        {
            m_node = node;
            m_fieldInfo = fieldInfo;
            m_name = m_fieldInfo.Name;

            var sideAttribute = fieldInfo.GetCustomAttributes(typeof(ParameterSideAttribute), false).FirstOrDefault() as ParameterSideAttribute;
            m_side = (sideAttribute != null) ? sideAttribute.Side : ParameterSide.Left;

            m_isList = m_fieldInfo.FieldType.IsGenericType && m_fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(List<>);

            m_primitiveType = null;
            if (m_isList)
            {
                m_primitiveType = m_fieldInfo.FieldType.GetGenericArguments()[0];
            }
            else if (ConnectedNode != null)
            {
                m_primitiveType = ConnectedNode.PrimitiveType;
            }
        }

        // ----------------------------------------------------------------------------------------
        public ParameterInfo(Node node, BaseParameter param)
        {
            m_param = param;
            m_node = node;
            m_fieldInfo = null;
            m_name = param.Name;

            m_side = ParameterSide.Left;

            if (typeof(InAction).IsAssignableFrom(param.GetType()))
            {
                m_side = ParameterSide.Left;
            }
            else if (typeof(OutAction).IsAssignableFrom(param.GetType()))
            {
                m_side = ParameterSide.Right;
            }
            else if (typeof(InValue).IsAssignableFrom(param.GetType()))
            {
                m_side = ParameterSide.Left;
            }
            else if (typeof(OutValue).IsAssignableFrom(param.GetType()))
            {
                m_side = ParameterSide.Right;
            }

            m_isList = false;
        }
    }
}
