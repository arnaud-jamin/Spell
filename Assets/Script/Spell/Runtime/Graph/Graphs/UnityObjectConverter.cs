using System;
using FullSerializer;
using UnityEngine;

namespace Spell.Graph
{
    public class UnityObjectConverter : fsConverter
    {
        public override bool CanProcess(Type type)
        {
            if (typeof(MonoBehaviour).IsAssignableFrom(type)
                || type == typeof(Sprite))
                return true;

            return false;
        }

        public override fsResult TryDeserialize(fsData data, ref object instance, Type storageType)
        {
            UnityEngine.Object obj;
            GlobalSettings.AssetDatabase.TryGetAsset((int)data.AsInt64, out obj);
            instance = obj;
            return fsResult.Success;
        }

        public override fsResult TrySerialize(object instance, out fsData serialized, Type storageType)
        {
            var obj = instance as UnityEngine.Object;
            serialized = new fsData(obj.GetInstanceID());

            if (GlobalSettings.AssetDatabase != null)
            {
                GlobalSettings.AssetDatabase.AddAsset(obj);
                UnityEditor.EditorUtility.SetDirty(GlobalSettings.AssetDatabase);
            }
            return fsResult.Success;
        }

        public override object CreateInstance(fsData data, Type storageType)
        {
            return -1;
        }
    }
}