using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Body parts will, when damaged, impair the player's ability to perform some actions.<br></br>
/// As an example, a damaged arm will reduce the player's recoil control and reload speed.
/// <br></br>A damaged leg will impair the player's movement speed and prevent them sprinting (if implemented)
/// <br></br>Taking damage to the head may impair vision, and body damage may prevent health regeneration
/// </summary>
[System.Serializable] 
public class BodyPart
{

}


public class Character : BaseDamageable
{







    public override void ReceiveDamage(DamageSource source, Vector3 point, Vector3 dir, float damage)
    {

    }

    public override void ReceiveDamage(DamageSource source, Vector3 dir, float damage)
    {

    }

    public override void ReceiveDamage(DamageSource source, float damage)
    {

    }
}
