using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Effect
{
    [SerializeReference, SubclassSelector]
    public List<IEffect<BaseDamageable>> effects = new();
    internal DamageSource source;
    int numEnded = 0;

    [SerializeField] internal int poolnum;
    EffectParticle ep;
    public void Execute(BaseDamageable target)
    {
        bool didApply = false;
        numEnded = 0;
        foreach (var effect in effects)
        {
            didApply |= target.ApplyEffect(effect);
            effect.SetSource(source);
            effect.OnCompleted += Effect_OnCompleted;
        }
        if (didApply)
        {
            Debug.Log("applied an effect");
            EffectParticleManager.GetEffectParticle(poolnum, target, out ep, true);
        }
    }

    private void Effect_OnCompleted(IEffect<BaseDamageable> obj)
    {
        numEnded++;
        if(numEnded >= effects.Count - 1)
        {
            OnEnded();
        }
    }

    public void OnEnded()
    {
        if(ep != null)
        {
            ep.Stop();
            ep = null;
        }
    }
}

