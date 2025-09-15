using TGD.Data;
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

            // 分级只用于“数值表达式”
            if (FieldVisibilityUI.Toggle(elem, EffectFieldMask.PerLevel, "Per-Level Values"))
            {
                var perLevel = elem.FindPropertyRelative("perLevel");
                EditorGUILayout.PropertyField(perLevel, new GUIContent("Use Per-Level Values"));

                if (perLevel.boolValue)
                {
                    PerLevelUI.DrawStringLevels(elem.FindPropertyRelative("valueExprLevels"), "Value Expression by Level");
                }
                else
                {
                    EditorGUILayout.PropertyField(elem.FindPropertyRelative("valueExpression"),
                        new GUIContent("Value Expression (e.g. 'p', 'atk*0.5')"));
                }
            }

            if (FieldVisibilityUI.Toggle(elem, EffectFieldMask.Condition, "Trigger Condition"))
                EditorGUILayout.PropertyField(elem.FindPropertyRelative("condition"), new GUIContent("Trigger Condition"));
        }
    }
}


