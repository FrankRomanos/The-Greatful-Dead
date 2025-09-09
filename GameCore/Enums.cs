using System;

namespace GameCore
{



    // 属性类型（删除百分比冷却缩减，加“冷却减少触发”相关标记，支持联动）
    public enum AttributeType
    {
        // 通用核心属性（不变）
        Health,             // 当前生命值
        MaxHealth,          // 最大生命值
        Attack,             // 攻击值（技能伤害基础）
        Speed,              // 战斗速度（影响动作时间）
        MoveSpeed,          // 移动速度（格子/秒）
        Armor,              // 护甲（物理减伤）

        // 能量系统（不变）
        MaxEnergy,          // 能量上限
        EnergyRegenPerSec,  // 每秒能量回复

        // 【修改】冷却相关：仅保留“触发式冷却减少”的辅助属性（无百分比）
        CoolingReductionOnSkillUse, // 使用技能后减冷却秒数（装备/天赋可加）

        // 玩家专属（不变）
        Strengt,
        Agility,
        Stamina,            // 耐力（提高生命）
        Mastery,             // 精通
        CriticalChance,     // 暴击率（%）
        ToughnessReductionBoost, // 削韧增强比例
        ThreatGenerationBoost, // 威胁值值增强比例（0.2=20%，装备/天赋可加）


        // BOSS专属（不变）
        BossMaxToughness,   // BOSS韧性上限
        BossCurrentToughness// BOSS当前韧性
    }

    // 【新增】持续动作状态（管理持续动作的运行/中断）
    public enum TimedSequenceState
    {
        Ready,      // 就绪（未触发）
        Running,    // 运行中（帧更新）
        Interrupted,// 已中断（停止更新）
        Completed   // 已完成（正常结束）
    }

    // 【新增】派生动作触发条件（依赖前置动作类型）
    public enum DerivedActionTrigger
    {
        AfterStandardAction, // 标准动作后触发
        AfterSkillUse,       // 使用特定技能后触发

    }

    // 其他枚举（SkillType/DamageType/EquipmentType等不变，补全之前的定义）
    public enum SkillType
    {
        Passive,    // 被动技能（自动生效）
        Active,     // 主动技能（需手动释放）
        Reaction,   // 反应技能（触发式）
        TimedSequence, // 持续动作技能（新增，对应ActionType）
        Derived     // 派生动作技能（新增，对应ActionType）
    }

    public enum DamageType { Physical, Magical, True }
    public enum EquipmentType { Weapon, Armor, Accessory }
    public enum TalentType { Passive, Active, ClassExclusive }
    public enum TalentActivateCondition 
    { 
        AlwaysActive, HealthBelowPercent, EquipSpecificItem, AfterDerivedAction 
    }
    public enum BossToughnessSource 
    { 
        PercentOfMaxHealth, FixedValue 
    }

    public enum BossTargetSelectType
    {
        ThreatBased,  // 按威胁值选择（常规攻击/技能）
        SpecialRule   // 特殊规则（单独实现，留接口）
    }

    // 通用属性加成模型（不变，支持装备/天赋联动）
    [Serializable]
    public class AttributeBonus
    {
        public AttributeType TargetAttribute; // 目标属性
        public float BonusValue;              // 加成值（如“使用技能后减2秒冷却”则值为2）
        public bool IsRatio;                  // 仅用于数值属性（如Attack），冷却减少用false
    }
}