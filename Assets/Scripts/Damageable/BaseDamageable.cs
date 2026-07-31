using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Collider))]
/// <summary>
/// Base class that receives damage from weapons
/// </summary>
public abstract class BaseDamageable : NetworkBehaviour
{
    Collider col;
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

    protected virtual void OnValidate()
    {
        if (col == null)
            col = GetComponent<Collider>();
    }

}
