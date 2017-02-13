using UnityEngine;
using System;
using Spell.Graph;

namespace Spell
{
    [SelectionBase]
    public class Unit : MonoBehaviour
    {
        //-----------------------------------------------------------------------------------------
        [Serializable]
        public class MovementSettings
        {
            public float rotationSpeed = 10;
        }

        //-----------------------------------------------------------------------------------------
        [Serializable]
        public class References
        {
            [AutoFind]
            public Stats stats = null;

            [AutoFind]
            public Caster caster = null;
        }

        //-----------------------------------------------------------------------------------------
        private Model m_model;
        private Stat m_movementSpeedStat;
        private float m_rotation = 0;

        //-----------------------------------------------------------------------------------------
        [SerializeField]
        private References m_references = null;

        [SerializeField]
        private MovementSettings m_movement = null;

        [SerializeField]
        private CharacterControls m_controls = null;

        //-----------------------------------------------------------------------------------------
        public CharacterControls Controls { get { return m_controls; } }
        public Model Model { get { return m_model; } set { m_model = value; } }
        public float Rotation { get { return m_rotation; } }

        //-----------------------------------------------------------------------------------------
        public void Initialize(Graph.Unit archetype)
        {
            m_rotation = transform.rotation.eulerAngles.y;
            m_movementSpeedStat = m_references.stats.GetStat(StatType.MoveSpeed);

            m_model = GameplayHelper.Instantiate("Model", archetype.Model, transform, Vector3.zero, Quaternion.identity);
        }

        //-----------------------------------------------------------------------------------------
        public void OnUpdate()
        {
            var castParam = new CastParam();
            castParam.source = this;
            castParam.castPosition = transform.position;

            //if (m_controls.Cast1.IsPressed)
            //{
            //    m_caster.Cast(0, castParam);
            //}

            //if (m_controls.Cast2.IsPressed)
            //{
            //    m_caster.Cast(1, castParam);
            //}

            //if (m_controls.Cast3.IsPressed)
            //{
            //    m_caster.Cast(2, castParam);
            //}

            //if (m_controls.Cast4.IsPressed)
            //{
            //    m_caster.Cast(3, castParam);
            //}
        }

        //-----------------------------------------------------------------------------------------
        private void Move()
        {
            var moveInput = new Vector3(m_controls.Move.Value.x, 0, m_controls.Move.Value.y);

            float desiredRotation;
            if (moveInput.magnitude > 0)
            {
                var direction = moveInput / moveInput.magnitude;
                moveInput = direction * moveInput.magnitude;
                desiredRotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            }
            else
            {
                moveInput = Vector3.zero;
                desiredRotation = m_rotation;
            }

            m_rotation = Mathf.MoveTowardsAngle(m_rotation, desiredRotation, m_movement.rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.AngleAxis(m_rotation, Vector3.up);
        }
    }
}