using System;
using FullSerializer;
using UnityEngine;

namespace Spell.Graph
{
    public class UnityObjectConverter : fsConverter
    {
        // ----------------------------------------------------------------------------------------
        public override bool CanProcess(Type type)
        {
            if (typeof(MonoBehaviour).IsAssignableFrom(type) || type == typeof(Sprite))
                return true;

            return false;
        }

        // ----------------------------------------------------------------------------------------
        public override fsResult TryDeserialize(fsData data, ref object instance, Type storageType)
        {
            UnityEngine.Object obj = null;
            Settings.AssetDatabase.TryGetAsset(data.AsString, out obj);
            instance = obj;
            return fsResult.Success;
        }

        // ----------------------------------------------------------------------------------------
        public override fsResult TrySerialize(object instance, out fsData serialized, Type storageType)
        {
            var obj = instance as UnityEngine.Object;
            var entry = Settings.AssetDatabase.AddAsset(obj);
            serialized = new fsData(entry.guid);
            return fsResult.Success;
        }

        // ----------------------------------------------------------------------------------------
        public override object CreateInstance(fsData data, Type storageType)
        {
            return -1;
        }
    }
}