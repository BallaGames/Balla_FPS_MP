using Balla;
using Unity.Netcode;
using UnityEngine;

[SelectionBase]
public class TestFirearm : NetworkBehaviour
{
    public float roundsPerMinute = 1200;
    [ReadOnly] public float timeBetweenShots;
    float fireTimer;
    public ParticleSystem muzzle;
    public bool fireInput;
    bool _lastFire;
    public bool canAutoFire;

    ProjectileModule module;

    protected virtual bool CanFire => fireTimer <= 0;


    public float maxDistance;

    private void Start()
    {
        if (module == null)
            module = GetComponent<ProjectileModule>();
    }

    private void FixedUpdate()
    {
        //If we are the owner and are the local client (server should not handle fire input directly in future)
        if ((IsClient || IsHost) && IsOwner)
        {
            if(_lastFire != fireInput)
            {
                _lastFire = fireInput;
            }
        }

        //Only server should handle firing the weapon.
        if (IsServer || IsHost)
        {
            if (fireTimer > 0)
                fireTimer -= Time.fixedDeltaTime;

            if (CanFire && fireInput)
            {
                FireWeapon();
                
            }
        }



    }
    [Rpc(SendTo.Everyone)]
    public void SyncFireInputRPC(bool input)
    {
        if (!IsOwner)
            fireInput = input;
    }
    [Rpc(SendTo.Everyone)]
    public void SendFired_RPC()
    {
        if(!IsServer && !IsHost)
            WeaponFX();
    }
    void WeaponFX()
    {
        if (muzzle != null)
            muzzle.Play();
    }

    public void FireWeapon()
    {
        SendFired_RPC();
        WeaponFX();
        fireTimer = timeBetweenShots;
        //transition from raycast firing to projecitle firing
        ProjectileManager.QueueProjectile(module);
        SendFired_RPC();
    }

    private void OnValidate()
    {
        float lastTime = timeBetweenShots;
        timeBetweenShots = 1 / (roundsPerMinute / 60);
        //If we've changed our rof, we'll update it
        if(lastTime != timeBetweenShots)
            fireTimer = timeBetweenShots;

    }
}
