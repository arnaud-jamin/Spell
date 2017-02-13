using System.Collections.Generic;
using UnityEngine;

namespace Spell
{
    [CreateAssetMenu(fileName = "AssetDatabase", menuName = "Spell/AssetDatabase")]
    public class AssetDatabase : ScriptableObject
    {
        private Dictionary<int, Object> m_database;

        public List<Object> Objects;

        public void Initialize()
        {
            m_database = new Dictionary<int, Object>();
            for (int i = 0; i < Objects.Count; ++i)
            {
                m_database[Objects[i].GetInstanceID()] = Objects[i];
            }
        }

        public bool TryGetAsset(int instanceId, out Object obj)
        {
            return m_database.TryGetValue(instanceId, out obj);
        }

        public void AddAsset(Object asset)
        {
            if (Objects.Contains(asset))
                return;

            Objects.Add(asset);
            m_database[asset.GetInstanceID()] = asset;
        }

        public void RemoveAsset(Object asset)
        {
            Objects.Remove(asset);
            m_database.Remove(asset.GetInstanceID());
        }
    }
}
