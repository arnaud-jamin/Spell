using System;
using UnityEditor;
using UnityEngine;

namespace Spell.Graph
{
    [CustomEditor(typeof(AbilityGraph))]
    public class GraphEditor : Editor
    {
        Vector2 m_scroll;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            m_scroll = EditorGUILayout.BeginScrollView(m_scroll);

            var jsonProperty = serializedObject.FindProperty("m_json");
            jsonProperty.stringValue = EditorGUILayout.TextArea(jsonProperty.stringValue);
            EditorGUILayout.EndScrollView();

            serializedObject.ApplyModifiedProperties();
        }
    }
}