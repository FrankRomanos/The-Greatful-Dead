using GameCore;
using GameCore.TurnAction;
using GameCore.Equipment;
using GameCore.Skill;
using GameCore.Talent;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI; // 新增：用于血条UI

namespace GameCore
{
    public class Unit : MonoBehaviour
    {
        [Header("===== 原有基础信息（原有） =====")]
        public string UnitName;
        public bool IsPlayer;
        public bool IsBoss;
        public int UnitLevel = 1;

        [Header("===== 新增：移动相关 =====")]
        public float MoveSpeed => Attributes.TryGetValue(AttributeType.MoveSpeed, out var val) ? val : 3f; // 关联移动速度属性
        public Rigidbody Rb; // 移动用刚体
        public bool CanMove = true; // 是否允许移动（比如持续动作中不能动）

        [Header("===== 新增：血条UI相关 =====")]
        public Slider HpSlider; // 血条滑块（拖入Unity面板的Slider组件）
        public Text HpText; // 血条文本（显示“当前HP/最大HP”）
        public Vector3 HpSliderOffset = new(0, 2, 0); // 血条相对于角色的偏移（避免挡住模型）

        [Header("===== 原有：装备/天赋/技能系统 =====")]
        public List<EquipmentBase> EquippedItems = new List<EquipmentBase>();
        public List<TalentBase> LearnedTalents = new List<TalentBase>();
        public List<SkillBase> Skills { get; private set; } = new List<SkillBase>();
        private List<(SkillBase Skill, TimedSequenceData Sequence, Unit Target)> _runningTimedSequences = new();

        [Header("===== 原有：BOSS韧性性/威胁值系统 =====")]
        public bool BossHasToughness = true;
        public BossToughnessSource ToughnessSource = BossToughnessSource.PercentOfMaxHealth;
        public float ToughnessPercent = 0.2f;
        public float FixedToughnessValue = 300f;
        public bool IsInvulnerableToToughness = false;
        public float CurrentThreatToBoss { get; private set; } = 0f;
        private readonly Dictionary<Unit, float> _playerThreatDict = new();
        public BossTargetSelectType DefaultTargetType = BossTargetSelectType.ThreatBased;
        public Unit CurrentTarget { get; private set; }
        private float _bossTurnTimer = 0f;

        // 核心属性（原有）
        public Dictionary<AttributeType, float> Attributes { get; private set; } = new();
        public float CurrentEnergy { get; private set; }
        public float MaxEnergy => Attributes.TryGetValue(AttributeType.MaxEnergy, out var val) ? val : 200f;
        public float EnergyRegenPerSec => Attributes.TryGetValue(AttributeType.EnergyRegenPerSec, out var val) ? val : 10f;

        // 事件（新增血条/移动相关事件）
        public event Action OnEnergyChanged;
        public event Action OnToughnessBroken;
        public event Action<TalentBase, bool> OnTalentStateChanged;
        public event Action<TimedSequenceData, TimedSequenceState> OnTimedSequenceStateChanged;
        public event Action<SkillBase> OnDerivedActionTriggered;
        public event Action<float, float> OnHpChanged; // 新增：HP变化时触发（用于UI更新）
        public event Action<bool> OnMoveStateChanged; // 新增：移动状态变化时触发（比如禁用移动时的反馈）


        private void Awake()
        {
            // 原有初始化：属性/天赋/威胁值
            InitializeBaseAttributes();
            CurrentEnergy = MaxEnergy;
            InitializeTalentState();
            if (IsBoss && BossHasToughness)
            {
                CalculateBossMaxToughness();
                Attributes[AttributeType.BossCurrentToughness] = Attributes[AttributeType.BossMaxToughness];
            }
            if (IsBoss)
            {
                _playerThreatDict.Clear();
                CurrentTarget = null;
            }

            // 新增：初始化移动组件（如果没手动拖，自动获取）
            if (Rb == null)
                Rb = GetComponent<Rigidbody>();
            // 新增：初始化血条UI（设置初始位置和数值）
            if (HpSlider != null)
            {
                UpdateHpSliderPosition();
                UpdateHpDisplay(Attributes[AttributeType.Health], Attributes[AttributeType.MaxHealth]);
            }
        }

