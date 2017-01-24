using System;
using UnityEngine;

namespace Spell.Graph
{
    public class NodeParameter
    {
    }

    public class OutValue : NodeParameter
    {
    }

    public class InValue : NodeParameter
    {
    }

    public class InAction : NodeParameter
    {
        public Action Action;

        public InAction()
        {
        }

        public InAction(Action action)
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

        public OutAction()
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
        public T DefaultValue = default(T);

        public InValue()
        {
        }

        public InValue(T defaultValue)
        {
            DefaultValue = defaultValue;
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
                    return DefaultValue;
                }
            }
        }
    }

    public class OutValue<T> : OutValue
    {
        private T m_value = default(T);

        public Func<T> Func = null;

        public OutValue()
        {
        }

        public OutValue(Func<T> func)
        {
            Func = func;
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
                    return m_value;
                }
            }

            set
            {
                m_value = value;
            }
        }
    }
}
