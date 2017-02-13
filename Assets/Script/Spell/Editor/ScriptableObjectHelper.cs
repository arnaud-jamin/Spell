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
        public static void CreateAbilityGraph() { CreateGraph<AbilityGraph, Ability>(); }

        // ----------------------------------------------------------------------------------------
        [MenuItem("Assets/Create/Spell/Caster")]
        public static void CreateCasterGraph() { CreateGraph<CasterGraph, Unit>(); }

        // ----------------------------------------------------------------------------------------
        [MenuItem("Assets/Create/Spell/Buff")]
        public static void CreateBuffGraph() { CreateGraph<BuffGraph, Buff>(); }

        // ----------------------------------------------------------------------------------------
        public static void CreateGraph<GraphType, RootType>() where GraphType: Graph where RootType : Node
        {
            var graph = ScriptableObjectHelper.CreateAsset<GraphType>(typeof(RootType).Name);
            graph.RootType = typeof(RootType);
            graph.Root = graph.CreateNode(graph.RootType);
            graph.Save();
        }
    }
}
