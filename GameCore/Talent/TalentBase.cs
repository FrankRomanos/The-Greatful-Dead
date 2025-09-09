using GameCore;
using GameCore.Skill;
using UnityEngine;

namespace GameCore.Talent
{
    // 天赋基类，统一所有天赋的接口
    public abstract class TalentBase : ScriptableObject
    {
        public string TalentName;
        public string TalentID;
        public TalentType TalentType; // 引用Enums.cs中的TalentType
        public TalentActivateCondition ActivateCondition; // 引用Enums.cs中的激活条件
        public float ConditionParam; // 条件参数（如血量百分比阈值）

        public virtual void OnSkillUsedAfter(Unit owner, SkillBase usedSkill)
        {
            // 基类默认空实现，子类需要时重写
            // 示例：子类（如“嗜血天赋”）可以重写这里，实现“释放技能后回血”
        }

        // 激活天赋
        public abstract bool Activate(Unit caster);
        // 禁用天赋
        public abstract void Deactivate(Unit caster);
        // 检查是否满足激活条件
        public abstract bool CheckActivateCondition(Unit caster);
    }
}
