using UnityEditor;
using UnityEngine;
using TGD.Data;

namespace TGD.Editor
{
    public class HealDrawer : IEffectDrawer
    {
        public void Draw(SerializedProperty elem)
        {
            EditorGUILayout.LabelField("Heal", EditorStyles.boldLabel);

            if (FieldVisibilityUI.Toggle(elem, EffectFieldMask.Crit, "Critical"))
                EditorGUILayout.PropertyField(elem.FindPropertyRelative("canCrit"), new GUIContent("Can Crit"));

            if (FieldVisibilityUI.Toggle(elem, EffectFieldMask.PerLevel, "Per-Level Values"))
            {
                var perLevel = elem.FindPropertyRelative("perLevel");
                EditorGUILayout.PropertyField(perLevel, new GUIContent("Use Per-Level Values"));

                if (perLevel.boolValue)
                {
                    PerLevelUI.DrawStringLevels(elem.FindPropertyRelative("valueExprLevels"), "Value Expression by Level");
                    if (FieldVisibilityUI.Toggle(elem, EffectFieldMask.Probability, "Probability"))
                        PerLevelUI.DrawStringLevels(elem.FindPropertyRelative("probabilityLvls"), "Probability by Level (%)");
                }
                else
                {
                    EditorGUILayout.PropertyField(elem.FindPropertyRelative("valueExpression"),
                        new GUIContent("Value Expression (e.g. 'atk*0.6')"));

                    if (FieldVisibilityUI.Toggle(elem, EffectFieldMask.Probability, "Probability"))
                        EditorGUILayout.PropertyField(elem.FindPropertyRelative("probability"), new GUIContent("Probability (%)"));
                }
            }

            if (FieldVisibilityUI.Toggle(elem, EffectFieldMask.Target, "Target"))
                EditorGUILayout.PropertyField(elem.FindPropertyRelative("target"), new GUIContent("Target"));

            if (FieldVisibilityUI.Toggle(elem, EffectFieldMask.Condition, "Trigger Condition"))
                EditorGUILayout.PropertyField(elem.FindPropertyRelative("condition"), new GUIContent("Trigger Condition"));
        }
    }
}

