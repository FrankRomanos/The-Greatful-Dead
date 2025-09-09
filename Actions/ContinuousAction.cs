using System;
using System.Collections.Generic;
using UnityEngine;

// 持续动作：BOSS专属，提供额外时间，可被连锁，结束后影响下一回合
public abstract class ContinuousAction : BaseAction
{
    [Header("持续动作配置")]
    [SerializeField] private int continuousCoolingSeconds = 360; // 冷却秒数（如360秒=60回合）
    [SerializeField] private float extraTimeProvided = 4f; // 额外操作时间（秒）
    [SerializeField] private float continuousDuration = 8f; // 持续阶段总时长
    [SerializeField] private int actionPointCost = 0; // BOSS技能通常不消耗行动点

    private float _continuousTimer; // 持续阶段计时器
    private bool _isContinuousActive; // 持续阶段是否激活

    // 动作类型：持续动作
    public override ActionType GetActionType()
    {
        return ActionType.Continuous;
    }

    // 基础冷却秒数（实现基类抽象方法）
    public override int GetBaseCoolingSeconds()
    {
        return continuousCoolingSeconds;
    }

    // 动作名称（子类需重写）
    public override string GetActionName()
    {
        return "持续动作";
    }

    // 执行动作（启动持续阶段）
    public override void TakeAction(GridPosition position, Action onActionComplete)
    {
        if (!CanExecute() || !HasEnoughEnergy())
        {
            onActionComplete?.Invoke();
            return;
        }

        ActionStart(onActionComplete);
        ConsumeEnergy(); // 消耗精力（BOSS可能消耗大量精力）
        _isContinuousActive = true;
        _continuousTimer = continuousDuration;
        GameManager.Instance.AddExtraRoundTime(extraTimeProvided); // 增加额外时间
        StartContinuousEffect(position); // 启动领域等效果
    }

    private void Update()
    {
        if (!_isContinuousActive) return;

        _continuousTimer -= Time.deltaTime;
        if (_continuousTimer <= 0)
        {
            EndContinuousPhase(); // 结束持续阶段
        }
    }

    // 抽象方法：启动持续效果（如领域伤害）
    protected abstract void StartContinuousEffect(GridPosition position);

    // 结束持续阶段
    private void EndContinuousPhase()
    {
        _isContinuousActive = false;
        EndContinuousEffect(); // 子类实现结束效果
        ActionComplete(); // 触发冷却
    }

    // 抽象方法：结束持续效果（如残留伤害）
    protected abstract void EndContinuousEffect();

    // 有效位置（BOSS技能通常范围较大）
    public override List<GridPosition> GetValidActionGridPosition()
    {
        // 子类重写为实际范围（如以BOSS为中心5x5）
        return new List<GridPosition> { unit.GetGridPosition() };
    }

    // 行动点消耗
    public override int GetActionPointsCost()
    {
        return actionPointCost;
    }

    // 可执行条件：冷却结束 + 是BOSS单位 + 基类条件
    public override bool CanExecute()
    {
        return CurrentCoolingSeconds <= 0
               && unit.IsEnemy() // 假设Unit有IsEnemy()方法
               && base.CanExecute();
    }

    // 可被连锁规则：可被任何动作连锁（额外时间内允许操作）
    public bool CanBeChainedBy(BaseAction otherAction)
    {
        return true;
    }

    // 不可作为派生动作前置
    public bool CanBePreviousActionForDerived()
    {
        return false;
    }
}
