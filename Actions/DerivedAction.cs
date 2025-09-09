using System;
using System.Collections.Generic;
using UnityEngine;

// 派生动作：必须跟随指定标准动作，不可被连锁，消耗精力
public abstract class DerivedAction : BaseAction
{
    [Header("派生动作配置")]
    [SerializeField] private int derivedCoolingSeconds = 6; // 冷却秒数（如6秒=1回合）
    [SerializeField] private int requiredPreviousActionId; // 必须的前置标准动作ID
    [SerializeField] private int actionPointCost = 0; // 通常不额外消耗行动点

    private BaseAction _previousAction; // 上一个执行的动作（用于前置检查）

    // 动作类型：派生动作
    public override ActionType GetActionType()
    {
        return ActionType.Derived;
    }

    // 基础冷却秒数（实现基类抽象方法）
    public override int GetBaseCoolingSeconds()
    {
        return derivedCoolingSeconds;
    }

    // 动作名称（子类需重写）
    public override string GetActionName()
    {
        return "派生动作";
    }

    // 执行派生动作（必须检查前置动作）
    public override void TakeAction(GridPosition position, Action onActionComplete)
    {
        if (!CanExecute() || !HasEnoughEnergy() || !IsPreviousActionValid())
        {
            onActionComplete?.Invoke();
            return;
        }

        ActionStart(onActionComplete);
        ConsumeEnergy(); // 消耗精力
        ExecuteDerivedAction(position); // 执行派生逻辑（如连斩）
        ActionComplete();
    }

    // 抽象方法：具体派生动作逻辑
    protected abstract void ExecuteDerivedAction(GridPosition position);

    // 有效位置（与前置标准动作一致）
    public override List<GridPosition> GetValidActionGridPosition()
    {
        if (_previousAction != null && IsPreviousActionValid())
        {
            return _previousAction.GetValidActionGridPosition();
        }
        return new List<GridPosition>();
    }

    // 行动点消耗
    public override int GetActionPointsCost()
    {
        return actionPointCost;
    }

    // 可执行条件：冷却结束 + 前置动作有效 + 基类条件
    public override bool CanExecute()
    {
        return CurrentCoolingSeconds <= 0
               && IsPreviousActionValid()
               && base.CanExecute();
    }

    // 可被连锁规则：不可被任何动作连锁
    public bool CanBeChainedBy(BaseAction otherAction)
    {
        return false;
    }

    // 可作为派生动作前置（极少用，留扩展）
    public bool CanBePreviousActionForDerived()
    {
        return true;
    }

    // 设置上一个动作（用于前置检查）
    public void SetPreviousAction(BaseAction previousAction)
    {
        _previousAction = previousAction;
    }

    // 检查前置动作是否有效（必须是指定标准动作）
    private bool IsPreviousActionValid()
    {
        return _previousAction != null
               && _previousAction.GetActionType() == ActionType.Standard
               && _previousAction.ActionId == requiredPreviousActionId; // 假设BaseAction有ActionId
    }
}
