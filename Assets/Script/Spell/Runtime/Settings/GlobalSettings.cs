using System;
using UnityEngine;

namespace Spell
{
    [CreateAssetMenu(fileName = "GlobalSettings", menuName = "Spell/GlobalSettings")]
    public class GlobalSettings : ScriptableObject
    {
        public static GlobalSettings m_instance = null;

        [SerializeField]
        private GeneralSettings general = null;

        [SerializeField]
        private StatsSettings stats = null;

        [SerializeField]
        private AssetDatabase assetDatabase = null;

        public static GlobalSettings Instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = GlobalSettingsLoader.Load();
                    m_instance.Initialize();
                }
                return m_instance;
            }
        }

        public static GeneralSettings General { get { return Instance.general; } }
        public static AssetDatabase AssetDatabase  { get { return Instance.assetDatabase; } }
        public static StatsSettings Stats { get { return Instance.stats; } }

        public void Initialize()
        {
            AssetDatabase.Initialize();
        }
    }
}
