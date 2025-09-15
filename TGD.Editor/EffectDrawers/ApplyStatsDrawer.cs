using TGD.Data;
using UnityEditor;
using UnityEngine;

namespace TGD.Editor
{
    public class ApplyStatusDrawer : IEffectDrawer
    {
        public void Draw(SerializedProperty elem)
        {
            EditorGUILayout.LabelField("Apply Status (Buff/Debuff)", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(elem.FindPropertyRelative("statusSkillID"), new GUIContent("Status Skill ID"));

            // 分级：用于持续/概率
            if (FieldVisibilityUI.Toggle(elem, EffectFieldMask.PerLevel, "Per-Level Values"))
            {
                var perLevel = elem.FindPropertyRelative("perLevel");
                EditorGUILayout.PropertyField(perLevel, new GUIContent("Use Per-Level Values"));

                if (perLevel.boolValue)
                {
                    if (FieldVisibilityUI.Toggle(elem, EffectFieldMask.Duration, "Duration"))
                        PerLevelUI.DrawIntLevels(elem.FindPropertyRelative("durationLevels"), "Duration by Level (turns)");

                    if (FieldVisibilityUI.Toggle(elem, EffectFieldMask.Probability, "Probability"))
                        PerLevelUI.DrawStringLevels(elem.FindPropertyRelative("probabilityLvls"), "Probability by Level (%)");
                }
                else
                {
                    if (FieldVisibilityUI.Toggle(elem, EffectFieldMask.Duration, "Duration"))
                        EditorGUILayout.PropertyField(elem.FindPropertyRelative("duration"), new GUIContent("Duration (turns)"));

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
