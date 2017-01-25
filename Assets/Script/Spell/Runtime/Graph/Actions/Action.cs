using System;
using UnityEngine;

namespace Spell.Graph
{
    public class NodeParameter
    {
        private string m_name;

        public string Name { get { return m_name; } }

        public NodeParameter()
        {
        }

        public NodeParameter(string name)
        {
            m_name = name;
        }
    }

    public class OutValue : NodeParameter
    {
        public OutValue(string name)
            : base(name)
        {
        }
    }

    public class InValue : NodeParameter
    {
        public InValue(string name) : base(name)
        {
        }
    }

    public class InAction : NodeParameter
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

    public class OutAction : NodeParameter
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
        }
    }

    public class OutValue<T> : OutValue
    {
        public Func<T> Func = null;
        private T defaultValue;
        private Func<T> func;
        private T m_defaultValue;

        public OutValue(string name, T defaultValue) : base(name)
        {
            m_defaultValue = defaultValue;
        }

        public OutValue(string name, T defaultValue, Func<T> func) : base(name)
        {
            this.defaultValue = defaultValue;
            this.func = func;
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
