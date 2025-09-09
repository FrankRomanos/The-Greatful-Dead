using System;
using System.Collections.Generic;
using UnityEngine;

// 整轮动作：需空过回合，不可被连锁，执行后延迟生效
public abstract class FullRoundAction : BaseAction
{
    [Header("整轮动作配置")]
    [SerializeField] private int fullRoundCoolingSeconds = 180; // 冷却秒数（如180秒=30回合）
    [SerializeField] private int emptyRounds = 1; // 需空过的回合数

    private int _remainingEmptyRounds; // 剩余空过回合数

    // 动作类型：整轮动作
    public override ActionType GetActionType()
    {
        return ActionType.FullRound;
    }

    // 基础冷却秒数（实现基类抽象方法）
    public override int GetBaseCoolingSeconds()
    {
        return fullRoundCoolingSeconds;
    }

    // 动作名称（子类需重写）
    public override string GetActionName()
    {
        return "整轮动作";
    }

    // 执行动作（启动空过回合）
    public override void TakeAction(GridPosition position, Action onActionComplete)
    {
        if (!CanExecute() || !HasEnoughEnergy())
        {
            onActionComplete?.Invoke();
            return;
        }

        ActionStart(onActionComplete);
        ConsumeEnergy(); // 消耗大量精力
        _remainingEmptyRounds = emptyRounds;
        StartChargingEffect(position); // 启动蓄力特效
        GameManager.Instance.MarkUnitAsEmptyRound(unit, emptyRounds); // 标记单位需空过回合
    }

    // 每回合结束时调用（检查空过是否完成）
    public void OnRoundEnd()
    {
        if (_remainingEmptyRounds > 0)
        {
            _remainingEmptyRounds--;
            if (_remainingEmptyRounds <= 0)
            {
                ActivateFinalEffect(); // 空过结束，触发最终效果
                ActionComplete(); // 触发冷却
            }
        }
    }

    // 抽象方法：启动蓄力效果（如角色发光）
    protected abstract void StartChargingEffect(GridPosition position);

    // 抽象方法：触发最终效果（如高额伤害）
    protected abstract void ActivateFinalEffect();

    // 有效位置（通常为自身）
    public override List<GridPosition> GetValidActionGridPosition()
    {
        return new List<GridPosition> { unit.GetGridPosition() };
    }

    // 行动点消耗（消耗所有剩余行动点）
    public override int GetActionPointsCost()
    {
        return unit.GetActionPoints(); // 假设Unit有剩余行动点字段
    }

    // 可执行条件：冷却结束 + 有足够行动点 + 基类条件
    public override bool CanExecute()
    {
        return CurrentCoolingSeconds <= 0
               && unit.GetActionPoints() >= GetActionPointsCost()
               && base.CanExecute();
    }

    // 可被连锁规则：不可被任何动作连锁
    public bool CanBeChainedBy(BaseAction otherAction)
    {
        return false;
    }

    // 不可作为派生动作前置
    public bool CanBePreviousActionForDerived()
    {
        return false;
    }
}

