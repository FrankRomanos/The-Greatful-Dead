using UnityEditor;
using UnityEngine;
using TGD.Data;

namespace TGD.Editor
{
    /// <summary>
    /// Attribute Modifier 的可视化 Drawer（支持可见性开关 + 分级 L1~L4）
    /// 依赖：FieldVisibilityUI、PerLevelUI、EffectFieldMask
    /// </summary>
    public class AttributeModifierDrawer : IEffectDrawer
    {
        public void Draw(SerializedProperty elem)
        {
            EditorGUILayout.LabelField("Attribute Modifier", EditorStyles.boldLabel);

            // 基本字段（始终显示）
            EditorGUILayout.PropertyField(elem.FindPropertyRelative("attributeType"), new GUIContent("Attribute"));
            EditorGUILayout.PropertyField(elem.FindPropertyRelative("modifierType"), new GUIContent("Modifier Type"));

            // —— 分级开关（可隐藏）——
            if (FieldVisibilityUI.Toggle(elem, EffectFieldMask.PerLevel, "Per-Level Values"))
            {
                var perLevel = elem.FindPropertyRelative("perLevel");
                EditorGUILayout.PropertyField(perLevel, new GUIContent("Use Per-Level Values"));

                if (perLevel.boolValue)
                {
                    // L1~L4：数值表达式 / 持续 / 概率
                    PerLevelUI.DrawStringLevels(elem.FindPropertyRelative("valueExprLevels"), "Value Expression by Level");
                    if (FieldVisibilityUI.Toggle(elem, EffectFieldMask.Duration, "Duration"))
                        PerLevelUI.DrawIntLevels(elem.FindPropertyRelative("durationLevels"), "Duration by Level (turns)");
                    if (FieldVisibilityUI.Toggle(elem, EffectFieldMask.Probability, "Probability"))
                        PerLevelUI.DrawStringLevels(elem.FindPropertyRelative("probabilityLvls"), "Probability by Level (%)");
                }
                else
                {
                    // 单值：表达式 + 可选持续/概率
                    EditorGUILayout.PropertyField(
                        elem.FindPropertyRelative("valueExpression"),
                        new GUIContent("Value Expression (e.g. '10', 'p', 'atk*0.5')")
                    );

                    if (FieldVisibilityUI.Toggle(elem, EffectFieldMask.Duration, "Duration"))
                        EditorGUILayout.PropertyField(elem.FindPropertyRelative("duration"), new GUIContent("Duration (turns)"));

                    if (FieldVisibilityUI.Toggle(elem, EffectFieldMask.Probability, "Probability"))
                        EditorGUILayout.PropertyField(elem.FindPropertyRelative("probability"), new GUIContent("Probability (%)"));
                }
            }

            // —— 目标 / 触发（可隐藏）——
            if (FieldVisibilityUI.Toggle(elem, EffectFieldMask.Target, "Target"))
                EditorGUILayout.PropertyField(elem.FindPropertyRelative("target"), new GUIContent("Target"));

            if (FieldVisibilityUI.Toggle(elem, EffectFieldMask.Condition, "Trigger Condition"))
                EditorGUILayout.PropertyField(elem.FindPropertyRelative("condition"), new GUIContent("Trigger Condition"));
        }
    }
}
