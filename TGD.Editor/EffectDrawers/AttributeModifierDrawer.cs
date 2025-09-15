using UnityEditor;
using UnityEngine;

namespace TGD.Editor
{
    public class AttributeModifierDrawer : IEffectDrawer
    {
        public void Draw(SerializedProperty elem)
        {
            EditorGUILayout.LabelField("Attribute Modifier", EditorStyles.boldLabel);

            // Attribute / ModifierType
            EditorGUILayout.PropertyField(elem.FindPropertyRelative("attributeType"), new GUIContent("Attribute"));
            EditorGUILayout.PropertyField(elem.FindPropertyRelative("modifierType"), new GUIContent("Modifier Type"));

            // ---------- 单值/通用 ----------
            var perLevelProp = elem.FindPropertyRelative("perLevel");
            EditorGUILayout.PropertyField(perLevelProp, new GUIContent("Use Per-Level Values"));
            if (!perLevelProp.boolValue)
            {
                EditorGUILayout.PropertyField(elem.FindPropertyRelative("valueExpression"),
                    new GUIContent("Value Expression (e.g. 'atk*0.6+discipline*0.1')"));
                EditorGUILayout.PropertyField(elem.FindPropertyRelative("duration"), new GUIContent("Duration (turns)"));
                EditorGUILayout.PropertyField(elem.FindPropertyRelative("probability"), new GUIContent("Probability (%)"));
            }
            else
            {
                // ---------- 按等级覆盖 ----------
                DrawPerLevelStringArray(elem.FindPropertyRelative("valueExprLevels"), "Value Expression by Level");
                DrawPerLevelIntArray(elem.FindPropertyRelative("durationLevels"), "Duration by Level (turns)");
                DrawPerLevelStringArray(elem.FindPropertyRelative("probabilityLvls"), "Probability by Level (%)");
            }

            // 通用：目标与触发
            EditorGUILayout.PropertyField(elem.FindPropertyRelative("target"), new GUIContent("Target"));
            EditorGUILayout.PropertyField(elem.FindPropertyRelative("condition"), new GUIContent("Trigger Condition"));
        }

        private void DrawPerLevelStringArray(SerializedProperty arr, string label)
        {
            EnsureArraySize(arr, 4);
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            for (int i = 0; i < 4; i++)
            {
                var p = arr.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(p, new GUIContent($"L{i + 1}"));
            }
            EditorGUI.indentLevel--;
        }

        private void DrawPerLevelIntArray(SerializedProperty arr, string label)
        {
            EnsureArraySize(arr, 4);
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            for (int i = 0; i < 4; i++)
            {
                var p = arr.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(p, new GUIContent($"L{i + 1}"));
            }
            EditorGUI.indentLevel--;
        }

        private void EnsureArraySize(SerializedProperty arr, int size)
        {
            if (arr == null) return;
            while (arr.arraySize < size) arr.InsertArrayElementAtIndex(arr.arraySize);
            while (arr.arraySize > size) arr.DeleteArrayElementAtIndex(arr.arraySize - 1);
        }
    }
}
