using UnityEditor;
using UnityEngine;

namespace TGD.Editor
{
    /// <summary>公用的 L1~L4 绘制工具</summary>
    public static class PerLevelUI
    {
        public static void DrawStringLevels(SerializedProperty arr, string label)
        {
            FieldVisibilityUI.EnsureSize(arr, 4);
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            for (int i = 0; i < 4; i++)
                EditorGUILayout.PropertyField(arr.GetArrayElementAtIndex(i), new GUIContent($"L{i + 1}"));
            EditorGUI.indentLevel--;
        }

        public static void DrawIntLevels(SerializedProperty arr, string label)
        {
            FieldVisibilityUI.EnsureSize(arr, 4);
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            for (int i = 0; i < 4; i++)
                EditorGUILayout.PropertyField(arr.GetArrayElementAtIndex(i), new GUIContent($"L{i + 1}"));
            EditorGUI.indentLevel--;
        }
    }
}