        private void Update()
        {
            // 原有更新：能量/冷却/持续动作/BOSS回合
            if (IsPlayer)
            {
                RegenEnergy(Time.deltaTime);
                UpdateAllSkillCooling(Time.deltaTime);
                UpdateTalentState(Time.deltaTime);
                UpdateAllRunningSequences(Time.deltaTime);
                // 新增：玩家移动输入（只在允许移动时生效）
                if (CanMove)
                    HandlePlayerMoveInput();
            }
            else if (IsBoss)
            {
                UpdateAllRunningSequences(Time.deltaTime);
                _bossTurnTimer += Time.deltaTime;
                if (_bossTurnTimer >= 10f)
                {
                    BossTurnAction();
                    _bossTurnTimer = 0f;
                }
            }

            // 新增：实时更新血条位置（角色移动时，血条跟着动）
            if (HpSlider != null)
                UpdateHpSliderPosition();
        }


        // 新增：判断当前单位是否有正在运行的时序动作（核心逻辑：检查列表中是否有状态为Running的时序）
        public bool IsTimedSequenceRunning()
        {
            // 情况1：如果没有任何运行中的时序，直接返回false
            if (_runningTimedSequences.Count == 0)
                return false;

            // 情况2：遍历所有时序，只要有一个状态是“Running”，就返回true
            foreach (var (_, sequence, _) in _runningTimedSequences)
            {
                // 注意：这里需要确保 TimedSequenceData 类里有“State”属性（之前的代码应该定义过，比如枚举 TimedSequenceState）
                if (sequence.State == TimedSequenceState.Running)
                {
                    return true;
                }
            }

            // 情况3：有时序但都不是Running状态（比如已完成/中断），返回false
            return false;
        }

        // 实现UpdateAllRunningSequences方法
        public void UpdateAllRunningSequences(float deltaTime)
        {
            // 遍历副本避免修改集合时报错
            var sequencesCopy = new List<(SkillBase, TimedSequenceData, Unit)>(_runningTimedSequences);
            foreach (var (skill, sequence, target) in sequencesCopy)
            {
                if (sequence.State == TimedSequenceState.Running)
                {
                    skill.UpdateTimedSequence(this, target, deltaTime);
                    // 若技能完成/中断，从列表移除
                    if (sequence.State is TimedSequenceState.Completed or TimedSequenceState.Interrupted)
                    {
                        _runningTimedSequences.Remove((skill, sequence, target));
                        OnTimedSequenceStateChanged?.Invoke(sequence, sequence.State);
                    }
                }
            }
        }

        // 补充StartTimedSequence方法（与Update配套，用于启动时序技能）
        public void StartTimedSequence(TimedSequenceData sequence, SkillBase skill, Unit target)
        {
            if (sequence.State != TimedSequenceState.Ready) return;

            sequence.State = TimedSequenceState.Running;
            sequence.ElapsedTime = 0f;
            sequence.LastIntervalTime = 0f;
            _runningTimedSequences.Add((skill sequence, target));
            sequence.OnSequenceStart?.Invoke();
            OnTimedSequenceStateChanged?.Invoke(sequence, sequence.State);
        }

        // Unit.cs 中添加
        public Unit GetNearestTarget(bool isEnemyTarget = true)
        {
            var allUnits = LevelGrid.Instance.GetAllUnits(); // 需确保LevelGrid有该方法
            Unit nearestTarget = null;
            float minDistance = float.MaxValue;
            Vector3 selfPos = transform.position;

            foreach (var unit in allUnits)
            {
                // 过滤：非自身 + 目标类型匹配（敌人/友方）
                if (unit == this) continue;
                if (unit.IsEnemy() != isEnemyTarget) continue;
                if (unit.Attributes.TryGetValue(AttributeType.Health, out float hp) && hp <= 0) continue; // 过滤死亡单位

                // 计算距离
                float distance = Vector3.Distance(selfPos, unit.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestTarget = unit;
                }
            }
            return nearestTarget;
        }

        // 补充LevelGrid的GetAllUnits方法（若不存在）
        // LevelGrid.cs 中添加
        public List<Unit> GetAllUnits()
        {
            var allUnits = new List<Unit>();
            foreach (var cell in gridCellDictionary.Values) // 假设gridCellDictionary是格子存储
            {
                if (cell.HasUnit())
                {
                    allUnits.Add(cell.GetUnit());
                }
            }
            return allUnits;
        }

        // Unit.cs 中添加字段（记录已使用的前置技能，回合内有效）
        private HashSet<SkillBase> _usedPreSkills = new();

        // 标记前置技能为已使用
        public void MarkSkillAsUsed(SkillBase preSkill)
        {
            if (preSkill != null)
            {
                _usedPreSkills.Add(preSkill);
            }
        }

