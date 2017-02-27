using Spell.Graph;
using UnityEngine;

namespace Spell
{
    public class Field : MonoBehaviour
    {
        // ----------------------------------------------------------------------------------------
        private Graph.Field m_fieldNode;
        private float m_duration;
        private Collider m_collider;
        //private Caster m_creator;

        // ----------------------------------------------------------------------------------------
        public void Initialize(Caster creator, Graph.Field fieldNode)
        {
            //m_creator = creator;
            m_fieldNode = fieldNode;
            m_duration = m_fieldNode.Duration.Evaluate();

            if (m_fieldNode.Shape != null)
            {
                m_collider = m_fieldNode.Shape.CreateCollider(gameObject);
            }
        }

        // ----------------------------------------------------------------------------------------
        public void OnTriggerEnter(Collider collider)
        {
            //m_fieldNode.Selection.Value = collider.attachedRigidbody.gameObject;
            m_fieldNode.OnEnter.Execute();
        }

        // ----------------------------------------------------------------------------------------
        public void OnTriggerStay(Collider collider)
        {
            //m_fieldNode.Selection.Value = collider.attachedRigidbody.gameObject;
            m_fieldNode.OnStay.Execute();
        }

        // ----------------------------------------------------------------------------------------
        public void OnTriggerExit(Collider collider)
        {
            //m_fieldNode.Selection.Value = collider.attachedRigidbody.gameObject;
            m_fieldNode.OnExit.Execute();
        }

        // ----------------------------------------------------------------------------------------
        public void FixedUpdate()
        {
            if (m_fieldNode.Shape != null && m_collider != null)
            {
                m_fieldNode.Shape.UpdateCollider(m_collider);
            }

            m_duration -= Time.deltaTime;
            if (m_duration <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
