using UnityEngine;

namespace Spell
{
    public class Model : MonoBehaviour
    {
        //-----------------------------------------------------------------------------------------
        [SerializeField]
        private float m_radius = 0.5f;

        //-----------------------------------------------------------------------------------------
        public float Radius { get { return m_radius; } }

        //-----------------------------------------------------------------------------------------
        public void OnDrawGizmos()
        {
            GizmosHelper.DrawCircle(transform.position, Quaternion.identity, m_radius, 40);
        }
    }
}