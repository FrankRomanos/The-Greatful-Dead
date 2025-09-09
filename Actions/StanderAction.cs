using System;
using System.Collections.Generic;
using UnityEngine;

// 标准动作基类（如普攻、火球术）
public abstract class StandardAction : BaseAction
{
    [Header("标准动作配置")]
    [SerializeField] private int actionPointCost = 1; // 消耗的行动点
    [SerializeField] private int standardCoolingSeconds = 12; // 标准动作冷却秒数

    // 六大动作规则：固定为标准动作类型
    public override ActionType GetActionType()
    {
        return ActionType.Standard;
    }

    // 六大动作规则：可被反应动作和自由动作连锁
    public bool CanBeChainedBy(BaseAction otherAction)
    {
        return otherAction.GetActionType() == ActionType.Reaction
            || otherAction.GetActionType() == ActionType.Free;
    }

    // 六大动作规则：可作为派生动作的前置
    public bool CanBePreviousActionForDerived()
    {
        return true;
    }

    // 实现抽象方法：动作名称（由子类重写）
    public override string GetActionName()
    {
        return "StandardAction"; // 子类需重写（如"火球术"）
    }

    public override int GetBaseCoolingSeconds()
    {
        return standardCoolingSeconds;
    }


    // 实现抽象方法：执行动作（核心逻辑）
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
        ExecuteStandardAction(position); // 子类实现具体逻辑
    }

    // 抽象方法：具体标准动作逻辑（如火球术伤害、普攻动画）
    protected abstract void ExecuteStandardAction(GridPosition position);

    // 实现抽象方法：有效位置列表（由子类定义范围）
    public override List<GridPosition> GetValidActionGridPosition()
    {
        // 子类需重写（如近战1格、远程3格）
        return new List<GridPosition>();
    }

    // 实现抽象方法：行动点消耗
    public override int GetActionPointsCost()
    {
        return actionPointCost;
    }

    // 实现抽象方法：可执行条件
    public override bool CanExecute()
    {
        // 标准动作可执行：冷却结束 + 基类条件
        return CurrentCoolingSeconds <= 0 && base.CanExecute();
    }

    // 标准动作执行完成（触发冷却）
    protected void CompleteStandardAction()
    {
        ActionComplete(); // 调用基类完成逻辑（触发冷却）
    }
}

