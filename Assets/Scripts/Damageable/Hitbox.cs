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


    public override void ReceiveDamage(DamageSource source, Vector3 point, Vector3 dir, float damage)
    {
        owner.ReceiveDamage(source, point, dir, damage);
        rb.AddForceAtPosition(source.forceMultiplier * damage * dir, point, ForceMode.Impulse);
    }

    public override void ReceiveDamage(DamageSource source, Vector3 dir, float damage)
    {
        owner.ReceiveDamage(source, dir, damage);
        rb.AddForce(source.forceMultiplier * damage * dir, ForceMode.Impulse);
    }
}
