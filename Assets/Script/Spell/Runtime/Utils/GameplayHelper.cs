using UnityEngine;

namespace Spell
{
    public class GameplayHelper 
    {
        //-----------------------------------------------------------------------------------------
        public static T Instantiate<T>(T original, Transform parent, Vector3 position, Quaternion rotation, bool worldPositionStay = false) where T : Object
        {
            var instance = MonoBehaviour.Instantiate(original, position, rotation) as T;

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

        ////-----------------------------------------------------------------------------------------
        //public static Character CreateCharacter(Transform parent, Caster caster, Vector3 position, float rotation)
        //{
        //    var character = Instantiate(GameManager.CharacterPrefab, parent.transform, position, Quaternion.AngleAxis(rotation, Vector3.up));
        //    character.Model = Instantiate(caster.Model, character.transform, Vector3.zero, Quaternion.identity);
        //    character.Caster = caster;
        //    return character;
        //}


    }
}
