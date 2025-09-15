using UnityEditor;
using UnityEngine;
using TGD.Data;

namespace TGD.Editor
{
    public class DamageDrawer : IEffectDrawer
    {
        public void Draw(SerializedProperty elem)
        {
            EditorGUILayout.LabelField("Damage", EditorStyles.boldLabel);

            // 学派/暴击：可隐藏
            if (FieldVisibilityUI.Toggle(elem, EffectFieldMask.School, "Damage School"))
                EditorGUILayout.PropertyField(elem.FindPropertyRelative("damageSchool"), new GUIContent("School"));

            if (FieldVisibilityUI.Toggle(elem, EffectFieldMask.Crit, "Critical"))
                EditorGUILayout.PropertyField(elem.FindPropertyRelative("canCrit"), new GUIContent("Can Crit"));

            // 分级开关：可隐藏
            if (FieldVisibilityUI.Toggle(elem, EffectFieldMask.PerLevel, "Per-Level Values"))
            {
                var perLevel = elem.FindPropertyRelative("perLevel");
                EditorGUILayout.PropertyField(perLevel, new GUIContent("Use Per-Level Values"));

                if (perLevel.boolValue)
                {
                    PerLevelUI.DrawStringLevels(elem.FindPropertyRelative("valueExprLevels"), "Value Expression by Level");
                    // 可选：分级概率
                    if (FieldVisibilityUI.Toggle(elem, EffectFieldMask.Probability, "Probability"))
                        PerLevelUI.DrawStringLevels(elem.FindPropertyRelative("probabilityLvls"), "Probability by Level (%)");
                }
                else
                {
                    // 单值
                    EditorGUILayout.PropertyField(elem.FindPropertyRelative("valueExpression"),
                        new GUIContent("Value Expression (e.g. 'atk*0.6+discipline*0.1')"));

                    if (FieldVisibilityUI.Toggle(elem, EffectFieldMask.Probability, "Probability"))
                        EditorGUILayout.PropertyField(elem.FindPropertyRelative("probability"), new GUIContent("Probability (%)"));
                }
            }

            // 目标/触发：可隐藏
            if (FieldVisibilityUI.Toggle(elem, EffectFieldMask.Target, "Target"))
                EditorGUILayout.PropertyField(elem.FindPropertyRelative("target"), new GUIContent("Target"));

            if (FieldVisibilityUI.Toggle(elem, EffectFieldMask.Condition, "Trigger Condition"))
                EditorGUILayout.PropertyField(elem.FindPropertyRelative("condition"), new GUIContent("Trigger Condition"));
        }
    }
}

