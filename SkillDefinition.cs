using UnityEngine;
using System.Collections.Generic;

public enum SkillActionType { Standard, Reaction, Free, Derived, FullRound, Sustained, Passive, State,Mastery,N_A }
public enum SkillTargetType { Single, AOE, Self, Line, Cone, N_A }

[CreateAssetMenu(fileName = "SkillDefinition", menuName = "RPG/SkillDefinition")]
public class SkillDefinition : ScriptableObject
{
    public string skillID;
    public string skillName;
    public Sprite icon;
    public string classID;
    public string moduleID;          // 技能模组，例如 "berserker_slash"
    public string variantKey;        // 当前形态，例如 "A" "B" "C"
    public string chainNextID;       // 下一段技能ID
    public bool resetOnTurnEnd;      // 是否回合结束重置回A



    public SkillActionType actionType = SkillActionType.N_A;
    public SkillTargetType targetType = SkillTargetType.N_A;

    public int energyCost = 0;
    public int timeCostSeconds = 0;
    public int cooldownSeconds = 0;
    public int cooldownRounds = 0; // 派生字段，RecalculateDerived 会填充
    public int range = 0;
    public float threat ;
    public float shredMultiplier;    // 削韧值倍率
    public string namekey;
    public string descriptionKey;



    // 原始效果字符串列表（导入器会填充），运行时可解析为具体 SkillEffect
    public List<EffectDefinition> effects = new List<EffectDefinition>();

    // 如需要你可以扩展更多字段（数字化分级、iconPath、tooltip 等）

    // 计算派生字段（例如将秒 -> 轮数），baseTurnTime 默认为 6 秒
    public void RecalculateDerived(int baseTurnTimeSeconds = 6)
    {
        if (baseTurnTimeSeconds <= 0) baseTurnTimeSeconds = 6;
        cooldownRounds = Mathf.CeilToInt((float)cooldownSeconds / baseTurnTimeSeconds);
    }
}

