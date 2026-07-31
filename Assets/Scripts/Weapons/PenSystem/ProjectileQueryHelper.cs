using System.Collections.Generic;
using UnityEngine;

public class ProjectileQueryHelper : MonoBehaviour
{
    public static Dictionary<PhysicsMaterial, MaterialDataScriptableObject> MaterialData;
    public MaterialDataScriptableObject[] materialSources;
    public float penetrateDotThreshold = 0.5f;
    public static ProjectileQueryHelper Instance;

    public LayerMask penetrateMask;

    public static Dictionary<Collider, BaseDamageable> damageables;



    public static void RegisterDamageable(Collider col, BaseDamageable d)
    {
        damageables ??= new();
        damageables.TryAdd(col, d);
    }
    public static bool TryGetDamageable(Collider col, out BaseDamageable damageable)
    {
        return damageables.TryGetValue(col, out damageable);
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            return;
            MaterialData = new();
        for (int i = 0; i < materialSources.Length; i++)
        {
            MaterialData.TryAdd(materialSources[i].material, materialSources[i]);
        }
    }
}
