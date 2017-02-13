using System;
using UnityEditor;
using UnityEngine;

namespace Spell.Graph
{
    [CustomEditor(typeof(Graph))]
    public class GraphInspectorEditor : Editor
    {
        Vector2 m_scroll;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var jsonProperty = serializedObject.FindProperty("m_json");

            if (GUILayout.Button("Copy to clipboard"))
            {
                GUIUtility.systemCopyBuffer = jsonProperty.stringValue;
            }

            m_scroll = EditorGUILayout.BeginScrollView(m_scroll);
            jsonProperty.stringValue = EditorGUILayout.TextArea(jsonProperty.stringValue);
            EditorGUILayout.EndScrollView();


            
            serializedObject.ApplyModifiedProperties();
        }
    }
}