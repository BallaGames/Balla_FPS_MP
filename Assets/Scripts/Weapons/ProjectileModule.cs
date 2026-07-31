using Balla;
using UnityEngine;

public class ProjectileModule : MonoBehaviour
{
    /// <summary>
    /// Only exists to get the number of the projectile type more easily, which is why the typeNum field is readonly.
    /// </summary>
    public Projectile projectilePrefab;
    [ReadOnly] public int projTypeNum;

    public float maxMuzzleAngle = 5;
    public uint projectileCount = 1;

    public Transform muzzle;
    public TestFirearm firearm;

    private void Awake()
    {
        if(projectilePrefab != null)
        {
            projTypeNum = projectilePrefab.projTypeNum;
        }
    }
}
