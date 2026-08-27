using Unity.Netcode;
using UnityEngine;

public class PlayerController : BaseDamageable
{
    public float maxHealth;
    public NetworkVariable<float> currentHealth = new();

    public PlayerMotor motor;
    public PlayerCamera cam;

    public RagdollController ragdollController;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)
            currentHealth.Value = maxHealth;

        currentHealth.OnValueChanged += OnHealthChanged;
        OnHealthChanged(0, currentHealth.Value);


    }

    public void OnHealthChanged(float previous, float current)
    {

    }


    public override bool ApplyEffect(IEffect<BaseDamageable> effect)
    {
        return base.ApplyEffect(effect);

    }

    public override void OnDie(DamageSource deathSource)
    {
        base.OnDie(deathSource);

    }

    public override void OnNetworkPreDespawn()
    {
        base.OnNetworkPreDespawn();

    }

    public override void ReceiveDamage(DamageSource source, Vector3 point, Vector3 dir, float damage)
    {

    }

    public override void ReceiveDamage(DamageSource source, Vector3 dir, float damage)
    {

    }

    public override void ReceiveDamage(DamageSource source, float damage)
    {

    }

    public override void RemoveEffect(IEffect<BaseDamageable> effect)
    {
        base.RemoveEffect(effect);
    }
}
