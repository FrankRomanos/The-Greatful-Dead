using GameCore;
using System;

namespace GameCore.TurnAction
{
    // 持续动作数据模型（BOSS/玩家的持续动作都用这个）
    [Serializable]
    public class TimedSequenceData
    {
        public string SequenceName;          // 持续动作名称（如“火焰喷射”）
        public float TotalDuration;          // 总持续时间（秒）
        public float Interval;               // 帧更新间隔（如0.5秒触发一次伤害）
        public float DamagePerInterval;      // 每次间隔的伤害（如每0.5秒10点伤害）
        public float EnergyCostPerSecond;    // 每秒能量消耗（如持续期间每秒5点能量）
        public bool CanBeInterrupted;        // 是否可被中断（如玩家闪避可中断）
        public Action OnSequenceStart;       // 开始时回调
        public Action OnSequenceUpdate;      // 每帧更新回调
        public Action OnSequenceEnd;         // 结束时回调
        public Action OnSequenceInterrupt;   // 中断时回调

        // 运行时状态（不序列化，实时更新）
        public TimedSequenceState State { get; set; } = TimedSequenceState.Ready;
        public float ElapsedTime { get; set; } = 0f;
        public float LastIntervalTime { get; set; } = 0f;
    }
}