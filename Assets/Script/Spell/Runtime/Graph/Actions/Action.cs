using FullSerializer;
using System;
using UnityEditor;
using UnityEngine;

namespace Spell.Graph
{
    public class BaseParameter
    {
        [fsIgnore]
        private string m_name;

        [fsIgnore]
        private int m_index;

        [fsIgnore]
        private Node m_node;

        [fsIgnore]
        public string Name { get { return m_name; } set { m_name = value; } }

        [fsIgnore]
        public int Index { get { return m_index; } set { m_index = value; } }

        [fsIgnore]
        public Node Node { get { return m_node; } set { m_node = value; } }

        [fsIgnore]
        public virtual BaseParameter ConnectedParameter { get { return null; } set { } }

        public virtual bool CanConnectTo(BaseParameter param)
        {
            return false;
        }

        public virtual void DrawField(Rect rect)
        {
        }

        public virtual Color GetColor()
        {
            return Graph.DefaultColor;
        }

        public virtual Vector2 GetSize()
        {
            return new Vector2(100, 16);
        }

    }

    public abstract class ValueParameter : BaseParameter
    {
        public virtual Type ValueType { get { return null; } }

        // ----------------------------------------------------------------------------------------
        public override Color GetColor()
        {
            if (ValueType == typeof(bool)) return Graph.BoolColor;
            if (ValueType == typeof(int)) return Graph.IntColor;
            if (ValueType == typeof(float)) return Graph.FloatColor;
            if (typeof(Shape).IsAssignableFrom(ValueType)) return Graph.ShapeColor;
            return Graph.DefaultColor;
        }

        // ----------------------------------------------------------------------------------------
        public override Vector2 GetSize()
        {
            if (ValueType == typeof(bool)) return new Vector2(75, 16);
            if (ValueType == typeof(int)) return new Vector2(75, 16);
            if (ValueType == typeof(float)) return new Vector2(75, 16);
            if (ValueType == typeof(Vector3)) return new Vector2(150, 16);
            if (ValueType == typeof(GameObject)) return new Vector2(100, 16);
            if (ValueType.IsEnum) return new Vector2(75, 16);
            return new Vector2(100, 16);
        }
    }

    public abstract class InValue : ValueParameter
    {
    }

    public abstract class OutValue : ValueParameter
    {
    }

    public abstract class ActionParameter : BaseParameter
    {
        public override Color GetColor()
        {
            return Graph.ActionColor;
        }
    }

    public class InAction : ActionParameter
    {
        public Action Action;

        public override bool CanConnectTo(BaseParameter param)
        {
            return (param is OutAction);
        }

        public void Execute()
        {
            if (Action != null)
            {
                Action();
            }
        }
    }

    public class OutAction : ActionParameter
    {
        private InAction m_inAction;

        public override bool CanConnectTo(BaseParameter param)
        {
            return (param is InAction);
        }

        public override BaseParameter ConnectedParameter
        {
            get { return m_inAction; }

            set
            {
                if (value == null)
                {
                    m_inAction = null;
                }
                else
                {
                    m_inAction = value as InAction;
                }
            }
        }

        public void Execute()
        {
            if (m_inAction != null)
            {
                m_inAction.Execute();
            }
        }
    }

    public class InValue<T> : InValue
    {
        private OutValue<T> m_outValue = null;
        public T DefaultValue;

        public override Type ValueType { get { return typeof(T); } }

        public override bool CanConnectTo(BaseParameter param)
        {
            if (param == null)
                return true;

            return param is OutValue<T>;
        }

        public override BaseParameter ConnectedParameter
        {
            get { return m_outValue; }
            set
            {
                if (value == null)
                {
                    m_outValue = null;
                }
                else
                { 
                    m_outValue = value as OutValue<T>;
                }
            }
        }

        public T Value
        {
            get
            {
                if (m_outValue != null)
                {
                    return m_outValue.Value;
                }
                else
                {
                    return DefaultValue;
                }
            }

            set
            {
            }
        }

        private object BoxedValue
        {
            get { return DefaultValue; }
            set { DefaultValue = (T)value; }
        }

        public override void DrawField(Rect rect)
        {
#if UNITY_EDITOR

            var type = typeof(T);

            if (type == typeof(string))
            {
                BoxedValue = EditorGUI.TextField(rect, (string)BoxedValue);
            }
            else if (typeof(UnityEngine.Object).IsAssignableFrom(type))
            {
                BoxedValue = EditorGUI.ObjectField(rect, (UnityEngine.Object)BoxedValue, type, false);
            }
            else if (BoxedValue != null)
            {
                if (type == typeof(bool))
                {
                    BoxedValue = EditorGUI.Toggle(rect, (bool)BoxedValue, "Toggle");
                }
                else if (type == typeof(int))
                {
                    BoxedValue = EditorGUI.IntField(rect, GUIContent.none, (int)BoxedValue, "NodeFieldValue");
                }
                else if (type == typeof(float))
                {
                    BoxedValue = EditorGUI.FloatField(rect, GUIContent.none, (float)BoxedValue, "NodeFieldValue");
                }
                else if (type == typeof(Vector2))
                {
                    BoxedValue = EditorHelper.Vector2Field(rect, (Vector2)BoxedValue, "NodeFieldNameLeft", "NodeFieldValue");
                }
                else if (type == typeof(Vector3))
                {
                    BoxedValue = EditorHelper.Vector3Field(rect, (Vector3)BoxedValue, "NodeFieldNameLeft", "NodeFieldValue");
                }
                else if (type == typeof(Color))
                {
                    GUI.skin = null;
                    BoxedValue = EditorGUI.ColorField(rect, GUIContent.none, (Color)BoxedValue, false, true, false, new ColorPickerHDRConfig(0, 0, 0, 0));
                }
                else if (type.IsEnum)
                {
                    if (type.GetCustomAttributes(typeof(FlagsAttribute), false).Length > 0)
                    {
                        BoxedValue = EditorGUI.MaskField(rect, (int)BoxedValue, Enum.GetNames(type), "NodeFieldValue");
                    }
                    else
                    {
                        BoxedValue = EditorGUI.Popup(rect, (int)BoxedValue, Enum.GetNames(type), "NodeFieldValue");
                    }
                }
            }
#endif
        }
    }

    public class OutValue<T> : OutValue
    {
        public Func<T> Func;
        public T DefaultValue;

        public override Type ValueType { get { return typeof(T); } }

        public override bool CanConnectTo(BaseParameter param)
        {
            if (param == null)
                return true;

            return param is InValue<T>;
        }

        public override BaseParameter ConnectedParameter
        {
            get { return null; }

            set
            {
                var param = value as InValue<T>;
                if (value == null)
                    return;

                param.ConnectedParameter = this;
            }
        }

        public T Value
        {
            get
            {
                if (Func != null)
                {
                    return Func();
                }
                else
                {
                    return DefaultValue;
                }
            }

            set
            {
                DefaultValue = value;
            }
        }
    }
}
