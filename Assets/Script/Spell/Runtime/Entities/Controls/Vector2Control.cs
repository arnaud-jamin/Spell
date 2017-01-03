using UnityEngine;
using System;

namespace Spell
{
    //---------------------------------------------------------------------------------------------
    [Serializable]
    public class Vector2Control
    {
        //-----------------------------------------------------------------------------------------
        [SerializeField]
        private bool m_override = false;

        [SerializeField]
        private Vector2 m_value = Vector2.zero;

        //-----------------------------------------------------------------------------------------
        public Vector2 Value { get { return m_value; } }
        
        //-----------------------------------------------------------------------------------------
        public void Reset()
        {
            m_value = Vector2.zero;
        }

        //-----------------------------------------------------------------------------------------
        public void SetValue(Vector2 value)
        {
            if (m_override == false)
            {
                m_value = value;
            }
        }

        //-----------------------------------------------------------------------------------------
        public void SetValue(string inputX, string inputY)
        {
            m_value = new Vector2(Input.GetAxis(inputX), Input.GetAxis(inputY));
        }

        //-----------------------------------------------------------------------------------------
        public void Update()
        {
        }
    }
}