using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EffectDefinition
{
    public EffectType effectType;

    // 常见数值
    public float value;
    public float duration;
    public TargetType target;

    // Buff / Debuff 专用
    public string statusID;   // Buff/Debuff 的状态 ID（比如 "sk011"）

    // 技能依赖
    public string requiredSkillID;  // 条件触发需要依赖的技能 ID

    // Cooldown
    public float cooldownChangeSeconds;
    public float cooldownSetSeconds;

    // TimeCost
    public float timeCostSetSeconds;
    public float timeCostChangeSeconds;
    public bool applyToNextUseOnly;

    // 额外参数
    public string probability;
    public string extra;
    public EffectCondition condition;

    public ResourceType gainResourceType; // NEW

    // ConditionalEffect 专用
    public ResourceType resourceType;
    public CompareOp compareOp;
    public float compareValue;

    // 子效果（仅 ConditionalEffect 使用）
    public List<EffectDefinition> onSuccess = new List<EffectDefinition>();
}

public enum TargetType { Self, Enemy, Allies, All }

public enum EffectType
{
    Damage,
    Heal,
    Buff,
    Debuff,
    GainResource,
    ReduceCooldown,
    ReplaceSkill,
    ResetCooldown,
    SetSkillTimeCost,
    ModifySkillTimeCost,
    ConditionalEffect
}

public enum EffectCondition
{
    None,
    AfterAttack,
    OnCriticalHit,
    OnCooldownEnd,
    AfterSkillUse,
    SkillStateActive
}

public enum ResourceType
{
    HP,
    Energy,
    Discipline,
    Iron,
    Rage,
    Versatility,
    Gunpowder,
    point,
    combo,
    punch,
    qi,
    vision
}

public enum CompareOp
{
    Equal,
    Greater,
    GreaterEqual,
    Less,
    LessEqual
}
