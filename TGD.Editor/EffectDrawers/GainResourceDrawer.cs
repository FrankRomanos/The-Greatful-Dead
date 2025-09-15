using TGD.Data;
using UnityEditor;
using UnityEngine;

namespace TGD.Editor
{
    public class GainResourceDrawer : IEffectDrawer
    {
        public void Draw(SerializedProperty elem)
        {
            EditorGUILayout.LabelField("Gain Resource", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(elem.FindPropertyRelative("resourceType"), new GUIContent("Resource Type"));

            // 分级用于：数值/概率
            if (FieldVisibilityUI.Toggle(elem, EffectFieldMask.PerLevel, "Per-Level Values"))
            {
                var perLevel = elem.FindPropertyRelative("perLevel");
                EditorGUILayout.PropertyField(perLevel, new GUIContent("Use Per-Level Values"));

                if (perLevel.boolValue)
                {
                    PerLevelUI.DrawStringLevels(elem.FindPropertyRelative("valueExprLevels"), "Value by Level (formula)");
                    if (FieldVisibilityUI.Toggle(elem, EffectFieldMask.Probability, "Probability"))
                        PerLevelUI.DrawStringLevels(elem.FindPropertyRelative("probabilityLvls"), "Probability by Level (%)");
                }
                else
                {
                    var valExpr = FieldVisibilityUI.GetProp(elem, "valueExpression", "value");
                    if (valExpr != null)
                        EditorGUILayout.PropertyField(valExpr, new GUIContent("Value (formula, e.g. '1', 'p', 'atk*0.1')"));
                    else
                        EditorGUILayout.HelpBox("Missing 'valueExpression' (or legacy 'value') field.", MessageType.Warning);

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





