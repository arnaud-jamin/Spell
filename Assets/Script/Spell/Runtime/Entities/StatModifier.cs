using System;
using UnityEngine.Events;

namespace Spell
{
    public class StatModifier
    {
        //-----------------------------------------------------------------------------------------
        public class ValueChangedEvent : UnityEvent { }

        // ----------------------------------------------------------------------------------------
        private float m_value = 0;
        private bool m_isAdditive = false;

        // ----------------------------------------------------------------------------------------
        public ValueChangedEvent ValueChanged = new ValueChangedEvent();

        // ----------------------------------------------------------------------------------------
        public StatModifier() { }

        // ----------------------------------------------------------------------------------------
        public StatModifier(float value)
        {
            m_value = value;
        }

        // ----------------------------------------------------------------------------------------
        public StatModifier(float value, bool isAdditive)
        {
            m_value = value;
            m_isAdditive = isAdditive;
        }

        // ----------------------------------------------------------------------------------------
        public float Value 
        { 
            get { return m_value; }
            set 
            {
                m_value = value; 
                ValueChanged.Invoke();
            }
        }

        // ----------------------------------------------------------------------------------------
        public bool IsAdditive 
        { 
            get { return m_isAdditive; }
            set 
            {
                m_isAdditive = value;
                ValueChanged.Invoke();
            }
        }
    };
}
