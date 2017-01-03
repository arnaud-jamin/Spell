using UnityEngine;

namespace Spell
{
    public class Stats : MonoBehaviour
    {
        // ----------------------------------------------------------------------------------------
        [SerializeField]
        private Stat[] m_stats = null;

        // ----------------------------------------------------------------------------------------
        void Awake()
        {
            Refresh();
        }

        // ----------------------------------------------------------------------------------------
        void OnValidate()
        {
            Refresh();
        }

        // ----------------------------------------------------------------------------------------
        void Refresh()
        {
            foreach (var stat in m_stats)
            {
                stat.Refresh();
            }
        }

        // ----------------------------------------------------------------------------------------
        void CreateStats()
        {
            int count = Utils.EnumCount<StatType>();
            m_stats = new Stat[count];

            for (int i = 0; i < count; ++i)
            {
                m_stats[i] = new Stat();
            }
        }

        // ----------------------------------------------------------------------------------------
        public Stat Get(StatType type)
        {
            return m_stats[(int)type];
        }
    }
}
