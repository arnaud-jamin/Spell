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

        ////-----------------------------------------------------------------------------------------
        //public static Player CreatePlayer(Transform playerParent, Transform characterParent, Player playerPrefab, SpellBook spellBook, Vector3 position, float rotation)
        //{
        //    var player = Instantiate(playerPrefab, playerParent.transform, Vector3.zero, Quaternion.identity);
        //    player.Initialize(spellBook);
        //    player.PlayerCharacterController.Character = CreateCharacter(characterParent, spellBook.Caster, position, rotation);
        //    return player;
        //}

        //-----------------------------------------------------------------------------------------
        public static Unit CreateCharacter(string name, Transform parent, Graph.Unit archetype, Vector3 position, float rotation)
        {
            var unit = Instantiate(name, GlobalSettings.General.UnitPrefab, parent.transform, position, Quaternion.AngleAxis(rotation, Vector3.up));
            unit.Initialize(archetype);
            return unit;
        }
    }
}
