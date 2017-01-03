using UnityEngine;
using System;

namespace Spell
{
    [SelectionBase]
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Stats))]
    public class Character : MonoBehaviour
    {
        //-----------------------------------------------------------------------------------------
        [Serializable]
        public class MovementSettings
        {
            public float rotationSpeed = 10;
        }

        //-----------------------------------------------------------------------------------------
        private Rigidbody m_rigidbody;
        private Model m_model;
        private Stats m_stats = null;
        private Stat m_movementSpeedStat;
        private Stat m_gripStat;
        private float m_rotation = 0;
        private string m_name;

        //-----------------------------------------------------------------------------------------
        [SerializeField]
        private MovementSettings m_movement = null;

        [SerializeField]
        private CharacterControls m_controls = null;

        [SerializeField]
        private Caster m_caster = null;

        //-----------------------------------------------------------------------------------------
        public CharacterControls Controls { get { return m_controls; } }
        public Model Model { get { return m_model; } set { m_model = value; } }
        public Caster Caster { get { return m_caster; } set { SetCaster(value); } }
        public float Rotation { get { return m_rotation; } }
        public string Name { get { return m_name; } }

        //-----------------------------------------------------------------------------------------
        void Awake()
        {
            m_rigidbody = GetComponent<Rigidbody>();
            m_stats = GetComponent<Stats>();
            m_rotation = transform.rotation.eulerAngles.y;
        }

        //-----------------------------------------------------------------------------------------
        void Start()
        {
            m_movementSpeedStat = m_stats.Get(StatType.MovementSpeed);
            m_gripStat = m_stats.Get(StatType.Grip);
        }

        //-----------------------------------------------------------------------------------------
        void SetCaster(Caster caster)
        {
            if (m_caster == caster)
                return;

            m_caster = caster;

            m_name = null;

            if (m_name == null)
            {
                m_name = "Unnamed";
            }
        }

        //-----------------------------------------------------------------------------------------
        private void FixedUpdate()
        {
            Move();
        }

        //-----------------------------------------------------------------------------------------
        void Update()
        {
            var castParam = new CastParam();
            castParam.source = this;
            castParam.castPosition = transform.position;

            if (m_controls.Cast1.IsPressed)
            {
                m_caster.Cast(0, castParam);
            }

            if (m_controls.Cast2.IsPressed)
            {
                m_caster.Cast(1, castParam);
            }

            if (m_controls.Cast3.IsPressed)
            {
                m_caster.Cast(2, castParam);
            }

            if (m_controls.Cast4.IsPressed)
            {
                m_caster.Cast(3, castParam);
            }
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

            var desiredVelocity = m_movementSpeedStat.Value * moveInput;
            var forceVelocity = (desiredVelocity - m_rigidbody.velocity);
            forceVelocity = forceVelocity.Multiply(m_gripStat.Value, 0, m_gripStat.Value);
            m_rigidbody.AddForce(forceVelocity, ForceMode.VelocityChange);

            m_rotation = Mathf.MoveTowardsAngle(m_rotation, desiredRotation, m_movement.rotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.AngleAxis(m_rotation, Vector3.up);
        }
    }
}