using GameCore.Skill;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Equipment
{
    [CreateAssetMenu(fileName = "NewEquipment", menuName = "GameCore/EquipmentBase")]
    public class EquipmentBase : ScriptableObject
    {
        public string EquipName;
        public string EquipID;
        public EquipmentType EquipType; // 引用Enums.cs的EquipmentType
        public List<AttributeModifier> AttributeModifiers; // 装备属性加成
        public SkillBase UnlockSkill; // 装备解锁的技能
        public virtual void OnSkillUsedAfter(Unit owner, SkillBase usedSkill)
        {
            // 基类默认空实现（没有特效），子类需要时重写
            // 示例：子类（如“冷却却戒指”）可以重写这里，实现“释放技能后减冷却”
        }
    }

    // 装备属性修改器（辅助类）
    [System.Serializable]
    public class AttributeModifier
    {
        public AttributeType AttributeType;
        public float ModifyValue; // 数值加成
        public bool IsPercent; // 是否为百分比加成
    }


}