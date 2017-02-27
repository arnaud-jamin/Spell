using System;
using UnityEngine;

namespace Spell
{
    [CreateAssetMenu(fileName = "GlobalSettingsLoader", menuName = "Spell/GlobalSettingsLoader")]
    public class GlobalSettingsLoader : ScriptableObject
    {
        [SerializeField]
        private Settings globalSettings = null;

        public static Settings Load()
        {
            var loader = Resources.Load<GlobalSettingsLoader>("Loader");
            return loader.globalSettings;
        }
    }
}
