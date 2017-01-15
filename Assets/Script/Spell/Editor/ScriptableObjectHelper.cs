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
            var path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == string.Empty)
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != string.Empty)
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), string.Empty);
            }

            var assetPath = AssetDatabase.GenerateUniqueAssetPath(path + "/" + name + ".asset");

            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
            return asset;
        }

        // ----------------------------------------------------------------------------------------
        [MenuItem("Assets/Create/Spell/Ability")]
        public static void CreateAbilityGraph() { CreateGraph<Ability>(); }

        // ----------------------------------------------------------------------------------------
        [MenuItem("Assets/Create/Spell/Caster")]
        public static void CreateCasterGraph() { CreateGraph<Caster>(); }

        // ----------------------------------------------------------------------------------------
        public static void CreateGraph<RootType>() where RootType : INode
        {
            var graph = ScriptableObjectHelper.CreateAsset<Graph>(typeof(RootType).Name);
            graph.RootType = typeof(RootType);
            graph.Root = graph.CreateNode(graph.RootType);
            graph.Save();
        }
    }
}
