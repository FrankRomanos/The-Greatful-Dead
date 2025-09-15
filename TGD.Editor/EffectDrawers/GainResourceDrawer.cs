using UnityEditor;
using UnityEngine;

namespace TGD.Editor
{
    public class GainResourceDrawer : DefaultEffectDrawer
    {
        public override void Draw(SerializedProperty elem)
        {
            EditorGUILayout.LabelField("Gain Resource", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(elem.FindPropertyRelative("resourceType"), new UnityEngine.GUIContent("Resource Type"));
            EditorGUILayout.PropertyField(elem.FindPropertyRelative("value"), new UnityEngine.GUIContent("Value"));
            EditorGUILayout.PropertyField(elem.FindPropertyRelative("target"), new UnityEngine.GUIContent("Target"));
            EditorGUILayout.PropertyField(elem.FindPropertyRelative("probability"), new UnityEngine.GUIContent("Probability (%)"));
            EditorGUILayout.PropertyField(elem.FindPropertyRelative("condition"), new UnityEngine.GUIContent("Trigger Condition")); var perLevelProp = elem.FindPropertyRelative("perLevel");
            EditorGUILayout.PropertyField(perLevelProp, new GUIContent("Use Per-Level Values"));
            if (!perLevelProp.boolValue)
            {
                EditorGUILayout.PropertyField(elem.FindPropertyRelative("duration"), new GUIContent("Duration (turns)"));
                EditorGUILayout.PropertyField(elem.FindPropertyRelative("probability"), new GUIContent("Probability (%)"));
            }
            else
            {
                EnsureArraySize(elem.FindPropertyRelative("durationLevels"), 4);
                EnsureArraySize(elem.FindPropertyRelative("probabilityLvls"), 4);

                var dur = elem.FindPropertyRelative("durationLevels");
                var prob = elem.FindPropertyRelative("probabilityLvls");

                EditorGUILayout.LabelField("Duration by Level (turns)", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                for (int i = 0; i < 4; i++)
                    EditorGUILayout.PropertyField(dur.GetArrayElementAtIndex(i), new GUIContent($"L{i + 1}"));
                EditorGUI.indentLevel--;

                EditorGUILayout.LabelField("Probability by Level (%)", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                for (int i = 0; i < 4; i++)
                    EditorGUILayout.PropertyField(prob.GetArrayElementAtIndex(i), new GUIContent($"L{i + 1}"));
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.PropertyField(elem.FindPropertyRelative("condition"), new GUIContent("Trigger Condition"));
        }

        private void EnsureArraySize(SerializedProperty arr, int size)
        {
            if (arr == null) return;
            while (arr.arraySize < size) arr.InsertArrayElementAtIndex(arr.arraySize);
            while (arr.arraySize > size) arr.DeleteArrayElementAtIndex(arr.arraySize - 1);
        }
    }
}
    


    

