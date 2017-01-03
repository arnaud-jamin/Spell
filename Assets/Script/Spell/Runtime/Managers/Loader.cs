using UnityEngine;
using System.Collections;

namespace Spell
{
    public class Loader : MonoBehaviour
    {
        [SerializeField]
        private GameManager m_gameManager = null;

        void Awake()
        {
            if (GameManager.Instance == null)
            {
                Instantiate(m_gameManager);
            }
            Destroy(gameObject);
        }
    }
}
