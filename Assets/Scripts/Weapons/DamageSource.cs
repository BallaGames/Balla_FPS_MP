using UnityEngine;

public class DamageSource : MonoBehaviour
{
    public float forceMultiplier;
    public float baseDamage;

    public float CalculateDamage()
    {
        return baseDamage;
    }
}
