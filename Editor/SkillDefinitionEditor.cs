using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SkillDefinition))]
public class SkillDefinitionEditor : Editor
{
    private SerializedProperty effectsProp;

    private void OnEnable()
    {
        effectsProp = serializedObject.FindProperty("effects");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // 默认 Inspector（排除 effects）
        DrawPropertiesExcluding(serializedObject, "m_Script", "effects");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Effects", EditorStyles.boldLabel);

        if (effectsProp == null)
        {
            EditorGUILayout.HelpBox("No 'effects' property found on SkillDefinition.", MessageType.Error);
            serializedObject.ApplyModifiedProperties();
            return;
        }

        for (int i = 0; i < effectsProp.arraySize; i++)
        {
            var element = effectsProp.GetArrayElementAtIndex(i);
            EditorGUILayout.BeginVertical("box");

            // 1) 显示 EffectType
            var effectTypeProp = element.FindPropertyRelative("effectType");
            EditorGUILayout.PropertyField(effectTypeProp, new GUIContent("Effect Type"));
            EffectType effectType = (EffectType)effectTypeProp.enumValueIndex;

            // 2) ConditionalEffect: ONLY show onSuccess AND condition fields (and nothing else)
            if (effectType == EffectType.ConditionalEffect)
            {
                // onSuccess 列表（先展示要做什么）
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("On Success Effects", EditorStyles.boldLabel);
                DrawOnSuccessList(element);

                // 然后是条件定义（Condition）
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Condition", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(element.FindPropertyRelative("resourceType"), new GUIContent("Resource Type"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("compareOp"), new GUIContent("Compare Op"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("compareValue"), new GUIContent("Compare Value"));

                if (GUILayout.Button("Remove Effect"))
                {
                    effectsProp.DeleteArrayElementAtIndex(i);
                    serializedObject.ApplyModifiedProperties();
                }

                EditorGUILayout.EndVertical();
                continue; // skip further drawing for ConditionalEffect
            }

            // 3) GainResource: show gainResourceType immediately, hide cooldown/timecost fields
            if (effectType == EffectType.GainResource)
            {
                // 将 gainResourceType 放在 EffectType 之后
                var gainResProp = element.FindPropertyRelative("gainResourceType");
                if (gainResProp != null)
                    EditorGUILayout.PropertyField(gainResProp, new GUIContent("Resource Type"));

                // 然后绘制其余字段 BUT skip cooldown/timeCost & skip conditional-only fields (resourceType for condition)
                var fields = typeof(EffectDefinition).GetFields(BindingFlags.Public | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    // Skip internal control fields
                    if (field.Name == "effectType" || field.Name == "onSuccess" || field.Name == "gainResourceType")
                        continue;

                    // Hide condition-specific resourceType/compare fields from non-ConditionalEffect
                    if (field.Name == "resourceType" || field.Name == "compareOp" || field.Name == "compareValue")
                        continue;

                    // Hide cooldown/time cost related fields for GainResource
                    if (field.Name.IndexOf("cooldown", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        field.Name.IndexOf("timeCost", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        field.Name == "applyToNextUseOnly")
                        continue;

                    var prop = element.FindPropertyRelative(field.Name);
                    if (prop != null)
                        EditorGUILayout.PropertyField(prop, new GUIContent(ObjectNames.NicifyVariableName(field.Name)), true);
                }

                if (GUILayout.Button("Remove Effect"))
                {
                    effectsProp.DeleteArrayElementAtIndex(i);
                    serializedObject.ApplyModifiedProperties();
                }

                EditorGUILayout.EndVertical();
                continue;
            }

            // 4) Default: other EffectTypes -> 全开（除非你用 EffectFieldAttribute 限制）
            DrawEffectFieldsForSerializedElement(element);

            if (GUILayout.Button("Remove Effect"))
            {
                effectsProp.DeleteArrayElementAtIndex(i);
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.EndVertical();
        }

        // Add new top-level effect 按钮
        if (GUILayout.Button("Add Effect"))
        {
            effectsProp.InsertArrayElementAtIndex(effectsProp.arraySize);
            var newElem = effectsProp.GetArrayElementAtIndex(effectsProp.arraySize - 1);
            InitializeNewEffectElement(newElem);
            serializedObject.ApplyModifiedProperties();
        }

        serializedObject.ApplyModifiedProperties();
    }

    // 绘制并管理 onSuccess 列表（包含 Add / Clear / Remove 单项，并初始化新子项）
    private void DrawOnSuccessList(SerializedProperty parentEffect)
    {
        var onSuccessProp = parentEffect.FindPropertyRelative("onSuccess");
        if (onSuccessProp == null)
        {
            EditorGUILayout.HelpBox("onSuccess property not found or not serializable.", MessageType.Warning);
            return;
        }

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Count: {onSuccessProp.arraySize}", GUILayout.Width(80));
        if (GUILayout.Button("Add Success Effect"))
        {
            onSuccessProp.arraySize++;
            serializedObject.ApplyModifiedProperties();
            var added = onSuccessProp.GetArrayElementAtIndex(onSuccessProp.arraySize - 1);
            InitializeNewEffectElement(added);
            serializedObject.ApplyModifiedProperties();
        }
        if (onSuccessProp.arraySize > 0)
        {
            if (GUILayout.Button("Clear All"))
            {
                onSuccessProp.arraySize = 0;
                serializedObject.ApplyModifiedProperties();
            }
        }
        EditorGUILayout.EndHorizontal();

        // 列表项逐个渲染（折叠 + remove）
        for (int j = 0; j < onSuccessProp.arraySize; j++)
        {
            var subElement = onSuccessProp.GetArrayElementAtIndex(j);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            subElement.isExpanded = EditorGUILayout.Foldout(subElement.isExpanded, $"Success Effect {j + 1}", true);
            if (GUILayout.Button("Remove", GUILayout.Width(60)))
            {
                onSuccessProp.DeleteArrayElementAtIndex(j);
                serializedObject.ApplyModifiedProperties();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                break; // array changed, break to avoid indexing issues
            }
            EditorGUILayout.EndHorizontal();

            if (subElement.isExpanded)
            {
                // 子效果绘制：使用默认的 field 绘制逻辑（子效果可以看到所有普通字段）
                DrawEffectFieldsForSerializedElement(subElement);
            }

            EditorGUILayout.EndVertical();
        }
    }

    // 初始化新添加的 EffectDefinition SerializedProperty（默认值，避免空元素）
    private void InitializeNewEffectElement(SerializedProperty elem)
    {
        if (elem == null) return;

        var typeProp = elem.FindPropertyRelative("effectType");
        if (typeProp != null) typeProp.enumValueIndex = (int)EffectType.Debuff; // 默认给 Debuff（可改）

        var fields = typeof(EffectDefinition).GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var f in fields)
        {
            if (f.Name == "effectType" || f.Name == "onSuccess") continue;
            var p = elem.FindPropertyRelative(f.Name);
            if (p == null) continue;

            switch (p.propertyType)
            {
                case SerializedPropertyType.String: p.stringValue = ""; break;
                case SerializedPropertyType.Float: p.floatValue = 0f; break;
                case SerializedPropertyType.Integer: p.intValue = 0; break;
                case SerializedPropertyType.Boolean: p.boolValue = false; break;
                case SerializedPropertyType.Enum: p.enumValueIndex = 0; break;
                case SerializedPropertyType.ObjectReference: p.objectReferenceValue = null; break;
                case SerializedPropertyType.Generic:
                    if (p.isArray) p.arraySize = 0;
                    break;
            }
        }
    }

    // 默认绘制方法：遵循 EffectFieldAttribute（若无 attribute 则默认显示）
    private void DrawEffectFieldsForSerializedElement(SerializedProperty element)
    {
        var fields = typeof(EffectDefinition).GetFields(BindingFlags.Public | BindingFlags.Instance);

        var effectTypeProp = element.FindPropertyRelative("effectType");
        EffectType currentEffectType = (effectTypeProp != null) ? (EffectType)effectTypeProp.enumValueIndex : EffectType.Damage;

        foreach (var field in fields)
        {
            if (field.Name == "effectType" || field.Name == "onSuccess") continue;

            // 若字段被标注为仅在特定 EffectType 下显示，则检查
            var attr = (EffectFieldAttribute)field.GetCustomAttribute(typeof(EffectFieldAttribute), false);
            if (attr != null && (attr.EffectTypes == null || !attr.EffectTypes.Contains(currentEffectType)))
                continue;

            // 额外：确保 ConditionalEffect 专用字段不会出现在非 ConditionalEffect（已在上层处理，但这里再保险）
            if ((field.Name == "resourceType" || field.Name == "compareOp" || field.Name == "compareValue") &&
                currentEffectType != EffectType.ConditionalEffect)
            {
                continue;
            }

            var prop = element.FindPropertyRelative(field.Name);
            if (prop != null)
            {
                EditorGUILayout.PropertyField(prop, new GUIContent(ObjectNames.NicifyVariableName(field.Name)), true);
            }
        }
    }
}

