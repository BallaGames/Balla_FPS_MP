using System.IO.IsolatedStorage;
using Unity.Netcode;
using UnityEngine;

public class RagdollController : NetworkBehaviour
{
    public Rigidbody[] ragdollBodies;
    public NetworkVariable<bool> stiffness = new();
    bool lastStiff;
    void OnValidate()
    {
        if (ragdollBodies.Length == 0)
        {
            ragdollBodies = GetComponentsInChildren<Rigidbody>();
        }

        if (IsServer)
            stiffness.Value = true;

        stiffness.OnValueChanged += OnStiffnessChanged;

        OnStiffnessChanged(false, true);
    }

    public void OnStiffnessChanged(bool previous, bool current)
    {
        SetStiffness();
    }

    void SetStiffness()
    {
        if (lastStiff != stiffness.Value)
        {
            for (int i = 0; i < ragdollBodies.Length; i++)
            {
                ragdollBodies[i].isKinematic = stiffness.Value;
            }
            lastStiff = stiffness.Value;
        }
    }
}
