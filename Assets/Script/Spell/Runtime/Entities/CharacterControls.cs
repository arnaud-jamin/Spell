using System;
using UnityEngine;

namespace Spell
{
    [Serializable]
    public class CharacterControls
    {
        //-----------------------------------------------------------------------------------------
        [SerializeField]
        private BoolControl m_cast1 = null;

        [SerializeField]
        private BoolControl m_cast2 = null;

        [SerializeField]
        private BoolControl m_cast3 = null;

        [SerializeField]
        private BoolControl m_cast4 = null;

        [SerializeField]
        private Vector2Control m_move = null;

        //-----------------------------------------------------------------------------------------
        public BoolControl Cast1 { get { return m_cast1; } }

        public BoolControl Cast2 { get { return m_cast2; } }

        public BoolControl Cast3 { get { return m_cast3; } }

        public BoolControl Cast4 { get { return m_cast4; } }

        public Vector2Control Move { get { return m_move; } }

        //-----------------------------------------------------------------------------------------
        public void ResetControls()
        {
            m_cast1.Reset();
            m_cast2.Reset();
            m_cast3.Reset();
            m_cast4.Reset();
            m_move.Reset();
        }

        //-----------------------------------------------------------------------------------------
        public void UpdateControls()
        {
            m_cast1.Update();
            m_cast2.Update();
            m_cast3.Update();
            m_cast4.Update();
            m_move.Update();
        }
    }
}