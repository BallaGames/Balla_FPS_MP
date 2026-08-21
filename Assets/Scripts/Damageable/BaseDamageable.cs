using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Collider))]
/// <summary>
/// Base class that receives damage from weapons
/// </summary>
public abstract class BaseDamageable : NetworkBehaviour
{
    Collider col;

    public virtual Transform AttachPoint => transform;

    public List<IEffect<BaseDamageable>> effects= new();
    public bool canTakeEffects;

    public override void OnNetworkSpawn()
    {
        ProjectileQueryHelper.RegisterDamageable(col, this);
    }

    private void Start()
    {
        if(col == null)
            col = GetComponent<Collider>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="effect"></param>
    /// <returns>true if successfully applied an effect</returns>
    public virtual bool ApplyEffect(IEffect<BaseDamageable> effect)
    {
        effect.OnCompleted += RemoveEffect;
        effects.Add(effect);
        effect.Apply(this);
        return true;
    }

    public virtual void RemoveEffect(IEffect<BaseDamageable> effect)
    {
        effects.Remove(effect);
        effect.OnCompleted -= RemoveEffect;
    }

    public virtual void OnDie(DamageSource deathSource)
    {
        foreach (var item in effects)
        {
            item.OnCompleted -= RemoveEffect;
            item.Cancel();
        }
        effects.Clear();
    }


    /// <summary>
    /// Causes this damageable to take an amount of damage, hit at the specified point.
    /// </summary>
    /// <param name="point"></param>
    /// <param name="damage"></param>
    public abstract void ReceiveDamage(DamageSource source, Vector3 point, Vector3 dir, float damage);
    /// <summary>
    /// Causes this damageable to take an amount of damage, spread all over the object.
    /// </summary>
    /// <param name="damage"></param>
    public abstract void ReceiveDamage(DamageSource source, Vector3 dir, float damage);
    public abstract void ReceiveDamage(DamageSource source, float damage);


    protected virtual void OnValidate()
    {
        if (col == null)
            col = GetComponent<Collider>();
    }


}
