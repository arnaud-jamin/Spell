using FullSerializer;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public Color GetColor(Type type)
        {
            return GetTypeColor(type);
        }

        // ----------------------------------------------------------------------------------------
        public Color GetNodeColor(INode node)
        {
            return GetTypeColor(node.GetType());
        }

        // ----------------------------------------------------------------------------------------
        public Color GetParameterColor(IParameterInfo parameter)
        {
            return GetTypeColor(((ParameterInfo)parameter).PrimitiveType);
        }

        // ----------------------------------------------------------------------------------------
        public Vector2 GetPrimitiveNodeSize(INode node)
        {
            return GetFieldSize(node.GetType());
        }

        // ----------------------------------------------------------------------------------------
        public static Color GetTypeColor(Type type)
        {
            if (typeof(float).IsAssignableFrom(type))   return new Color(0.2f, 0.2f, 0.5f);
            if (typeof(int).IsAssignableFrom(type))     return new Color(0.8f, 0.8f, 0.0f);
            if (typeof(Action).IsAssignableFrom(type))  return new Color(0.5f, 0.2f, 0.2f);
            if (typeof(Shape).IsAssignableFrom(type))   return new Color(0.2f, 0.5f, 0.2f);

            return new Color(0.2f, 0.2f, 0.2f);
        }

        // ----------------------------------------------------------------------------------------
        public static Vector2 GetFieldSize(Type type)
        {
            if (type == null)               return new Vector2(100, 16);
            if (type == typeof(float))      return new Vector2(75, 16);
            if (type == typeof(bool))       return new Vector2(75, 16);
            if (type == typeof(int))        return new Vector2(75, 16);
            if (type == typeof(Vector3))    return new Vector2(150, 16);
            if (type == typeof(GameObject)) return new Vector2(100, 16);
            if (type.IsEnum)                return new Vector2(75, 16);
            return new Vector2(100, 16);
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
            m_root = CreateNode(RootType);
        }

        // ----------------------------------------------------------------------------------------
        public void Save()
        {
            fsData data;
            m_serializer.TrySerialize(GetType(), this, out data).AssertSuccessWithoutWarnings();
            //m_json = fsJsonPrinter.CompressedJson(data);
            m_json = fsJsonPrinter.PrettyJson(data);
            EditorUtility.SetDirty(this);
        }

        // ----------------------------------------------------------------------------------------
        public void Load()
        {
            var data = fsJsonParser.Parse(m_json);
            var obj = (object)this;
            m_serializer.TryDeserialize(data, GetType(), ref obj).AssertSuccessWithoutWarnings();
        }

        // ----------------------------------------------------------------------------------------
        public List<NodeTypeInfo> GetAssignableNodes(IParameterInfo parameter)
        {
            return GetAssignableNodes((parameter as ParameterInfo).FieldInfo.FieldType);
        }

        // ----------------------------------------------------------------------------------------
        public List<NodeTypeInfo> GetAllNodes()
        {
            return GetAssignableNodes(typeof(INode));
        }

        // ----------------------------------------------------------------------------------------
        private List<NodeTypeInfo> GetAssignableNodes(Type type)
        {
            // TODO: collapse things like 'Action/' and 'Action/Select' to '' and 'Select'

            var assembly = Assembly.GetAssembly(type);
            var allTypes = assembly.GetTypes();
            var nodeInfos = allTypes.Where(t => type.IsAssignableFrom(t) && t.IsAbstract == false && t.IsInterface == false)
                                    .Select(t => NodeTypeInfo.GetNodeInfo(t))
                                    .Where(t => t.excludeFromMenu == false)
                                    .OrderBy(t => t.menuPath + "/" + t.name).ToList();
            return nodeInfos;
        }

        // ----------------------------------------------------------------------------------------
        public bool CanConnectParameters(IParameterInfo p1, IParameterInfo p2)
        {
            if (p1 != null)
                return true;

            return false;
        }

        // ----------------------------------------------------------------------------------------
        public void ConnectParameters(IParameterInfo p1, IParameterInfo p2)
        {
        }

        // ----------------------------------------------------------------------------------------
        public void DrawField(BaseValue parameter, Rect rect)
        {
#if UNITY_EDITOR

            if (parameter.ValueType == typeof(string))
            {
                parameter.PrimitiveValue = EditorGUI.TextField(rect, (string)parameter.PrimitiveValue);
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(parameter.ValueType))
            {
                parameter.PrimitiveValue = EditorGUI.ObjectField(rect, (UnityEngine.Object)parameter.PrimitiveValue, parameter.ValueType, false);
            }
            else if (parameter.PrimitiveValue != null)
            {
                if (parameter.ValueType == typeof(bool))
                {
                    parameter.PrimitiveValue = EditorGUI.Toggle(rect, (bool)parameter.PrimitiveValue, "Toggle");
                }
                else if (parameter.ValueType == typeof(int))
                {
                    parameter.PrimitiveValue = EditorGUI.IntField(rect, GUIContent.none, (int)parameter.PrimitiveValue, "NodeFieldValue");
                }
                else if (parameter.ValueType == typeof(float))
                {
                    parameter.PrimitiveValue = EditorGUI.FloatField(rect, GUIContent.none, (float)parameter.PrimitiveValue, "NodeFieldValue");
                }
                else if (parameter.ValueType == typeof(Vector2))
                {
                    parameter.PrimitiveValue = EditorHelper.Vector2Field(rect, (Vector2)parameter.PrimitiveValue, "NodeFieldNameLeft", "NodeFieldValue");
                }
                else if (parameter.ValueType == typeof(Vector3))
                {
                    parameter.PrimitiveValue = EditorHelper.Vector3Field(rect, (Vector3)parameter.PrimitiveValue, "NodeFieldNameLeft", "NodeFieldValue");
                }
                else if (parameter.ValueType == typeof(Color))
                {
                    GUI.skin = null;
                    parameter.PrimitiveValue = EditorGUI.ColorField(rect, GUIContent.none, (Color)parameter.PrimitiveValue, false, true, false, new ColorPickerHDRConfig(0, 0, 0, 0));
                }
                else if (parameter.ValueType.IsEnum)
                {
                    if (parameter.ValueType.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0)
                    {
                        parameter.PrimitiveValue = EditorGUI.MaskField(rect, (int)parameter.PrimitiveValue, Enum.GetNames(parameter.ValueType), "NodeFieldValue");
                    }
                    else
                    {
                        parameter.PrimitiveValue = EditorGUI.Popup(rect, (int)parameter.PrimitiveValue, Enum.GetNames(parameter.ValueType), "NodeFieldValue");
                    }
                }
            }
#endif
        }
    }
}