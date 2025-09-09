using UnityEngine;

// 单例模式临时实现，后续可重构
public class GameManager : MonoBehaviour
{
    // 单例实例（临时简化）
    public static GameManager Instance { get; private set; }

    // 临时标记：是否为玩家回合（可后续改为实际逻辑）
    private bool _isPlayerTurn = true;

    private void Awake()
    {
        // 简单单例逻辑（避免重复实例）
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 检查是否为玩家回合（供反应动作使用）
    public bool IsPlayerTurn()
    {
        return _isPlayerTurn; // 临时返回true，后续替换为实际回合逻辑
    }

    // 增加额外回合时间（供持续动作使用）
    public void AddExtraRoundTime(float extraTime)
    {
        // 临时空实现，后续添加实际时间管理逻辑
        Debug.Log($"增加额外时间：{extraTime}秒");
    }

    // 标记单位需要空过回合（供整轮动作使用）
    public void MarkUnitAsEmptyRound(Unit unit, int rounds)
    {
        // 临时空实现，后续添加实际空过逻辑
        Debug.Log($"{unit.name} 需要空过 {rounds} 回合");
    }

    // （可选）切换回合的临时方法（供测试用）
    public void SwitchTurn()
    {
        _isPlayerTurn = !_isPlayerTurn;
        Debug.Log($"回合切换：{(IsPlayerTurn() ? "玩家回合" : "敌方回合")}");
    }
}
