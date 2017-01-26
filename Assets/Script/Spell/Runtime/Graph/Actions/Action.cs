using System;
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
    }


    public abstract class BaseValue : BaseParameter
    {
        public BaseValue(string name) : base(name) { }
        public abstract object PrimitiveValue { get; set; }
        public virtual Type ValueType { get { return null; } }
    }

    public abstract class BaseValue<T> : BaseValue
    {
        public BaseValue(string name) : base(name) { }
        public abstract T Value { get; set; }
        public override Type ValueType { get { return typeof(T); } }
    }

    public class InAction : BaseParameter
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

    public class OutAction : BaseParameter
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

    public class InValue<T> : BaseValue<T>
    {
        public OutValue<T> OutValue = null;
        private T m_defaultValue;

        public InValue(string name, T defaultValue) : base(name)
        {
            m_defaultValue = defaultValue;
        }

        public override object PrimitiveValue
        {
            get { return Value; }
            set { Value = (T)value; }
        }

        public override T Value
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
    }

    public class OutValue<T> : BaseValue<T>
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

        public override object PrimitiveValue
        {
            get { return Value; }
            set { Value = (T)value; }
        }

        public override T Value
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
