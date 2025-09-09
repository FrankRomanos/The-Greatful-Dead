using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseAction : MonoBehaviour
{
    [Header("基础动作配置")]
    [SerializeField] protected int baseEnergyCost; // 每个动作的基础精力消耗（可在Inspector配置）

    // -------------------------- 临时模拟能量（Unit重构后删除）--------------------------
    [Header("临时模拟能量（仅当前测试用）")]
    [SerializeField] private int _simulateMaxEnergy = 100; // 模拟角色最大精力值
    private int _simulateCurrentEnergy; // 模拟角色当前精力值

    [Header("冷却配置")]
    [SerializeField] protected int baseCoolingSeconds; // 基础冷却秒数（如12秒=2回合）
    public int CurrentCoolingSeconds { get; protected set; } = 0;     //当前冷却
    public abstract int GetBaseCoolingSeconds();
     

    protected Unit unit;
    protected bool isActive;
    protected Action onActionComplete;


    protected virtual void Awake()
    {
        unit = GetComponent<Unit>();
        // 初始化临时模拟精力（游戏启动时充满）
        _simulateCurrentEnergy = _simulateMaxEnergy;
    }


    // -------------------------- 精力核心逻辑（后续对接Unit仅需修改这两个方法）--------------------------
    /// <summary>
    /// 检查是否有足够精力执行动作
    /// </summary>
    protected virtual bool HasEnoughEnergy()
    {
        // 临时：使用模拟精力判断
        return _simulateCurrentEnergy >= GetTotalEnergyCost();
    }

    /// <summary>
    /// 执行动作时消耗精力
    /// </summary>
    protected virtual void ConsumeEnergy()
    {
        // 临时：消耗模拟精力
        _simulateCurrentEnergy -= GetTotalEnergyCost();
        // 调试日志（可选，方便查看精力变化）
        Debug.Log($"[{unit.name}] 消耗精力：{GetTotalEnergyCost()} | 剩余精力：{_simulateCurrentEnergy}");
    }
    // ---------------------------------------------------------------------------------------------------


    /// <summary>
    /// 计算动作的总精力消耗（支持子类扩展，如装备减耗）
    /// </summary>
    protected virtual int GetTotalEnergyCost()
    {
        // 默认返回基础消耗，子类可重写（例：装备减20%消耗则返回 baseEnergyCost * 0.8）
        return baseEnergyCost;
    }


    // -------------------------- 抽象方法（子类必须实现）--------------------------
    /// <summary>
    /// 获取动作类型（六大动作之一）
    /// </summary>
    public abstract ActionType GetActionType();

    /// <summary>
    /// 获取动作名称（如“普攻”“火球术”）
    /// </summary>
    public abstract string GetActionName();


    /// <summary>
    /// 执行动作（核心逻辑）
    /// </summary>
    public abstract void TakeAction(GridPosition position, Action onActionComplete);

    /// <summary>
    /// 获取动作的有效执行位置列表
    /// </summary>
    public abstract List<GridPosition> GetValidActionGridPosition();

    /// <summary>
    /// 获取动作消耗的行动点
    /// </summary>
    public abstract int GetActionPointsCost();

    /// <summary>
    /// 检查动作是否可执行（冷却、精力等综合判断）
    /// </summary>
    public virtual bool CanExecute()
    {
        return true;
    }


    // -------------------------- 通用辅助方法 --------------------------
    /// <summary>
    /// 动作开始时的通用初始化
    /// </summary>
    protected void ActionStart(Action onActionComplete)
    {
        isActive = true;
        this.onActionComplete = onActionComplete;
    }

    /// <summary>
    /// 动作完成后的通用处理（触发冷却）
    /// </summary>
    protected void ActionComplete()
    {
        isActive = false;
        // 所有动作完成后触发冷却（冷却时间由子类定义）
        CurrentCoolingSeconds = GetBaseCoolingSeconds(); // 关键：调用子类实现的方法
        onActionComplete?.Invoke();
    }

    /// <summary>
    /// 更新冷却时间（每帧调用，由Unit或管理器驱动）
    /// </summary>
    public void UpdateCoolingByRound()
    {
        UpdateCoolingBySeconds(6); // 1回合=6秒，固定减少6秒
    }

    public void UpdateCoolingBySpecial(int secondsToReduce)
    {
        UpdateCoolingBySeconds(secondsToReduce);
    }

    private void UpdateCoolingBySeconds(int secondsToReduce)
    {
        CurrentCoolingSeconds = Mathf.Max(0, CurrentCoolingSeconds - secondsToReduce);
    }
    public int GetDisplayRounds()
    {
        if (CurrentCoolingSeconds <= 0) return 0;
        // 向上取整：(秒数 + 5) ÷ 6（避免浮点数运算，用整数计算）
        return (CurrentCoolingSeconds + 5) / 6;
    }

}