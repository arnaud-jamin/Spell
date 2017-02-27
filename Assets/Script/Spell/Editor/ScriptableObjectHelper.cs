using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Spell.Graph
{
    public class ScriptableObjectHelper
    {
        // ----------------------------------------------------------------------------------------
        public static T CreateAsset<T>(string name) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            var path = UnityEditor.AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == string.Empty)
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != string.Empty)
            {
                path = path.Replace(Path.GetFileName(UnityEditor.AssetDatabase.GetAssetPath(Selection.activeObject)), string.Empty);
            }

            var assetPath = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(path + "/" + name + ".asset");

            UnityEditor.AssetDatabase.CreateAsset(asset, assetPath);
            UnityEditor.AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            return asset;
        }

        // ----------------------------------------------------------------------------------------
        [MenuItem("Assets/Create/Spell/Ability")]
        public static void CreateAbilityGraph() { CreateGraph<AbilityGraph>(); }

        // ----------------------------------------------------------------------------------------
        [MenuItem("Assets/Create/Spell/Unit")]
        public static void CreateCasterGraph() { CreateGraph<UnitGraph>(); }

        // ----------------------------------------------------------------------------------------
        [MenuItem("Assets/Create/Spell/Buff")]
        public static void CreateBuffGraph() { CreateGraph<BuffGraph>(); }

        // ----------------------------------------------------------------------------------------
        public static void CreateGraph<T>() where T : Graph
        {
            var graph = CreateAsset<T>(typeof(T).Name);
            graph.CreateRoot();
            graph.Save();
        }
    }
}
