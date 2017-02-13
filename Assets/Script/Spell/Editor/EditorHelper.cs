using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Spell
{
    public class EditorHelper
    {
        // ----------------------------------------------------------------------------------------
        public static Type GetPropertyType(SerializedProperty property)
        {
            var paths = property.propertyPath.Split('.');

            var type = property.serializedObject.targetObject.GetType();
            for (var i = 0; i < paths.Length; ++i)
            {
                var path = paths[i];
                var field = type.GetField(path, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                if (field == null)
                    return null;

                type = field.FieldType;
            }

            return type;
        }

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

        // ----------------------------------------------------------------------------------------
        public static void ZoomedEnumField(System.Action beginZoom, System.Action endZoom, Rect rect, Vector2 menuScreenPosition, GUIStyle style, int index, string[] names, Action<int> action)
        {
            if (GUI.Button(rect, names[index], style))
            {
                endZoom();
                var menu = new GenericMenu();
                for (var i = 0; i < names.Length; i++)
                {
                    menu.AddItem(new GUIContent(names[i]), i == index, (it) => { action((int)it); }, i);
                }
                menu.DropDown(new Rect(menuScreenPosition, Vector2.zero));
                beginZoom();
            }
        }

        // ----------------------------------------------------------------------------------------
        public static void ZoomedMaskField(System.Action beginZoom, System.Action endZoom, Rect rect, Vector2 menuScreenPosition, GUIStyle style, int currentValue, string[] names, Action<int> action)
        {
            var isEverythingOn = true;
            var isNothingOn = false;

            var text = string.Empty;
            var matches = 0;
            for (var i = 0; i < names.Length; i++)
            {
                var itemValue = 1 << i;
                if ((itemValue & currentValue) != 0)
                {
                    if (matches == 0)
                    {
                        text = names[i];
                    }
                    else if (matches > 1)
                    {
                        text = "Mixed";
                    }
                    else if (matches == 1)
                    {
                        text = text + ", " + names[i];
                    }

                    matches++;
                }
                else
                {
                    isEverythingOn = false;
                }
            }

            if (isEverythingOn)
            {
                text = "Everything";
            }
            else if (matches == 0)
            {
                isNothingOn = true;
                text = "Nothing";
            }

            if (GUI.Button(rect, text, style))
            {
                endZoom();

                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Nothing"), isNothingOn, () =>
                {
                    action(0);
                });

                menu.AddItem(new GUIContent("Everything"), isEverythingOn, () =>
                {
                    action(-1);
                });

                menu.AddSeparator(string.Empty);

                for (var i = 0; i < names.Length; i++)
                {
                    var itemValue = (1 << i);
                    var isItemOn = (currentValue & itemValue) != 0;
                    menu.AddItem(new GUIContent(names[i]), isItemOn, () => { action(isItemOn ? currentValue ^ itemValue : currentValue | itemValue); });
                }

                menu.DropDown(new Rect(menuScreenPosition, Vector2.zero));

                beginZoom();
            }
        }
    }
}
