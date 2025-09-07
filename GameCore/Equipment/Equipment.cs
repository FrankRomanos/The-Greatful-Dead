using GameCore;
using GameCore.Action;
using UnityEngine;

namespace GameCore.Skill
{
    [CreateAssetMenu(fileName = "NewSkill", menuName = "GameCore/Skill")]
    public class Skill : ScriptableObject
    {
        [Header("基础信息（补全动作类型关联）")]
        public string SkillName;
        public string SkillID;
        public SkillType SkillType; // 关联ActionType：TimedSequence/Derived
        public ActionType ActionType;
        public DamageType DamageType;
        public Sprite SkillIcon;

        [Header("消耗与冷却（【修改】仅固定冷却，支持减秒数）")]
        public float EnergyCost;          // 能量消耗（派生动作可设0）
        public float BaseCoolingTime;     // 基础冷却时间（秒）
        public float CurrentCoolingTime;  // 当前冷却（实时更新）
        public bool IgnoreCoolingOnDerived; // 派生动作是否忽略冷却

        [Header("【新增】持续动作配置（仅SkillType=TimedSequence生效）")]
        public TimedSequenceData TimedSequence;

        [Header("【新增】派生动作配置（仅SkillType=Derived生效）")]
        public Skill RequiredPreSkill;    // 依赖的前置技能
        public float DerivedActionTime;   // 派生动作耗时（比标准动作短）

        [Header("伤害与效果（不变）")]
        public float DamageRatio;         // 伤害倍率
        public float ToughnessReductionRatio; // 削韧倍率
        public float ThreatGenerationRatio;   // 威胁值倍率


        // 技能是否可释放（补全持续/派生动作判断）
        public bool CanUse(BattleUnit caster)
        {
            // 冷却判断（派生动作可忽略）
            if (!IgnoreCoolingOnDerived && CurrentCoolingTime > 0) return false;

            // 能量判断（派生动作可设0）
            if (caster.CurrentEnergy < EnergyCost) return false;

            // 派生动作：需先释放前置技能
            if (SkillType == SkillType.Derived && !caster.HasUsedSkill(RequiredPreSkill))
            {
                Debug.Log($"需先释放前置技能：{RequiredPreSkill.SkillName}");
                return false;
            }

            // 持续动作：不能同时运行多个
            if (SkillType == SkillType.TimedSequence && caster.IsTimedSequenceRunning())
            {
                Debug.Log("已有持续动作运行中，无法触发新持续动作");
                return false;
            }

            return true;
        }

        // 【修改】冷却减少（仅减指定秒数，无百分比）
        public void ReduceCoolingTime(float reduceSeconds)
        {
            if (reduceSeconds <= 0) return;
            CurrentCoolingTime = Mathf.Max(0, CurrentCoolingTime - reduceSeconds);
            Debug.Log($"{SkillName} 冷却减少 {reduceSeconds} 秒，剩余：{CurrentCoolingTime:F1}秒");
        }

        // 【补全】释放技能（分类型处理：普通/持续/派生）
        public virtual bool Use(BattleUnit caster, BattleUnit target = null)
        {
            if (!CanUse(caster) || target == null) return false;

            // 消耗能量（派生动作可设0）
            if (!caster.ConsumeEnergy(EnergyCost)) return false;

            // 分技能类型处理
            switch (SkillType)
            {
                case SkillType.Active:
                    // 普通主动技能（原逻辑）
                    ExecuteActiveSkill(caster, target);
                    break;
                case SkillType.TimedSequence:
                    // 持续动作：启动持续更新
                    caster.StartTimedSequence(this.TimedSequence, this, target);
                    break;
                case SkillType.Derived:
                    // 派生动作：耗时短，不触发主回合结束
                    ExecuteDerivedSkill(caster, target);
                    break;
            }

            // 启动冷却（派生动作可忽略）
            if (!IgnoreCoolingOnDerived)
            {
                CurrentCoolingTime = BaseCoolingTime;
            }

            // 触发装备/天赋的技能后特效（如减冷却）
            caster.TriggerPostSkillEffects(this);
            return true;
        }

        // 普通主动技能执行（原逻辑不变）
        protected virtual void ExecuteActiveSkill(BattleUnit caster, BattleUnit target)
        {
            float damage = CalculateDamage(caster);
            target.TakeDamage(damage, DamageType);
            caster.ApplyToughnessAndThreat(damage, this, target);
            Debug.Log($"{caster.UnitName} 使用 {SkillName}，造成 {damage:F1} 伤害");
        }

        // 【新增】派生动作执行（耗时短，标记前置技能）
        protected virtual void ExecuteDerivedSkill(BattleUnit caster, BattleUnit target)
        {
            float damage = CalculateDamage(caster) * 0.8f; // 派生动作伤害略低
            target.TakeDamage(damage, DamageType);
            caster.ApplyToughnessAndThreat(damage, this, target);
            caster.MarkSkillAsUsed(RequiredPreSkill); // 标记前置技能已使用
            Debug.Log($"{caster.UnitName} 触发派生动作 {SkillName}，耗时 {DerivedActionTime:F1}秒，伤害 {damage:F1}");
        }

        // 【新增】持续动作帧更新（由Unit调用）
        public virtual void UpdateTimedSequence(BattleUnit caster, BattleUnit target, float deltaTime)
        {
            var sequence = this.TimedSequence;
            if (sequence.State != TimedSequenceState.Running) return;

            // 累计时间
            sequence.ElapsedTime += deltaTime;
            sequence.LastIntervalTime += deltaTime;

            // 检查是否结束
            if (sequence.ElapsedTime >= sequence.TotalDuration)
            {
                sequence.State = TimedSequenceState.Completed;
                sequence.OnSequenceEnd?.Invoke();
                Debug.Log($"{caster.UnitName} 持续动作 {SkillName} 完成，总耗时 {sequence.ElapsedTime:F1}秒");
                return;
            }

            // 按间隔触发伤害/效果
            if (sequence.LastIntervalTime >= sequence.Interval)
            {
                // 间隔伤害
                float intervalDamage = sequence.DamagePerInterval;
                target.TakeDamage(intervalDamage, DamageType);
                // 间隔能量消耗
                caster.ConsumeEnergy(sequence.EnergyCostPerSecond * sequence.Interval);
                // 回调更新
                sequence.OnSequenceUpdate?.Invoke();
                // 重置间隔计时
                sequence.LastIntervalTime = 0f;
                Debug.Log($"{caster.UnitName} 持续动作 {SkillName} 间隔触发，伤害 {intervalDamage:F1}");
            }
        }

        // 【新增】中断持续动作
        public virtual void InterruptTimedSequence(BattleUnit caster)
        {
            var sequence = this.TimedSequence;
            if (sequence.State != TimedSequenceState.Running) return;

            sequence.State = TimedSequenceState.Interrupted;
            sequence.OnSequenceInterrupt?.Invoke();
            Debug.Log($"{caster.UnitName} 持续动作 {SkillName} 被中断");
        }

        // 伤害计算（不变）
        protected virtual float CalculateDamage(BattleUnit caster)
        {
            return caster.Attributes.TryGetValue(AttributeType.Attack, out float attack)
                ? attack * DamageRatio
                : 0f;
        }

        // 冷却更新（仅减时间，不变）
        public void UpdateCooling(float deltaTime)
        {
            if (CurrentCoolingTime > 0)
            {
                CurrentCoolingTime = Mathf.Max(0, CurrentCoolingTime - deltaTime);
            }
        }
    }
}
