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
        private Color m_color;
        private Type m_primitiveType;
        private Vector2 m_size;
        private string m_name;
        private BaseParameter m_param;

        // ----------------------------------------------------------------------------------------
        public string Name { get { return m_name; } }
        public Color Color { get { return m_color;  } }
        public ParameterSide Side { get { return m_side; } }
        public INode ConnectedNode { get { return m_fieldInfo != null ? m_fieldInfo.GetValue(m_node) as INode : null; } }
        public bool IsList { get { return m_isList; } }
        public IList List { get { return m_fieldInfo != null ? m_fieldInfo.GetValue(m_node) as IList : null; } }
        public Vector2 Size { get { return m_size; } }

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

            m_color = Graph.GetTypeColor(m_primitiveType);
            m_size = Graph.GetFieldSize(m_primitiveType);
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
            else if (typeof(InValue<>).IsAssignableFrom(param.GetType()))
            {
                m_side = ParameterSide.Left;
            }
            else if (typeof(OutValue<>).IsAssignableFrom(param.GetType()))
            {
                m_side = ParameterSide.Right;
            }

            if (param is BaseValue)
            {
                m_primitiveType = ((BaseValue)param).ValueType;
            }

            m_isList = false;
            m_color = Graph.GetTypeColor(m_primitiveType);
            m_size = Graph.GetFieldSize(m_primitiveType);
        }

        // ----------------------------------------------------------------------------------------
        public bool CanConnectToParameter(IParameterInfo parameter)
        {
            return m_fieldInfo.FieldType.IsAssignableFrom(((ParameterInfo)parameter).m_fieldInfo.FieldType);
        }

        // ----------------------------------------------------------------------------------------
        public bool ConnectToParameter(IParameterInfo parameter)
        {
            return ConnectToNode(parameter.ConnectedNode);
        }

        // ----------------------------------------------------------------------------------------
        public bool CanConnectToNode(INode node)
        {
            var currentValue = ConnectedNode;
            if (currentValue == node)
                return false;

            if (IsList && typeof(INode).IsAssignableFrom(m_primitiveType))
                return true;

            if (m_fieldInfo.FieldType.IsAssignableFrom(node.GetType()))
                return true;

            return false;
        }

        // ----------------------------------------------------------------------------------------
        public bool ConnectToNode(INode nodeToConnect)
        {
            var currentValue = ConnectedNode;
            if (currentValue == nodeToConnect)
                return false;

            // TODO: manage cycles:
            //parameter.connections.Add(new NodeConnection() { connectedNode = value, index = 0, pin = parameter });
            //var isCreatingCycle = IsCreatingCycle(value, value);
            //parameter.connections.RemoveAt(parameter.connections.Count - 1);
            //if (isCreatingCycle)
            //    return;

            if (IsList)
            {
                if (typeof(INode).IsAssignableFrom(m_primitiveType))
                {
                    var list = List;
                    if (list.Contains(nodeToConnect) == false)
                    {
                        list.Add(nodeToConnect);
                        return true;
                    }
                }
            }
            else if (m_fieldInfo.FieldType.IsAssignableFrom(nodeToConnect.GetType()))
            {
                m_fieldInfo.SetValue(m_node, nodeToConnect);

                // When we create a connection we try to retain the value
                // of the old node to the new one.
                if (currentValue != null && currentValue.PrimitiveValue != null)
                {
                    nodeToConnect.PrimitiveValue = currentValue.PrimitiveValue;
                }
            }
            return false;
        }

        // ----------------------------------------------------------------------------------------
        public void Disconnect(IGraph graph, INode node)
        {
            if (IsList)
            {
                List.Remove(node);
            }
            else
            {
                if (m_primitiveType != null)
                {
                    var primitiveValue = ConnectedNode.PrimitiveValue;
                    m_fieldInfo.SetValue(m_node, graph.CreateFixedValue(m_primitiveType));

                    // When we delete a connection we try to retain the value
                    // of the old node to the new one.
                    if (primitiveValue != null)
                    {
                        ConnectedNode.PrimitiveValue = primitiveValue;
                    }
                }
                else
                {
                    m_fieldInfo.SetValue(m_node, null);
                }
            }
        }
    }
}
