using UnityEngine;

public class DamageSource : MonoBehaviour
{

    public float baseDamage;
    public float forceMult;
    public virtual float CalculateDamage()
    {
        return baseDamage;
    }
}
