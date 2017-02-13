using System;
using UnityEngine;

namespace Spell
{
    [CreateAssetMenu(fileName = "GlobalSettingsLoader", menuName = "Spell/GlobalSettingsLoader")]
    public class GlobalSettingsLoader : ScriptableObject
    {
        [SerializeField]
        private GlobalSettings globalSettings = null;

        public static GlobalSettings Load()
        {
            var loader = Resources.Load<GlobalSettingsLoader>("Loader");
            return loader.globalSettings;
        }
    }
}
