﻿using UnityEngine;
using System;
using Spell.Graph;

namespace Spell
{
    [SelectionBase]
    public class Unit : MonoBehaviour
    {
        //-----------------------------------------------------------------------------------------
        [Serializable]
        public class References
        {
            [AutoFind]
            public Rigidbody rigidBody = null;

            [AutoFind]
            public CapsuleCollider collider = null;

            [AutoFind]
            public Stats stats = null;

            [AutoFind]
            public Caster caster = null;

            [AutoFind]
            public Projector selectionFeedback = null;
        }

        //-----------------------------------------------------------------------------------------
        private GameManager m_gameManager;
        private Model m_model;
        private Graph.Unit m_archetype;
        private Stat m_moveSpeedStat;
        private Stat m_collisionSize;
        private Stat m_turnRate;
        private float m_rotation = 0;
        private bool m_isSelected = false;
        private float m_desiredRotation = 0;
        private Vector3 m_desiredPosition = Vector3.zero;

        //-----------------------------------------------------------------------------------------
        [SerializeField]
        private References m_references = null;

        //-----------------------------------------------------------------------------------------
        public GameManager GameManager { get { return m_gameManager; } }
        public Model Model { get { return m_model; } set { m_model = value; } }
        public float Rotation { get { return m_rotation; } }
        public bool IsSelected { get { return m_isSelected; } set { SetIsSelected(value); } }

        //-----------------------------------------------------------------------------------------
        public void Initialize(GameManager gameManager, Graph.Unit archetype)
        {
            m_gameManager = gameManager;

            m_archetype = archetype;
            m_rotation = transform.rotation.eulerAngles.y;

            m_references.stats.Initialize(archetype);
            m_moveSpeedStat = m_references.stats.GetStat(StatType.MoveSpeed);
            m_collisionSize = m_references.stats.GetStat(StatType.CollisionSize);
            m_turnRate = m_references.stats.GetStat(StatType.TurnRate);

            m_references.selectionFeedback.gameObject.SetActive(false);
            m_model = GameplayHelper.Instantiate("Model", archetype.Model, transform, Vector3.zero, Quaternion.identity);
        }

        //-----------------------------------------------------------------------------------------
        public void OnFixedUpdate()
        {
            Move();

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

        bool m_isStopped = true;
        Vector3 m_startPos = Vector3.zero;
        float m_startTime = 0;

        //-----------------------------------------------------------------------------------------
        private void Move()
        {
            m_references.rigidBody.velocity = Vector3.zero;

            var diff = m_desiredPosition - transform.position;
            if (m_isStopped == false)
            {
                if (diff.magnitude > 0.01f)
                {
                    var step = Mathf.Min(diff.magnitude, m_moveSpeedStat.Value * Time.fixedDeltaTime);
                    var direction = diff.normalized;
                    m_references.rigidBody.velocity = (step * direction) / Time.fixedDeltaTime;
                }
                else
                {
                    if (m_isStopped == false)
                    {
                        var distance = Vector3.Distance(transform.position, m_startPos);
                        var time = Time.time - m_startTime;
                        m_desiredPosition = transform.position;
                        Debug.Log("Time:" + time + " Distance:" + distance + " Speed:" + distance / time);
                    }
                    m_isStopped = true;
                }
            }

            m_rotation = Mathf.MoveTowardsAngle(m_rotation, m_desiredRotation, m_turnRate.Value * Time.fixedDeltaTime);
            transform.rotation = Quaternion.AngleAxis(m_rotation, Vector3.up);
        }

        //-----------------------------------------------------------------------------------------
        public void MoveTo(Vector3 destination)
        {
            m_isStopped = false;
            m_startPos = transform.position;
            m_startTime = Time.time;
            m_desiredPosition = destination;

            var diff = m_desiredPosition - transform.position;
            if (diff.magnitude > 0.01f)
            {
                var direction = diff.normalized;
                m_desiredRotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            }
        }

        //-----------------------------------------------------------------------------------------
        public void Attack(Unit selection)
        {
        }

        //-----------------------------------------------------------------------------------------
        private void SetIsSelected(bool value)
        {
            m_isSelected = value;
            m_references.selectionFeedback.gameObject.SetActive(m_isSelected);
        }

        //-----------------------------------------------------------------------------------------
        protected void OnDrawGizmos()
        {
            if (m_archetype != null)
            {
                Gizmos.color = Color.white;
                GizmosHelper.DrawCircle(transform.position, Quaternion.identity, m_collisionSize.Value * 0.5f, 20);
            }
        }
    }
}