using UnityEngine;

public class RagdollController : BaseDamageable
{
    public Rigidbody[] ragdollBodies;
    public bool stiff;
    bool lastStiff;
    protected override void OnValidate()
    {
        base.OnValidate();
        if (ragdollBodies.Length == 0)
        {
            ragdollBodies = GetComponentsInChildren<Rigidbody>();
        }
        SetStiffness(stiff);
    }

    void SetStiffness(bool stiffness)
    {
        stiff = stiffness;

        if (lastStiff != stiff)
        {
            for (int i = 0; i < ragdollBodies.Length; i++)
            {
                ragdollBodies[i].isKinematic = stiff;
            }
            lastStiff = stiff;
        }
    }

    public override void ReceiveDamage(DamageSource source, Vector3 dir, float damage)
    {
        SetStiffness(false);
    }
    public override void ReceiveDamage(DamageSource source, Vector3 point, Vector3 dir, float damage)
    {
        SetStiffness(false);
    }
}
