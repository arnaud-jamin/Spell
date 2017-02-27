using UnityEngine;

namespace Spell
{
    public class CameraManager : MonoBehaviour
    {
        //-----------------------------------------------------------------------------------------
        //private GameManager m_gameManager;

        //-----------------------------------------------------------------------------------------
        [SerializeField]
        private Camera m_mainCamera = null;

        //[SerializeField]
        //private float m_distance = 5;

        //[SerializeField]
        //private Vector3 m_offset = new Vector3(0, 0, 0);

        //[SerializeField]
        //private Vector3 m_rotation = new Vector3(0, 0, 0);

        //-----------------------------------------------------------------------------------------
        public Camera MainCamera { get { return m_mainCamera; } }

        //-----------------------------------------------------------------------------------------
        //public void Initialize(GameManager gameManager)
        //{
        //    m_gameManager = gameManager;
        //}

        //-----------------------------------------------------------------------------------------
        public void OnLateUpdate()
        {
            //if (m_mainCamera == null)
            //    return;

            //var unit = m_gameManager.CurrentUnit;
            //var position = (unit != null) ? unit.transform.position : Vector3.zero;
            //m_mainCamera.transform.rotation = Quaternion.Euler(m_rotation);
            //m_mainCamera.transform.position = position + m_offset - m_mainCamera.transform.forward * m_distance;
        }
    }
}
