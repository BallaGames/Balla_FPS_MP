using System.Drawing;
using UnityEngine;

/// <summary>
/// Passes damage events onto another damageable
/// </summary>
public class Hitbox : BaseDamageable
{
    public BaseDamageable owner;
    public Rigidbody rb;
    public bool hasRB = false;

    public override Transform AttachPoint => owner.AttachPoint;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (hasRB && rb == null)
            rb = GetComponent<Rigidbody>();
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        if(hasRB && rb == null)
            rb = GetComponent<Rigidbody>(); 
    }
    //effects can't be applied to hitboxes unlesss they're a limb effect, which we'll deal with later.
    //For now, we'll just try to pass it upwards to the owner.
    public override bool ApplyEffect(IEffect<BaseDamageable> effect)
    {
        if (!owner.effects.Contains(effect))
        {
            return owner.ApplyEffect(effect);
        }
        return false;
    }

    //We shouldn't have to do anything with this either right now.
    public override void RemoveEffect(IEffect<BaseDamageable> effect)
    {
        base.RemoveEffect(effect);
    }


    public override void ReceiveDamage(DamageSource source, Vector3 point, Vector3 dir, float damage)
    {
        owner.ReceiveDamage(source, point, dir, damage);
        //rb.AddForceAtPosition(source.forceMultiplier * damage * dir, point, ForceMode.Impulse);
    }

    public override void ReceiveDamage(DamageSource source, Vector3 dir, float damage)
    {
        owner.ReceiveDamage(source, dir, damage);
        //rb.AddForce(source.forceMultiplier * damage * dir, ForceMode.Impulse);
    }

    public override void ReceiveDamage(DamageSource source, float damage)
    {
        owner.ReceiveDamage(source, damage);
    }
}
