using ImprovedTimers;
using System;
using System.Collections.Generic;
using UnityEngine;

public interface IEffect<TTarget>
{
    public void Apply(TTarget target);
    public void Cancel();

    event Action<IEffect<TTarget>> OnCompleted;
    void SetSource(DamageSource source);
}

