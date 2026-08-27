using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

//or should the door be interactable? we don't have interaction yet because we don't have a player yet
public class BaseDoor : BaseDamageable
{

    BaseDoorController controller;

    public bool canDoorBeDamaged;
    public float maxHealth;
    public NetworkVariable<float> currentHealth = new();
    float lastCurrHealth;
    public UnityEvent onDoorBroken;

    public ParticleSystem doorBreakParticle;
    public GameObject doorViz;
    Vector3 lastHitDir;


    float currDoorLerp;
    float doorLerpTarg;

    public float doorMass;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        controller = GetComponentInParent<BaseDoorController>();

        if(IsServer)
            currentHealth.Value = maxHealth;

        currentHealth.OnValueChanged += HealthChanged;
        
        HealthChanged(0, currentHealth.Value);

        lastHitDir = transform.forward;
    }

    protected virtual void FixedUpdate()
    {

    }

    protected virtual void HealthChanged(float prev, float curr)
    {
        if(currentHealth.Value > 0 && lastCurrHealth <= 0)
        {
            doorViz.SetActive(true);
        }

        if (currentHealth.Value <= 0 && lastCurrHealth > 0)
        {
            
        }
        lastCurrHealth = currentHealth.Value;
    }

    public override void ReceiveDamage(DamageSource source, Vector3 point, Vector3 dir, float damage)
    {
        ReceiveDamage(source, damage);
        lastHitDir = dir;
    }

    public override void ReceiveDamage(DamageSource source, Vector3 dir, float damage)
    {
        ReceiveDamage(source, damage);
        lastHitDir = dir;
    }

    public override void ReceiveDamage(DamageSource source, float damage)
    {
        currentHealth.Value -= damage;
    }

    public override void OnDie(DamageSource deathSource)
    {
        base.OnDie(deathSource);
    }

    protected override void OnClientDieRecevied()
    {
        base.OnClientDieRecevied();
        BreakEffects();
    }
    void BreakEffects()
    {
        doorBreakParticle.transform.forward = lastHitDir;
        doorViz.SetActive(false);
    }
}
