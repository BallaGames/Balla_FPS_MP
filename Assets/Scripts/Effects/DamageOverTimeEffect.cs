using ImprovedTimers;
using System;
using System.Diagnostics;

[Serializable]
public class DamageOverTimeEffect : IEffect<BaseDamageable>
{
    public float duration = 5;
    public float interval = 1;
    public float damagePerTick = 5;

    IntervalTimer timer;
    BaseDamageable currTarget;
    DamageSource source = null;

    public event Action<IEffect<BaseDamageable>> OnCompleted;

    public void Apply(BaseDamageable target)
    {
        currTarget = target;
        timer = new(duration, interval);

        timer.OnInterval += OnInterval;
        timer.OnTimerStop += Cleanup;
        timer.Start();
    }
    public void SetSource(DamageSource source)
    {
        this.source = source;
    }

    protected virtual void Cleanup()
    {
        timer = null;
        currTarget = null;
        source = null;
        OnCompleted?.Invoke(this);
    }

    protected virtual void OnInterval()
    {
        if (currTarget != null && source != null)
        {
            currTarget.ReceiveDamage(source, damagePerTick);
            UnityEngine.Debug.Log($"Damaged {currTarget} for {damagePerTick} points");
        }
    }

    public void Cancel()
    {
        timer?.Stop();
        Cleanup();
        OnCompleted?.Invoke(this);
    }
}

