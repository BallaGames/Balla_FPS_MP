using UnityEngine;

[CreateAssetMenu(fileName = "New Material Data", menuName = "Scriptable Objects/New Material Data")]
public class MaterialDataScriptableObject : ScriptableObject
{
    public PhysicsMaterial material;
    [Tooltip("If this material can be penetrated")]
    public bool canPenetrate = true;
    [Tooltip("How hard this surface is to penetrate per metre of thickness.\n" +
        "Higher values require a projectile with higher penetrative ability.")]
    public float penetrateResist = 100f;
    [Tooltip("How much life a projectile retains (as a multiplier) when penetrating this object, per metre of thickness."), Range(0.2f, 1)]
    public float penetrateLifeMult = 0.75f;
    [Tooltip("How much velocity is retained (as multiplier) when penetrating this surface, per metre of thickness.\n" +
        "Lower values mean more velocity is lost"), Range(0.2f, 1)]
    public float penetrateVelocityMultiply = 0.75f;
    public float minVelocityToEscape = 20f;
    public float maxPenDistance = .5f;

}
