using UnityEngine;

namespace Spell
{
    public class CameraManager : MonoBehaviour
    {
        //-----------------------------------------------------------------------------------------
        [SerializeField]
        private Camera m_mainCamera = null;

        [SerializeField]
        private float m_distance = 5;

        [SerializeField]
        private Vector3 m_offset = new Vector3(0, 0, 0);

        [SerializeField]
        private Vector3 m_rotation = new Vector3(0, 0, 0);

        //-----------------------------------------------------------------------------------------
        public void ManagerUpdate()
        {
            if (m_mainCamera == null)
                return;

            var player = GameManager.ActivePlayer;
            m_mainCamera.transform.rotation = Quaternion.Euler(m_rotation);
            m_mainCamera.transform.position = player.Unit.transform.position + m_offset - m_mainCamera.transform.forward * m_distance;
        }
    }
}