        // 检查前置技能是否已使用
        public bool HasUsedSkill(SkillBase preSkill)
        {
            return _usedPreSkills.Contains(preSkill);
        }

        // 清除已使用的前置技能（如回合结束时调用）
        public void ClearUsedPreSkill()
        {
            _usedPreSkills.Clear();
        }

        // 补充回合结束时调用ClearUsedPreSkill（在TurnManager中）
        // TurnManager.cs 的回合切换逻辑中添加
        private void EndTurn()
        {
            // 其他逻辑...
            foreach (var unit in GetCurrentPhaseUnits())
            {
                unit.ClearUsedPreSkill(); // 每个单位清除前置技能记录
            }
            // 切换回合...
        }

        public void TriggerPostSkillEffects(SkillBase usedSkill)
        {
            // 1. 先触发所有装备的技能后特效
            if (EquippedItems != null) // 避免列表列表为空导致报错
            {
                foreach (var equip in EquippedItems)
                {
                    if (equip != null) // 避免单个装备为空
                    {
                        equip.OnSkillUsedAfter(this, usedSkill); // 调用装备的“技能后逻辑”
                    }
                }
            }

            // 2. 再触发所有天赋的技能后特效
            if (LearnedTalents != null)
            {
                foreach (var talent in LearnedTalents)
                {
                    if (talent != null)
                    {
                        talent.OnSkillUsedAfter(this, usedSkill); // 调用天赋的“技能后逻辑”
                    }
                }
            }
        }

        #region 新增：1. 移动逻辑（玩家手动控制，BOSS可扩展AI移动）
        // 玩家移动输入处理（2D示例，WSAD或方向键）
        private void HandlePlayerMoveInput()
        {
            if (Rb == null) return;

            // 获取输入（水平/垂直方向）
            float horizontal = Input.GetAxisRaw("Horizontal"); // A=-1, D=1
            float vertical = Input.GetAxisRaw("Vertical"); // S=-1, W=1
            Vector2 moveDir = new Vector2(horizontal, vertical).normalized; // 归一化：避免斜向移动更快

            // 应用移动速度（关联AttributeType.MoveSpeed属性，装备/天赋加移动速度会生效）
            Rb.velocity = moveDir * MoveSpeed;

            // 触发移动状态事件（比如移动时播放动画）
            if (moveDir.magnitude > 0.1f)
                OnMoveStateChanged?.Invoke(true); // 正在移动
            else
                OnMoveStateChanged?.Invoke(false); // 停止移动
        }

        // 外部控制移动开关（比如持续动作中禁用移动）
        public void SetMoveEnabled(bool enabled)
        {
            if (CanMove == enabled) return;
            CanMove = enabled;
            // 禁用移动时，立刻停止当前移动
            if (!CanMove && Rb != null)
                Rb.velocity = Vector2.zero;
            OnMoveStateChanged?.Invoke(false); // 通知外部“已停止移动”
            Debug.Log($"{UnitName} 移动状态：{(enabled ? "允许" : "禁止")}");
        }

        // BOSS AI移动（留接口，后续可扩展，比如追着威胁最高目标跑）

        public void BossAIMove()
        {
            // 防御判断：不是BOSS/没目标/没3D刚体，直接退出
            if (!IsBoss || CurrentTarget == null || Rb == null)
                return;

            // 3D场景核心：用Vector3计算方向（适配X/Y/Z轴，符合博德之门3的3D空间）
            // 1. 计算BOSS到目标的方向向量（目标位置 - 自身位置）
            Vector3 targetDir = CurrentTarget.transform.position - transform.position;
            // 2. 忽略Y轴（可选：博德之门3风格通常是“平面移动”，避免BOSS因高度差偏移）
            targetDir.y = 0;
            // 3. 归一化方向：确保斜向移动速度和直线移动一致（不会斜着跑更快）
            targetDir = targetDir.normalized;

            // 关键：给3D刚体赋值速度（Rigidbody.velocity是Vector3类型，完全匹配无报错）
            Rb.velocity = targetDir * MoveSpeed;

            // 额外优化：让BOSS让BOSS面朝目标（符合3D游戏视觉逻辑，可选但推荐）
            if (targetDir.magnitude > 0.1f) // 避免目标过近时频繁旋转
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetDir);
                // 平滑旋转（参数0.1f是旋转速度，可调整）
                Rb.rotation = Quaternion.Lerp(Rb.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }
        #endregion


