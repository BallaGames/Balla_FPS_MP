using Unity.Netcode;
using UnityEngine;


public class Door : BaseDamageable
{
    public ConfigurableJoint hinge;
    protected bool lastState;

    Rigidbody rb;

    public float hingeMaxHealth;
    [SerializeField] protected float hingeHealth;

    public NetworkVariable<bool> isDestroyed = new();
    public NetworkVariable<bool> isHingeBroken = new();
    public NetworkVariable<bool> isLockBroken = new();

    public Vector3 hingePos;
    public Vector3 hingeBounds;

    public const ConfigurableJointMotion Locked = ConfigurableJointMotion.Locked;
    public const ConfigurableJointMotion Limited = ConfigurableJointMotion.Limited;
    public const ConfigurableJointMotion Free = ConfigurableJointMotion.Free;

    public bool destroyLocksWhenHingesBroken;

    public ConfigurableJoint doorLock;
    public float lockMaxHealth;
   [SerializeField] protected float lockHealth;
    public Vector3 lockPos;
    public Vector3 lockBounds;

    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(hingePos, hingeBounds);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(lockPos, lockBounds);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        hingeHealth = hingeMaxHealth;
        lockHealth = lockMaxHealth;

        isHingeBroken.OnValueChanged += UpdateHingeBroken;
        isLockBroken.OnValueChanged += UpdateLockBroken;

        //Initialise on first spawn AND make sure all is good for new joiners
        UpdateHingeBroken(false, isHingeBroken.Value);
        UpdateLockBroken(false, isLockBroken.Value);
    }

    void UpdateHingeBroken(bool previous, bool current)
    {
        hinge.xMotion = current ? Free : Limited;
        hinge.yMotion = current ? Free : Limited;
        hinge.zMotion = current ? Free : Limited;
        hinge.angularXMotion = current ? Free : Limited;
        hinge.angularYMotion = current ? Free : Limited;
        hinge.angularZMotion = current ? Free : Limited;
    }
    void UpdateLockBroken(bool previous, bool current)
    {
        doorLock.xMotion = current ? Free : Locked;
        doorLock.yMotion = current ? Free : Locked;
        doorLock.zMotion = current ? Free : Locked;
        doorLock.angularXMotion = current ? Free : Limited;
        doorLock.angularYMotion = current ? Free : Limited;
        doorLock.angularZMotion = current ? Free : Limited;
    }


    public override void ReceiveDamage(DamageSource source, Vector3 point, Vector3 dir, float damage)
    {
        //convert point to local space
        Vector3 lp = transform.InverseTransformPoint(point);
        if(CheckBox(lp, hingeBounds, hingePos))
        {
            //Damage the hinge;
            Debug.Log("damaged door");
            hingeHealth -= damage;
            if(hingeHealth <= 0)
            {
                lockHealth = 0;
            }
        }
        else if(doorLock != null && CheckBox(lp, lockBounds, lockPos))
        {
            //damage the lock
            Debug.Log("damaged lock");
            lockHealth -= damage;
        }
        isHingeBroken.Value = hingeHealth <= 0;
        isLockBroken.Value = hingeHealth <= 0 || lockHealth <= 0;
        rb.AddForceAtPosition(source.forceMultiplier * damage * dir, point, ForceMode.Impulse);



    }
    bool CheckBox(Vector3 p, Vector3 bounds, Vector3 pos)
    {
        Vector3 min = pos - (bounds / 2);
        Vector3 max = pos + (bounds / 2);
        return (p.x <= max.x && p.x >= min.x) &&
            (p.y <= max.y && p.y >= min.y) &&
            (p.z <= max.z && p.z >= min.z);
    }

    public override void ReceiveDamage(DamageSource source, Vector3 dir, float damage)
    {
        hingeHealth -= damage;
        rb.AddForce(source.forceMultiplier * damage * dir, ForceMode.Impulse);
    }
}
