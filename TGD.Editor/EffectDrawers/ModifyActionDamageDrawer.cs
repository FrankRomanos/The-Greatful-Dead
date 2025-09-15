using UnityEditor;
using UnityEngine;

namespace TGD.Editor
{
    public class ModifyActionDamageDrawer : IEffectDrawer
    {
        public void Draw(SerializedProperty elem)
        {
            EditorGUILayout.LabelField("Modify Action Damage", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(elem.FindPropertyRelative("targetActionType"), new GUIContent("Action Type"));
            EditorGUILayout.PropertyField(elem.FindPropertyRelative("modifierType"), new GUIContent("Modifier Type"));

            var perLevelProp = elem.FindPropertyRelative("perLevel");
            EditorGUILayout.PropertyField(perLevelProp, new GUIContent("Use Per-Level Values"));
            if (!perLevelProp.boolValue)
            {
                EditorGUILayout.PropertyField(elem.FindPropertyRelative("valueExpression"),
                    new GUIContent("Value Expression (e.g. 'p', 'atk*0.5')"));
            }
            else
            {
                EnsureArraySize(elem.FindPropertyRelative("valueExprLevels"), 4);
                var arr = elem.FindPropertyRelative("valueExprLevels");
                EditorGUILayout.LabelField("Value Expression by Level", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                for (int i = 0; i < 4; i++)
                    EditorGUILayout.PropertyField(arr.GetArrayElementAtIndex(i), new GUIContent($"L{i + 1}"));
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