        #region 新增：2. 血条显示与更新逻辑
        // 更新血条位置（跟随角色，加偏移）
        private void UpdateHpSliderPosition()
        {
            if (HpSlider == null) return;
            // 世界坐标转UI屏幕坐标（确保血条显示在角色头顶）
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position + HpSliderOffset);
            HpSlider.transform.position = screenPos;
        }

        // 更新血条数值和文本（受伤/回血时调用）
        private void UpdateHpDisplay(float currentHp, float maxHp)
        {
            if (HpSlider != null)
            {
                HpSlider.maxValue = maxHp;
                HpSlider.value = currentHp;
            }
            if (HpText != null)
            {
                HpText.text = $"{Mathf.RoundToInt(currentHp)}/{Mathf.RoundToInt(maxHp)}";
            }
            // 触发HP变化事件（比如低血量时播放警告动画）
            OnHpChanged?.Invoke(currentHp, maxHp);
        }

        // 重写TakeDamage方法：加受伤反馈+血条更新（原有伤害逻辑不变，只加反馈）
        public void TakeDamage(float damage, DamageType damageType)
        {
            float finalDamage = CalculateFinalDamage(damage, damageType);
            if (Attributes.TryGetValue(AttributeType.Health, out float currHp))
            {
                currHp = Mathf.Max(0, currHp - finalDamage);
                Attributes[AttributeType.Health] = currHp;

                // 新增：受伤反馈（播放动画/扣血特效，留接口）
                PlayTakeDamageEffect();
                // 新增：更新血条显示
                UpdateHpDisplay(currHp, Attributes[AttributeType.MaxHealth]);

                // 新增：死亡判断（血量为0时触发死亡）
                if (currHp <= 0)
                    OnUnitDead();
            }
        }

        // 新增：回血方法（比如吃血药/技能回血，和扣血对应）
        public void Heal(float healAmount)
        {
            if (healAmount <= 0) return;
            if (Attributes.TryGetValue(AttributeType.Health, out float currHp) &&
                Attributes.TryGetValue(AttributeType.MaxHealth, out float maxHp))
            {
                currHp = Mathf.Min(maxHp, currHp + healAmount);
                Attributes[AttributeType.Health] = currHp;
                // 更新血条显示
                UpdateHpDisplay(currHp, maxHp);
                // 回血特效（留接口）
                PlayHealEffect();
                Debug.Log($"{UnitName} 回血 {healAmount:F1}，当前HP：{currHp:F1}");
            }
        }

        // 新增：受伤特效（留接口，可挂动画/粒子效果）
        private void PlayTakeDamageEffect()
        {
            // 示例：播放红色闪屏效果（实际项目替换为动画/粒子）
            StartCoroutine(TakeDamageFlash());
            // 可扩展：播放受伤音效、播放角色后仰动画等
        }

        // 新增：回血特效（留接口）
        private void PlayHealEffect()
        {
            // 示例：播放绿色闪屏效果
            StartCoroutine(HealFlash());
            // 可扩展：播放回血音效、播放绿色粒子等
        }

        // 新增：死亡处理（留接口，比如播放死亡动画、触发战斗结束）
        private void OnUnitDead()
        {
            CanMove = false; // 死亡后禁止移动
            if (Rb != null) Rb.velocity = Vector2.zero; // 停止移动
            // 可扩展：播放死亡动画、禁用碰撞体、通知战斗管理器等
            Debug.Log($"{UnitName} 已死亡");
        }

