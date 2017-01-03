using Spell.Graph;
using UnityEngine;

namespace Spell.Entities
{
    public class Field : MonoBehaviour
    {
        // ----------------------------------------------------------------------------------------
        private Graph.Field m_fieldNode;
        private float m_duration;
        //private Collider m_collider;
        //private Caster m_creator;

        // ----------------------------------------------------------------------------------------
        public void Initialize(Caster creator, Graph.Field fieldNode)
        {
            //m_creator = creator;
            m_fieldNode = fieldNode;
            m_duration = m_fieldNode.Duration.Evaluate();

            if (m_fieldNode is CircleField)
            {
                var circleField = m_fieldNode as CircleField;
                var spehreCollider = gameObject.AddComponent<SphereCollider>();
                spehreCollider.radius = circleField.Radius.Evaluate();
                //m_collider = spehreCollider;
            }
            else if (m_fieldNode is BoxField)
            {
                var boxField = m_fieldNode as BoxField;
                var boxCollider = gameObject.AddComponent<BoxCollider>();
                boxCollider.size = new Vector3(boxField.Width.Evaluate(), 0, boxField.Height.Evaluate());
                //m_collider = boxCollider;
            }
        }

        // ----------------------------------------------------------------------------------------
        public void OnTriggerEnter(Collider collider)
        {
            m_fieldNode.Selection.Value = collider.attachedRigidbody.gameObject;
            m_fieldNode.OnEnter.Execute();
        }

        // ----------------------------------------------------------------------------------------
        public void OnTriggerStay(Collider collider)
        {
            m_fieldNode.Selection.Value = collider.attachedRigidbody.gameObject;
            m_fieldNode.OnStay.Execute();
        }

        // ----------------------------------------------------------------------------------------
        public void OnTriggerExit(Collider collider)
        {
            m_fieldNode.Selection.Value = collider.attachedRigidbody.gameObject;
            m_fieldNode.OnExit.Execute();
        }

        // ----------------------------------------------------------------------------------------
        public void FixedUpdate()
        {
            m_duration -= Time.deltaTime;
            if (m_duration <= 0)
            {
                Destroy(gameObject);
            }
        }
    }
}
