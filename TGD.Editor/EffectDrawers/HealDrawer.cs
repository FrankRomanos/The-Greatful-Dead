using UnityEditor;
using UnityEngine;

namespace TGD.Editor
{
    public class HealDrawer : IEffectDrawer
    {
        public void Draw(SerializedProperty elem)
        {
            EditorGUILayout.LabelField("Heal", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(elem.FindPropertyRelative("canCrit"), new GUIContent("Can Crit"));

            var perLevelProp = elem.FindPropertyRelative("perLevel");
            EditorGUILayout.PropertyField(perLevelProp, new GUIContent("Use Per-Level Values"));
            if (!perLevelProp.boolValue)
            {
                EditorGUILayout.PropertyField(elem.FindPropertyRelative("valueExpression"),
                    new GUIContent("Value Expression (e.g. 'atk*0.6')"));
                EditorGUILayout.PropertyField(elem.FindPropertyRelative("probability"), new GUIContent("Probability (%)"));
            }
            else
            {
                EnsureSize(elem.FindPropertyRelative("valueExprLevels"), 4);
                EnsureSize(elem.FindPropertyRelative("probabilityLvls"), 4);

                var val = elem.FindPropertyRelative("valueExprLevels");
                var prb = elem.FindPropertyRelative("probabilityLvls");

                EditorGUILayout.LabelField("Value Expression by Level", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                for (int i = 0; i < 4; i++)
                    EditorGUILayout.PropertyField(val.GetArrayElementAtIndex(i), new GUIContent($"L{i + 1}"));
                EditorGUI.indentLevel--;

                EditorGUILayout.LabelField("Probability by Level (%)", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                for (int i = 0; i < 4; i++)
                    EditorGUILayout.PropertyField(prb.GetArrayElementAtIndex(i), new GUIContent($"L{i + 1}"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(elem.FindPropertyRelative("target"), new GUIContent("Target"));
            EditorGUILayout.PropertyField(elem.FindPropertyRelative("condition"), new GUIContent("Trigger Condition"));
        }

        private void EnsureSize(SerializedProperty arr, int n)
        {
            while (arr.arraySize < n) arr.InsertArrayElementAtIndex(arr.arraySize);
            while (arr.arraySize > n) arr.DeleteArrayElementAtIndex(arr.arraySize - 1);
        }
    }
}
