using System;
using System.Collections.Generic;
using UnityEngine;

// 自由动作：不占时间，可连锁大部分动作，有独立冷却和精力消耗
public abstract class FreeAction : BaseAction
{
    [Header("自由动作配置")]
    [SerializeField] private int freeActionCoolingSeconds = 30; // 冷却秒数（如30秒=5回合）
    [SerializeField] private int actionPointCost = 0; // 通常不消耗行动点

    // 动作类型：自由动作
    public override ActionType GetActionType()
    {
        return ActionType.Free;
    }

    // 基础冷却秒数（实现基类抽象方法）
    public override int GetBaseCoolingSeconds()
    {
        return freeActionCoolingSeconds;
    }

    // 动作名称（子类需重写）
    public override string GetActionName()
    {
        return "自由动作";
    }

    // 执行动作（瞬间完成，0耗时）
    public override void TakeAction(GridPosition position, Action onActionComplete)
    {
        // 检查条件：可执行 + 精力充足
        if (!CanExecute() || !HasEnoughEnergy())
        {
            onActionComplete?.Invoke();
            return;
        }

        ActionStart(onActionComplete);
        ConsumeEnergy(); // 消耗精力（基类方法）
        ExecuteFreeAction(position); // 子类实现具体逻辑
        ActionComplete(); // 立即完成（0耗时）
    }

    // 抽象方法：具体自由动作逻辑（如大招特效）
    protected abstract void ExecuteFreeAction(GridPosition position);

    // 有效位置（默认自身位置，子类可扩展）
    public override List<GridPosition> GetValidActionGridPosition()
    {
        return new List<GridPosition> { unit.GetGridPosition() };
    }

    // 行动点消耗
    public override int GetActionPointsCost()
    {
        return actionPointCost;
    }

    // 可执行条件：冷却结束 + 基类条件
    public override bool CanExecute()
    {
        return CurrentCoolingSeconds <= 0 && base.CanExecute();
    }

    // 可被连锁规则：除整轮动作外，均可连锁自由动作
    public bool CanBeChainedBy(BaseAction otherAction)
    {
        return otherAction.GetActionType() != ActionType.FullRound;
    }

    // 不可作为派生动作前置
    public bool CanBePreviousActionForDerived()
    {
        return false;
    }
}

