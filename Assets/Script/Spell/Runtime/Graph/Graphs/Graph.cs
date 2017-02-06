using FullSerializer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Spell.Graph
{
    public class Graph : ScriptableObject, IGraph
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
        protected INode m_root = null;

        [SerializeField]
        protected List<INode> m_nodes = new List<INode>();

        [SerializeField]
        private Vector2 m_viewOffset = new Vector2(0, 0);

        [SerializeField]
        private float m_viewZoom = 1.0f;

        // ----------------------------------------------------------------------------------------
        public INode Root { get { return m_root; } set { m_root = value; } }
        public List<INode> Nodes { get { return m_nodes; } }
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
                m_serializer.TrySerialize(GetType(), this, out data).AssertSuccessWithoutWarnings();
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
                m_serializer.TryDeserialize(data, GetType(), ref obj).AssertSuccessWithoutWarnings();
            }
            catch
            {
            }
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
        private ParameterControlType GetControlType(Type type)
        {
            if (type == typeof(string))             return ParameterControlType.String;
            if (type == typeof(bool))               return ParameterControlType.Bool;
            if (type == typeof(int))                return ParameterControlType.Int;
            if (type == typeof(float))              return ParameterControlType.Float;
            if (type == typeof(Vector2))            return ParameterControlType.Vector2;
            if (type == typeof(Vector3))            return ParameterControlType.Vector3;
            if (type == typeof(Color))              return ParameterControlType.Color;
            if (type == typeof(AnimationCurve))     return ParameterControlType.Curve;
            if (type.Inherit<UnityEngine.Object>()) return ParameterControlType.UnityObject;
            if (type.IsEnumMask())                  return ParameterControlType.Mask;
            if (type.IsEnum)                        return ParameterControlType.Enum;
            return ParameterControlType.None;
        }

        // ----------------------------------------------------------------------------------------
        public List<NewNodeInfo> GetNodes()
        {
            var nodes = new List<NewNodeInfo>();
            for (var i = 0; i < Nodes.Count; ++i)
            {
                var node = Nodes[i];
                var nodeInfo = new NewNodeInfo();
                nodeInfo.index = i;
                nodeInfo.name = node.GetType().Name;
                nodeInfo.color = GetTypeColor(node.GetType());
                nodeInfo.position = node.GraphPosition;
                nodeInfo.parameters = GetNodeParameters((Node)node, i);
                nodes.Add(nodeInfo);
            }
            return nodes;
        }

        // ----------------------------------------------------------------------------------------
        public List<NewParameterInfo> GetNodeParameters(Node node, int nodeIndex)
        {
            var parameters = new List<NewParameterInfo>();

            var fields = node.GetType().GetAllFields();
            for (var i = 0; i < fields.Length; ++i)
            {
                var field = fields[i];

                if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var listItemType = field.FieldType.GetGenericArguments()[0];
                    var parameterInfo = new NewParameterInfo();
                    parameterInfo.index = parameters.Count;
                    parameterInfo.nodeIndex = nodeIndex;
                    parameterInfo.name = field.Name;
                    parameterInfo.hasPin = true;
                    parameterInfo.isList = true;
                    parameterInfo.color = GetTypeColor(listItemType);
                    parameterInfo.controlType = ParameterControlType.None;
                    parameterInfo.connections = new List<NewConnection>();

                    var list = field.GetValue(node) as IList;
                    for (var connectionIndex = 0; connectionIndex < list.Count; ++connectionIndex)
                    {
                        var connectedNode = list[connectionIndex] as INode;
                        var connectedNodeIndex = Nodes.IndexOf(connectedNode);
                        if (connectedNodeIndex != -1)
                        {
                            parameterInfo.connections.Add(new NewConnection() { index = connectionIndex, nodeIndex = connectedNodeIndex });
                        }
                    }
                    parameters.Add(parameterInfo);
                }
                else if (field.FieldType.Inherit<Node>())
                {
                    var parameterInfo = new NewParameterInfo();
                    parameterInfo.index = parameters.Count;
                    parameterInfo.nodeIndex = nodeIndex;
                    parameterInfo.name = field.Name;
                    parameterInfo.hasPin = true;
                    parameterInfo.color = GetTypeColor(field.FieldType);
                    parameterInfo.connections = new List<NewConnection>();

                    var fieldValue = field.GetValue(node);
                    var connectedNodeIndex = Nodes.IndexOf(fieldValue as Node);
                    if (connectedNodeIndex < 0)
                    {
                        if (fieldValue != null)
                        {
                            var subFields = fieldValue.GetType().GetAllFields();
                            if (subFields.Length == 1)
                            {
                                var fieldToControl = subFields[0];
                                parameterInfo.controlType = GetControlType(fieldToControl.FieldType);
                                parameterInfo.controlGetter = () => { return fieldToControl.GetValue(fieldValue); };
                                parameterInfo.controlSetter = (value) => { fieldToControl.SetValue(fieldValue, value); };
                            }
                        }
                    }
                    else
                    {
                        parameterInfo.connections.Add(new NewConnection() { index = 0, nodeIndex = connectedNodeIndex });
                    }
                    parameters.Add(parameterInfo);
                }
                else
                {
                    var parameterInfo = new NewParameterInfo();
                    parameterInfo.index = parameters.Count;
                    parameterInfo.nodeIndex = nodeIndex;
                    parameterInfo.name = field.Name;
                    parameterInfo.color = GetTypeColor(field.FieldType);
                    parameterInfo.controlType = GetControlType(field.FieldType);
                    parameterInfo.controlGetter = () => { return field.GetValue(node); };
                    parameterInfo.controlSetter = (value) => { field.SetValue(node, value); };
                    parameterInfo.connections = new List<NewConnection>();
                    parameters.Add(parameterInfo);
                }
            }

            return parameters;
        }


    }
}