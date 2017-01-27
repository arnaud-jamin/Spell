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
            var graph = target as Graph;

            serializedObject.Update();

            if (GUILayout.Button("Save"))
            {
                graph.Save();
            }

            if (GUILayout.Button("Load"))
            {
                graph.Load();
            }

            if (GUILayout.Button("Clear"))
            {
                graph.Clear();
                graph.Save();
            }

            m_scroll = EditorGUILayout.BeginScrollView(m_scroll);
            EditorGUI.BeginChangeCheck();
            var jsonProperty = serializedObject.FindProperty("m_json");
            var newValue = EditorGUILayout.TextArea(jsonProperty.stringValue);
            EditorGUILayout.EndScrollView();
            if (EditorGUI.EndChangeCheck())
            {
                jsonProperty.stringValue = newValue;
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}