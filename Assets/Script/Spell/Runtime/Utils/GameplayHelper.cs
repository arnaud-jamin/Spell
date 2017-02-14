using UnityEngine;

namespace Spell
{
    public class GameplayHelper 
    {
        //-----------------------------------------------------------------------------------------
        public static T Instantiate<T>(string name, T original, Transform parent, Vector3 position, Quaternion rotation, bool worldPositionStay = false) where T : Object
        {
            var instance = MonoBehaviour.Instantiate(original, position, rotation) as T;
            instance.name = name;

            Transform instanceTransform = null;
            if (instance is GameObject)
            {
                instanceTransform = (instance as GameObject).transform;
            }
            else
            {
                instanceTransform = (instance as Component).transform;
            }

            if (parent == null)
            {
                parent = GameManager.InstancesRoot;
            }

            instanceTransform.SetParent(parent, worldPositionStay);

            return instance;
        }
    }
}