        // 辅助：受伤闪屏协程（简单示例）
        private System.Collections.IEnumerator TakeDamageFlash()
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr == null) yield break;

            sr.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            sr.color = Color.white;
        }

        // 辅助：回血闪屏协程（简单示例）
        private System.Collections.IEnumerator HealFlash()
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr == null) yield break;

            sr.color = Color.green;
            yield return new WaitForSeconds(0.1f);
            sr.color = Color.white;
        }
        #endregion


        #region 原有：3. 基础属性/装备/天赋/技能/威胁值等逻辑（完全保留，未修改）
        private void InitializeBaseAttributes()
        {
            if (IsBoss)
            {
                Attributes[AttributeType.MaxHealth] = 5000f;
                Attributes[AttributeType.Health] = 5000f;
                Attributes[AttributeType.Attack] = 150f;
                Attributes[AttributeType.Speed] = 0;
                Attributes[AttributeType.MoveSpeed] = 2f;
                Attributes[AttributeType.CoolingReductionOnSkillUse] = 0f;
                Attributes[AttributeType.ThreatGenerationBoost] = 0f;
            }
            else if (IsPlayer)
            {
                Attributes[AttributeType.MaxHealth] = 1000f;
                Attributes[AttributeType.Health] = 1000f;
                Attributes[AttributeType.Attack] = 100f;
                Attributes[AttributeType.Speed] = 0;
                Attributes[AttributeType.MoveSpeed] = 4f;
                Attributes[AttributeType.MaxEnergy] = 200f;
                Attributes[AttributeType.EnergyRegenPerSec] = 10f;
                Attributes[AttributeType.CoolingReductionOnSkillUse] = 0f;
                Attributes[AttributeType.ToughnessReductionBoost] = 0f;
                Attributes[AttributeType.ThreatGenerationBoost] = 0f;

            }
        }
        public void AddAttribute(AttributeType attrType, float addValue)
        {
            if (!Enum.IsDefined(typeof(AttributeType), attrType))
            {
                Debug.LogWarning($"无效属性：{attrType}");
                return;
            }

            // 已有该属性：累加加成；没有：直接赋值（避免Key不存在错误）
            if (Attributes.ContainsKey(attrType))
            {
                Attributes[attrType] += addValue;
            }
            else
            {
                Attributes[attrType] = addValue; // 新增属性（比如天赋加的新属性）
            }

            // 特殊处理：MaxEnergy变化时同步当前能量
            if (attrType == AttributeType.MaxEnergy)
            {
                CurrentEnergy = Mathf.Min(CurrentEnergy, Attributes[attrType]);
            }

            Debug.Log($"{UnitName} {attrType}增加{addValue}，当前值：{Attributes[attrType]}");
        }

        /// <summary>
        /// 移除属性加成（和Add对应，float类型）
        /// </summary>
        public void RemoveAttribute(AttributeType attrType, float removeValue)
        {
            if (!Enum.IsDefined(typeof(AttributeType), attrType) || !Attributes.ContainsKey(attrType))
            {
                Debug.LogWarning($"无法移除属性：{attrType}（无效或不存在）");
                return;
            }

            // 扣减后确保不小于0（避免属性变负数）
            Attributes[attrType] = Mathf.Max(0f, Attributes[attrType] - removeValue);

            // 特殊处理：MaxEnergy减少时同步当前能量
            if (attrType == AttributeType.MaxEnergy)
            {
                CurrentEnergy = Mathf.Min(CurrentEnergy, Attributes[attrType]);
            }

            Debug.Log($"{UnitName} {attrType}减少{removeValue}，当前值：{Attributes[attrType]}");
        }

        /// <summary>
        /// 获取属性最终值（直接返回你的Attributes字典值，无额外逻辑）
        /// </summary>
        public float GetFinalAttributeValue(AttributeType attrType)
        {
            // 存在属性则返回值，不存在返回0（避免Key不存在错误）
            Attributes.TryGetValue(attrType, out float value);
            return value;
        }



        #endregion
        #region 4. 能量系统（不变）
        private void RegenEnergy(float deltaTime)
        {
            float regenAmount = EnergyRegenPerSec * deltaTime;
            CurrentEnergy = Mathf.Min(CurrentEnergy + regenAmount, MaxEnergy);
            OnEnergyChanged?.Invoke();
        }

        public bool ConsumeEnergy(float cost)
        {
            if (CurrentEnergy < cost) return false;
            CurrentEnergy = Mathf.Max(0, CurrentEnergy - cost);
            OnEnergyChanged?.Invoke();
            return true;
        }
        #endregion
        #region 5.  TurnTime
        /// <summary>
        /// 获取当前单位的回合时间（整数秒，速度越高时间越长）
        /// </summary>
        private Dictionary<AttributeType, (int BaseValue, int EquipAddedValue)> _attributeDict = new()
        {

            { AttributeType.Speed, (0, 0) },            // Speed初始0（你的设定），靠装备/天赋增加    
            { AttributeType.MaxEnergy, (100, 0) },      // 初始最大能量100（之前确认的核心设定）
            { AttributeType.EnergyRegenPerSec, (5, 0) }};   // 每秒回复5点（之前确认的核心设定）
        public int GetTurnTime()
        {
            // 1. 基础回合时间（你之前确认的默认6秒）
            int baseTurnTime = 6;

            // 2. Speed影响：1点Speed=回合时间+1秒（你的核心规则）
            int currentSpeed = GetFinalAttributeValue(AttributeType.Speed); // Speed是整数（装备/天赋增加）
            int speedExtraTime = currentSpeed * 1; // 1点+1秒，无系数，直接相乘

            // 3. 最终回合时间（整数，最低1秒避免异常）
            int finalTurnTime = Math.Max(1, baseTurnTime + speedExtraTime);

            // 日志：明确显示Speed对回合时间的影响（符合你的属性重要性设定）
            Debug.Log($"{UnitName} 回合时间：基础{baseTurnTime}s + Speed({currentSpeed}点)×1s = {finalTurnTime}s");
            return finalTurnTime;
        }
        #endregion
        #region 6. 派生动作管理（【新增】完整逻辑）
        // 触发派生动作
        public bool TriggerDerivedAction(SkillBase derivedSkill)
        {
            if (derivedSkill == null || derivedSkill.SkillType != SkillType.Derived)
                return false;

            // 检查派生动作是否可释放
            if (!derivedSkill.CanUse(this)) return false;

            // 释放派生动作
            derivedSkill.Use(this, GetNearestTarget());
            OnDerivedActionTriggered?.Invoke(derivedSkill);
            return true;
        }

        // 标记前置技能已使用（派生动作依赖）
        public void MarkSkillAsUsed(SkillBase preSkill)
        {
            if (preSkill == null && !_usedPreSkills.Contains(preSkill))
            {
                _usedPreSkills.Add(preSkill);
                // 前置技能标记5秒后失效（避免永久触发）
                Invoke(nameof(ClearUsedPreSkill), 5f);
            }
        }
        #endregion

        #region 7. BOSS韧性+伤害处理（不变）
        private void CalculateBossMaxToughness()
        {
            float maxTough = ToughnessSource == BossToughnessSource.PercentOfMaxHealth
                ? Attributes[AttributeType.MaxHealth] * ToughnessPercent
                : FixedToughnessValue;
            Attributes[AttributeType.BossMaxToughness] = maxTough;
        }

        public void ReduceToughness(float reductionAmount)
        {
            if (!IsBoss || !BossHasToughness || IsInvulnerableToToughness) return;

            if (Attributes.TryGetValue(AttributeType.BossCurrentToughness, out float currTough) &&
                Attributes.TryGetValue(AttributeType.BossMaxToughness, out float maxTough))
            {
                float toughBoost = Attributes.TryGetValue(AttributeType.ToughnessReductionBoost, out var val) ? val : 0f;
                float finalReduction = reductionAmount * (1 + toughBoost);
                currTough = Mathf.Max(0, currTough - finalReduction);
                Attributes[AttributeType.BossCurrentToughness] = currTough;

                if (currTough <= 0)
                {
                    OnToughnessBroken?.Invoke();
                    // 破韧后触发派生动作（装备/天赋可配置）
                    foreach (var equip in EquippedItems)
                    {
                        equip.CheckTriggerEffect(this, DerivedActionTrigger.AfterToughnessBreak);
                    }
                }
            }
        }

        public void ResetToughness()
        {
            if (!IsBoss || !BossHasToughness) return;
            if (Attributes.TryGetValue(AttributeType.BossMaxToughness, out float maxTough))
            {
                Attributes[AttributeType.BossCurrentToughness] = maxTough;
            }
        }



        private float CalculateFinalDamage(float damage, DamageType damageType)
        {
            if (damageType == DamageType.Physical && Attributes.TryGetValue(AttributeType.Armor, out float armor))
            {
                float armorReduction = Mathf.Min(0.8f, armor * 0.001f);
                return damage * (1 - armorReduction);
            }
            return damage;
        }

        public void ApplyToughnessAndThreat(float damage, SkillBase skill, Unit target)
        {
            if (target.IsBoss)
            {
                float toughReduction = damage * skill.ToughnessReductionRatio;
                target.ReduceToughness(toughReduction);
            }

            if (IsPlayer && target.IsBoss)
            {
                float threat = damage * skill.ThreatGenerationRatio;
                // 威胁值逻辑（不变）
            }
        }
        #endregion


        #region 8. 冷却系统（仅更新，无百分比）
        private void UpdateAllSkillCooling(float deltaTime)
        {
            foreach (var skill in Skills)
            {
                skill.UpdateCooling(deltaTime);
            }
        }
        #endregion

        // （以下省略原有代码：EquipItem/UnequipItem、LearnTalent/ForgetTalent、RegenEnergy/ConsumeEnergy、
        // StartTimedSequence/UpdateAllRunningSequences、TriggerDerivedAction、CalculateBossMaxToughness/ReduceToughness、
        // ApplyToughnessAndThreat、BossTurnAction/PerformNormalAttack 等）
    }
}
