using System.Collections.Generic;
using UnityEngine;

namespace Spell
{
    [CreateAssetMenu(fileName = "AssetDatabase", menuName = "Spell/AssetDatabase")]
    public class AssetDatabase : ScriptableObject
    {
        // ----------------------------------------------------------------------------------------
        [System.Serializable]
        public class Entry
        {
            public Object asset;
            public string guid;
        }

        // ----------------------------------------------------------------------------------------
        private Dictionary<string, Object> m_database;

        [SerializeField]
        private List<Entry> m_objects = null;

        // ----------------------------------------------------------------------------------------
        public void Initialize()
        {
            m_database = new Dictionary<string, Object>();
            for (int i = 0; i < m_objects.Count; ++i)
            {
                var obj = m_objects[i];
                m_database[obj.guid] = obj.asset;
            }
        }

        // ----------------------------------------------------------------------------------------
        public bool TryGetAsset(string guid, out Object obj)
        {
            return m_database.TryGetValue(guid, out obj);
        }

#if UNITY_EDITOR
        // ----------------------------------------------------------------------------------------
        public Entry AddAsset(Object asset)
        {
            var entry = m_objects.Find(e => e.asset == asset);
            if (entry != null)
                return entry;

            entry = new Entry { asset = asset, guid = System.Guid.NewGuid().ToString() };

            Debug.Log("Adding asset " + asset.name + " " + entry.guid);

            m_objects.Add(entry);
            m_database[entry.guid] = asset;

            UnityEditor.EditorUtility.SetDirty(Settings.AssetDatabase);

            return entry;
        }
#endif //UNITY_EDITOR
    }
}
