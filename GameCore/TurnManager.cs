using GameCore;

using GameCore.Skill;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore
{
    public class TurnManager : MonoBehaviour
    {
        public static TurnManager Instance { get; private set; }

        [Header("战斗配置")]
        public List<Unit> playerUnits = new List<Unit>(); // 玩家角色列表
        public List<Unit> enemyUnits = new List<Unit>();  // 非BOSS敌人列表
        public List<Unit> bossUnits = new List<Unit>();   // BOSS列表
        public float coolDownUpdateInterval = 1f;         // 技能冷却更新间隔（秒）

        [Header("当前回合状态")]
        public bool isPlayerTurnPhase = true; // 是否玩家回合阶段（玩家→BOSS循环）
        public int currentUnitIndex = 0;      // 当前行动单位索引
        public bool isInSpecialTurn = false;  // 是否处于BOSS特殊回合（时序动作）
        public List<Skill> activeTimedSkills = new List<Skill>(); // 活跃的时序动作

        private Unit currentActingUnit;       // 当前行动的单位
        private float coolDownTimer = 0f;     // 冷却计时器


        private void Awake()
        {
            // 单例模式：确保全局唯一
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }


        private void Update()
        {
            // 每1秒更新一次技能冷却（和游戏时间同步）
            if (Time.timeScale > 0)
            {
                coolDownTimer += Time.deltaTime;
                if (coolDownTimer >= coolDownUpdateInterval)
                {
                    UpdateAllSkillCoolDown();
                    coolDownTimer = 0f;
                }
            }
        }


        #region 战斗流程控制（开始/结束回合）
        /// <summary>
        /// 开始战斗（初始化所有状态）
        /// </summary>
        public void StartBattle()
        {
            ResetAllUnitsTurnState();
            StartNextUnitTurn();
            Debug.Log("=== 战斗开始！进入玩家回合阶段 ===");
        }

        /// <summary>
        /// 开始下一个单位的回合
        /// </summary>
        public void StartNextUnitTurn()
        {
            // 1. 确定当前阶段的单位列表（玩家/敌人/BOSS）
            List<Unit> currentPhaseUnits = GetCurrentPhaseUnits();
            if (currentPhaseUnits.Count == 0)
                return;

            // 2. 处理索引溢出（循环到第一个单位）
            if (currentUnitIndex >= currentPhaseUnits.Count)
            {
                SwitchTurnPhase(); // 切换回合阶段（玩家→BOSS/BOSS→玩家）
                currentPhaseUnits = GetCurrentPhaseUnits();
                currentUnitIndex = 0;
            }

            // 3. 开始当前单位的回合
            currentActingUnit = currentPhaseUnits[currentUnitIndex];
            currentActingUnit.ResetTurnState();
            Debug.Log($"=== {currentActingUnit.unitName} 回合开始（时长：{currentActingUnit.GetCurrentTurnDuration():F1}秒） ===");
        }

        /// <summary>
        /// 结束当前单位的回合（触发能量回复、切换单位）
        /// </summary>
        public void EndUnitTurn()
        {
            if (currentActingUnit == null)
                return;

            // 1. 触发当前单位的基础能量回复（用该单位的回合时长）
            float unitTurnDuration = currentActingUnit.GetCurrentTurnDuration();
            currentActingUnit.RegenBaseEnergy(unitTurnDuration);

            // 2. 标记单位已行动
            currentActingUnit.hasActedThisTurn = true;

            // 3. 检查是否是玩家阶段且所有玩家已行动（触发额外回复）
            if (isPlayerTurnPhase && IsAllUnitsActed(playerUnits))
            {
                TriggerAllPlayersExtraEnergy();
            }

            // 4. 检查BOSS特殊回合（时序动作）是否需要处理
            if (isInSpecialTurn && CheckAllPlayersFinishedSpecialTurn())
            {
                SettleTimedSkills();
                isInSpecialTurn = false;
            }

            // 5. 切换到下一个单位
            currentUnitIndex++;
            StartNextUnitTurn();
        }

        /// <summary>
        /// 切换回合阶段（玩家→BOSS 或 BOSS→玩家）
        /// </summary>
        private void SwitchTurnPhase()
        {
            isPlayerTurnPhase = !isPlayerTurnPhase;
            string phaseName = isPlayerTurnPhase ? "玩家" : "BOSS";
            Debug.Log($"=== 回合阶段切换：进入{phaseName}回合 ===");
        }

        /// <summary>
        /// 获取当前阶段的单位列表（玩家/敌人/BOSS）
        /// </summary>
        private List<Unit> GetCurrentPhaseUnits()
        {
            if (isPlayerTurnPhase)
                return playerUnits;
            else
                return bossUnits.Count > 0 ? bossUnits : enemyUnits;
        }
        #endregion


        #region 能量回复触发（基础+额外）
        /// <summary>
        /// 触发所有玩家的额外能量回复（玩家阶段结束时）
        /// </summary>
        private void TriggerAllPlayersExtraEnergy()
        {
            Debug.Log("=== 所有玩家回合结束，触发额外能量回复 ===");
            foreach (var player in playerUnits)
            {
                player.RegenPlayerExtraEnergy();
            }
        }

        /// <summary>
        /// 检查是否所有单位都已行动
        /// </summary>
        private bool IsAllUnitsActed(List<Unit> units)
        {
            return units.TrueForAll(unit => unit.hasActedThisTurn);
        }
        #endregion


        #region BOSS特殊回合（时序动作处理）
        /// <summary>
        /// 添加BOSS时序动作（触发特殊回合）
        /// </summary>
        public void AddTimedSkill(Skill skill)
        {
            if (skill.actionType != ActionType.TimedSequence || activeTimedSkills.Contains(skill))
                return;

            activeTimedSkills.Add(skill);
            isInSpecialTurn = true;
            skill.startTime = Time.time;
            // 触发玩家特殊回合（预支下回合时间）
            skill.TriggerSpecialTurn(playerUnits);
            Debug.Log($"=== BOSS释放时序技能：{skill.skillName}，进入特殊回合 ===");
        }

        /// <summary>
        /// 检查所有玩家是否完成特殊回合
        /// </summary>
        private bool CheckAllPlayersFinishedSpecialTurn()
        {
            return playerUnits.TrueForAll(player => !player.needSpecialTurn || player.hasFinishedSpecialTurn);
        }

        /// <summary>
        /// 结算所有到期的时序动作
        /// </summary>
        private void SettleTimedSkills()
        {
            List<Skill> skillsToRemove = new List<Skill>();
            foreach (var skill in activeTimedSkills)
            {
                if (skill.IsTimedSequenceExpired())
                {
                    skill.SettleTimedEffect(playerUnits);
                    skill.isSettled = true;
                    skillsToRemove.Add(skill);
                }
            }

            // 移除已结算的技能
            foreach (var skill in skillsToRemove)
                activeTimedSkills.Remove(skill);

            Debug.Log("=== 特殊回合结束，时序技能结算完成 ===");
        }
        #endregion


        #region 辅助方法（冷却更新、顺序调整、状态重置）
        /// <summary>
        /// 更新所有单位的技能冷却
        /// </summary>
        private void UpdateAllSkillCoolDown()
        {
            UpdateUnitsCoolDown(playerUnits);
            UpdateUnitsCoolDown(enemyUnits);
            UpdateUnitsCoolDown(bossUnits);
        }

        /// <summary>
        /// 更新单个列表中所有单位的技能冷却
        /// </summary>
        private void UpdateUnitsCoolDown(List<Unit> units)
        {
            foreach (var unit in units)
            {
                foreach (var skill in unit.skills)
                {
                    if (skill.currentCoolDown > 0)
                        skill.currentCoolDown--;
                }
            }
        }

        /// <summary>
        /// 调整玩家角色顺序（UI拖拽时调用）
        /// </summary>
        public void ReorderPlayerUnits(int fromIndex, int toIndex)
        {
            if (fromIndex < 0 || fromIndex >= playerUnits.Count || toIndex < 0 || toIndex >= playerUnits.Count)
                return;

            Unit temp = playerUnits[fromIndex];
            playerUnits[fromIndex] = playerUnits[toIndex];
            playerUnits[toIndex] = temp;
            Debug.Log($"调整玩家顺序：{fromIndex} → {toIndex}");
        }

        /// <summary>
        /// 重置所有单位的回合状态
        /// </summary>
        private void ResetAllUnitsTurnState()
        {
            ResetUnitsTurnState(playerUnits);
            ResetUnitsTurnState(enemyUnits);
            ResetUnitsTurnState(bossUnits);
        }

        private void ResetUnitsTurnState(List<Unit> units)
        {
            foreach (var unit in units)
            {
                unit.ResetTurnState();
            }
        }
        #endregion
    }
}

