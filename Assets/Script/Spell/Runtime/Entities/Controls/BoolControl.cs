using UnityEngine;
using System;

namespace Spell
{
    //---------------------------------------------------------------------------------------------
    [Serializable]
    public class BoolControl
    {
        //-----------------------------------------------------------------------------------------
        private bool m_previousValue = false;

        //-----------------------------------------------------------------------------------------
        [SerializeField]
        private bool m_override = false;

        [SerializeField]
        private bool m_value = false;

        //-----------------------------------------------------------------------------------------
        public bool IsHeld { get { return m_value; } }
        public bool IsPressed { get { return m_value && m_previousValue == false; } }
        public bool IsReleased { get { return m_value == false  && m_previousValue; } }

        //-----------------------------------------------------------------------------------------
        public void Reset()
        {
            m_value = false;
            m_previousValue = false;
        }

        //-----------------------------------------------------------------------------------------
        public void SetValue(bool value)
        {
            if (m_override == false)
            {
                m_value = value;
            }
        }

        //-----------------------------------------------------------------------------------------
        //public void SetValue(PlayerIndex playerIndex, Buttons button)
        //{
        //    m_previousValue = InputManager.GetPreviousGamePad(playerIndex).IsButtonDown(button);
        //    m_value = InputManager.GetGamePad(playerIndex).IsButtonDown(button);
        //}

        //-----------------------------------------------------------------------------------------
        public void Update()
        {
            m_previousValue = m_value;
        }
    }
}