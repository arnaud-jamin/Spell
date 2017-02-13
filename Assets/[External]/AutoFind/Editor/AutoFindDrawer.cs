using UnityEngine;
using UnityEditor;

namespace Spell
{
    [CustomPropertyDrawer(typeof(AutoFindAttribute))]
    public class AutoFindDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
        {
            var autoFind = attribute as AutoFindAttribute;
            var type = autoFind.objectType != null ? autoFind.objectType : EditorHelper.GetPropertyType(property);
            var target = property.serializedObject.targetObject as MonoBehaviour;

            if (type == null || target == null)
            {
                EditorGUI.PropertyField(rect, property, label);
            }
            else
            {
                EditorGUI.BeginProperty(rect, label, property);
                var indent = EditorGUI.indentLevel;
                EditorGUI.indentLevel = 0;
                var propertyRect = new Rect(rect.x, rect.y, rect.width * 0.85f, rect.height);
                var buttonRect = new Rect(rect.x + rect.width * 0.85f, rect.y, rect.width * 0.15f, rect.height);
                EditorGUI.PropertyField(propertyRect, property, label);

                if (property.objectReferenceValue == null)
                {
                    if (GUI.Button(buttonRect, new GUIContent("Find", "Find " + type.Name + " in hierarchy"), EditorStyles.miniButton))
                    {
                        var result = autoFind.searchInChildren ? target.GetComponentInChildren(type, true) : target.GetComponent(type);
                        if (result != null)
                        {
                            property.objectReferenceValue = result;
                        }
                    }
                }
                else
                {
                    var leftButtonRect = buttonRect;
                    leftButtonRect.width = buttonRect.width * 0.5f;
                    if (GUI.Button(leftButtonRect, new GUIContent("↺", "Cycle through " + type.Name), EditorStyles.miniButtonLeft))
                    {
                        var results = autoFind.searchInChildren ? target.GetComponentsInChildren(type, true) : target.GetComponents(type);
                        int index = 0;
                        for (var i = 0; i < results.Length; i++)
                        {
                            if (results[i] == property.objectReferenceValue)
                            {
                                index = i;
                                break;
                            }
                        }

                        if (index == results.Length - 1)
                        {
                            index = -1;
                        }
                        index++;
                        property.objectReferenceValue = results[index];
                    }

                    var rightButtonRect = leftButtonRect;
                    rightButtonRect.x = leftButtonRect.x + leftButtonRect.width;
                    if (GUI.Button(rightButtonRect, new GUIContent("✖", "Set to null"), EditorStyles.miniButtonRight))
                    {
                        property.objectReferenceValue = null;
                    }
                }
                EditorGUI.indentLevel = indent;
                EditorGUI.EndProperty();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label);
        }
    }

}