using UnityEngine;
using System;
using Spell.Graph;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Spell
{
    [SelectionBase]
    public class Unit : MonoBehaviour
    {
        //-----------------------------------------------------------------------------------------
        [Serializable]
        public class ReferencesSettings
        {
            [AutoFind]
            public Rigidbody rigidBody = null;

            [AutoFind]
            public CapsuleCollider collider = null;

            [AutoFind]
            public Projector selectionFeedback = null;
        }

        //-----------------------------------------------------------------------------------------
        public class CastEvent : UnityEvent<Ability.CastEventArgs> { }

        //-----------------------------------------------------------------------------------------
        private GameManager m_gameManager;
        private Model m_model;
        private Stats m_stats;
        private Health m_health;
        private Mana m_mana;
        private List<Ability> m_abilities = new List<Ability>();
        private List<GameObject> m_summoned = new List<GameObject>();

        private Graph.Unit m_archetype;
        private Stat m_moveSpeedStat;
        private Stat m_collisionSize;
        private Stat m_turnRate;
        private float m_rotation = 0;
        private bool m_isSelected = false;
        private float m_desiredRotation = 0;
        private Vector3 m_desiredPosition = Vector3.zero;

        // temp
        private bool m_isStopped = true;
        private Vector3 m_startPos = Vector3.zero;
        private float m_startTime = 0;

        //-----------------------------------------------------------------------------------------
        [SerializeField]
        private ReferencesSettings m_references = null;

        //-----------------------------------------------------------------------------------------
        public GameManager GameManager { get { return m_gameManager; } }
        public Graph.Unit Archetype { get { return m_archetype; } }
        public Stats Stats { get { return m_stats; } }
        public Health Health { get { return m_health; } }
        public Mana Mana { get { return m_mana; } }
        public Model Model { get { return m_model; } set { m_model = value; } }
        public float Rotation { get { return m_rotation; } }
        public bool IsSelected { get { return m_isSelected; } set { SetIsSelected(value); } }
        public List<Ability> Abilities { get { return m_abilities; } }
        public List<GameObject> Summoned { get { return m_summoned; } }

        //---------------------------------------------------------------------------------------
        public CastEvent AbilityCasted = new CastEvent();

        //-----------------------------------------------------------------------------------------
        public void Initialize(GameManager gameManager, Graph.Unit archetype)
        {
            m_gameManager = gameManager;

            m_archetype = archetype;
            m_rotation = transform.rotation.eulerAngles.y;

            m_stats = new Stats(archetype);
            m_moveSpeedStat = m_stats.GetStat(StatType.MoveSpeed);
            m_collisionSize = m_stats.GetStat(StatType.CollisionSize);
            m_turnRate = m_stats.GetStat(StatType.TurnRate);

            m_health = new Health(this);
            m_mana = new Mana(this);

            m_references.selectionFeedback.gameObject.SetActive(false);
            m_model = GameplayHelper.Instantiate("Model", archetype.Model, transform, Vector3.zero, Quaternion.identity);

            for (int i = 0; i < m_archetype.Abilities.Count; ++i)
            {
                var abilityArchetype = m_archetype.Abilities[i].Evaluate();
                var ability = new Ability(abilityArchetype);
                m_abilities.Add(ability);
            }
        }

        //-----------------------------------------------------------------------------------------
        public void OnFixedUpdate()
        {
            Move();

            for (int i = 0; i < m_abilities.Count; ++i)
            {
                var ability = m_abilities[i];
                ability.OnFixedUpdate();
            }
        }

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

        //---------------------------------------------------------------------------------------
        public void Cast(Ability.CastParam castParam)
        {
            if (castParam.abilityIndex < 0 || castParam.abilityIndex >= m_abilities.Count)
                return;

            var ability = m_abilities[castParam.abilityIndex];
            var manaCost = ability.Archetype.ManaCost;

            if (m_mana.CurrentValue < manaCost)
                return;

            ability.Cast(castParam);

            AbilityCasted.Invoke(new Ability.CastEventArgs { ability = ability, param = castParam });
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