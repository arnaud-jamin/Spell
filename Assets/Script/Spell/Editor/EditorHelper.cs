using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Spell.Graph
{
    public class EditorHelper
    {
        // ----------------------------------------------------------------------------------------
        public static Vector2 Vector2Field(Rect rect, Vector2 value, GUIStyle labelStyle, GUIStyle valueStyle)
        {
            var labelWidth = 15;
            rect.width -= labelWidth * 2;
            var fieldWidth = Mathf.RoundToInt(rect.width / 2.0f);

            var x = rect.x;
            EditorGUI.LabelField(new Rect(x + 2, rect.y, labelWidth, rect.height), "X:", labelStyle);
            x += labelWidth;

            value.x = EditorGUI.FloatField(new Rect(x, rect.y, fieldWidth, rect.height), GUIContent.none, value.x, valueStyle);
            x += fieldWidth;

            EditorGUI.LabelField(new Rect(x + 2, rect.y, labelWidth, rect.height), "Y:", labelStyle);
            x += labelWidth;

            value.y = EditorGUI.FloatField(new Rect(x, rect.y, fieldWidth, rect.height), GUIContent.none, value.y, valueStyle);
            x += fieldWidth;

            return value;
        }

        // ----------------------------------------------------------------------------------------
        public static Vector3 Vector3Field(Rect rect, Vector3 value, GUIStyle labelStyle, GUIStyle valueStyle)
        {
            var labelWidth = 15;
            rect.width -= labelWidth * 3;
            var fieldWidth = Mathf.RoundToInt(rect.width / 3.0f);

            var x = rect.x;
            EditorGUI.LabelField(new Rect(x + 2, rect.y, labelWidth, rect.height), "X:", labelStyle);
            x += labelWidth;

            value.x = EditorGUI.FloatField(new Rect(x, rect.y, fieldWidth, rect.height), GUIContent.none, value.x, valueStyle);
            x += fieldWidth;

            EditorGUI.LabelField(new Rect(x + 2, rect.y, labelWidth, rect.height), "Y:", labelStyle);
            x += labelWidth;

            value.y = EditorGUI.FloatField(new Rect(x, rect.y, fieldWidth, rect.height), GUIContent.none, value.y, valueStyle);
            x += fieldWidth;

            EditorGUI.LabelField(new Rect(x + 2, rect.y, labelWidth, rect.height), "Z:", labelStyle);
            x += labelWidth;

            value.z = EditorGUI.FloatField(new Rect(x, rect.y, fieldWidth, rect.height), GUIContent.none, value.z, valueStyle);
            x += fieldWidth;

            return value;
        }
    }
}
