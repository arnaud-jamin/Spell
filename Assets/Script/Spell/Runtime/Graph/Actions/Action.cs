using System;
using UnityEditor;
using UnityEngine;

namespace Spell.Graph
{
    public class BaseParameter
    {
        private string m_name;

        public string Name { get { return m_name; } }

        public BaseParameter()
        {
        }

        public BaseParameter(string name)
        {
            m_name = name;
        }

        public virtual void DrawField(Rect rect)
        {
        }
    }

    public abstract class InValue : BaseParameter
    {
        public InValue(string name) : base(name) { }

        public virtual Type ValueType { get { return null; } }
    }

    public abstract class OutValue : BaseParameter
    {
        public OutValue(string name) : base(name) { }

        public virtual Type ValueType { get { return null; } }
    }

    public abstract class BaseAction : BaseParameter
    {
        public BaseAction(string name) : base(name) { }
    }

    public class InAction : BaseAction
    {
        public Action Action;

        public InAction(string name, Action action) : base(name)
        {
            Action = action;
        }

        public void Execute()
        {
            if (Action != null)
            {
                Action();
            }
        }
    }

    public class OutAction : BaseAction
    {
        public InAction InAction;

        public OutAction(string name) : base(name)
        {
        }

        public void Execute()
        {
            if (InAction != null)
            {
                InAction.Execute();
            }
        }
    }

    public class InValue<T> : InValue
    {
        public OutValue<T> OutValue = null;
        private T m_defaultValue;

        public InValue(string name, T defaultValue) : base(name)
        {
            m_defaultValue = defaultValue;
        }

        public override Type ValueType { get { return typeof(T); } }

        public T Value
        {
            get
            {
                if (OutValue != null)
                {
                    return OutValue.Value;
                }
                else
                {
                    return m_defaultValue;
                }
            }

            set
            {
            }
        }

        private object BoxedValue
        {
            get { return m_defaultValue; }
            set { m_defaultValue = (T)value; }
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
        private Func<T> m_func;
        private T m_defaultValue;

        public OutValue(string name, T defaultValue) : base(name)
        {
            m_defaultValue = defaultValue;
        }

        public OutValue(string name, T defaultValue, Func<T> func) : base(name)
        {
            m_defaultValue = defaultValue;
            m_func = func;
        }

        public T Value
        {
            get
            {
                if (m_func != null)
                {
                    return m_func();
                }
                else
                {
                    return m_defaultValue;
                }
            }

            set
            {
                m_defaultValue = value;
            }
        }
    }
}
