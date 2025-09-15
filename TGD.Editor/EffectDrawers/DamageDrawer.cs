using UnityEditor;
using UnityEngine;

namespace TGD.Editor
{
    public class DamageDrawer : IEffectDrawer
    {
        public void Draw(SerializedProperty elem)
        {
            EditorGUILayout.LabelField("Damage", EditorStyles.boldLabel);

            // 学派 & 暴击
            EditorGUILayout.PropertyField(elem.FindPropertyRelative("damageSchool"), new GUIContent("School"));
            EditorGUILayout.PropertyField(elem.FindPropertyRelative("canCrit"), new GUIContent("Can Crit"));

            // 单值 vs 分级
            var perLevelProp = elem.FindPropertyRelative("perLevel");
            EditorGUILayout.PropertyField(perLevelProp, new GUIContent("Use Per-Level Values"));
            if (!perLevelProp.boolValue)
            {
                EditorGUILayout.PropertyField(elem.FindPropertyRelative("valueExpression"),
                    new GUIContent("Value Expression (e.g. 'atk*0.6+discipline*0.1')"));
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

            // 通用：目标 & 触发
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
