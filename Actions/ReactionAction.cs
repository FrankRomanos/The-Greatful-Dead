using System;
using System.Collections.Generic;
using UnityEngine;

// 反应动作：可中断其他动作，在自身/敌方回合触发，不可作为派生前置
public abstract class ReactionAction : BaseAction
{
    [Header("反应动作配置")]
    [SerializeField] private int reactionCoolingSeconds = 30; // 冷却秒数（如30秒=5回合）
    [SerializeField] private bool triggerOnSelfTurn = true; // 自身回合可触发
    [SerializeField] private bool triggerOnEnemyTurn = true; // 敌方回合可触发

    private BaseAction _triggerSourceAction; // 触发反应的源动作（如攻击）

    // 动作类型：反应动作
    public override ActionType GetActionType()
    {
        return ActionType.Reaction;
    }

    // 基础冷却秒数（实现基类抽象方法）
    public override int GetBaseCoolingSeconds()
    {
        return reactionCoolingSeconds;
    }

    // 动作名称（子类需重写）
    public override string GetActionName()
    {
        return "反应动作";
    }

    // 执行反应动作（中断源动作）
    public override void TakeAction(GridPosition position, Action onActionComplete)
    {
        if (!CanExecute() || (NeedConsumeEnergy() && !HasEnoughEnergy()))
        {
            onActionComplete?.Invoke();
            return;
        }

        ActionStart(onActionComplete);
        if (NeedConsumeEnergy())
        {
            ConsumeEnergy(); // 消耗精力（部分反应动作可能不消耗）
        }
        ExecuteReactionAction(_triggerSourceAction); // 执行反应逻辑（如格挡、闪避）
        ActionComplete();
    }

    // 抽象方法：具体反应逻辑
    protected abstract void ExecuteReactionAction(BaseAction sourceAction);

    // 有效位置（反应动作通常无需位置，返回自身）
    public override List<GridPosition> GetValidActionGridPosition()
    {
        return new List<GridPosition> { unit.GetGridPosition() };
    }

    // 行动点消耗（通常不消耗）
    public override int GetActionPointsCost()
    {
        return 0;
    }

    // 可执行条件：冷却结束 + 触发条件满足 + 基类条件
    public override bool CanExecute()
    {
        return CurrentCoolingSeconds <= 0
               && IsValidTrigger()
               && base.CanExecute();
    }

    // 可被连锁规则：可连锁除整轮外的所有动作
    public bool CanBeChainedBy(BaseAction otherAction)
    {
        return otherAction.GetActionType() != ActionType.FullRound;
    }

    // 不可作为派生动作前置
    public bool CanBePreviousActionForDerived()
    {
        return false;
    }

    // 设置触发源动作（如被攻击时传入攻击动作）
    public void SetTriggerSource(BaseAction sourceAction)
    {
        _triggerSourceAction = sourceAction;
    }

    // 检查是否符合触发条件（回合+源动作）
    private bool IsValidTrigger()
    {
        bool isCorrectTurn = (GameManager.Instance.IsPlayerTurn() && triggerOnSelfTurn)
                            || (!GameManager.Instance.IsPlayerTurn() && triggerOnEnemyTurn);
        return isCorrectTurn && IsValidSourceAction(_triggerSourceAction);
    }

    // 抽象方法：是否需要消耗精力（子类决定，如闪避消耗、格挡不消耗）
    protected abstract bool NeedConsumeEnergy();

    // 检查源动作是否有效（子类可重写，如"仅对攻击动作反应"）
    protected virtual bool IsValidSourceAction(BaseAction sourceAction)
    {
        return sourceAction != null;
    }
}
