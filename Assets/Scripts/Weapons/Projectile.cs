using Unity.Mathematics;
using UnityEngine;
using UnityEngine.VFX;

public class Projectile : DamageSource
{
    /// <summary>
    /// Which kind of projectile this is; functionally an Id but my d key is on the fritz.
    /// </summary>
    public int projTypeNum;
    /// <summary>
    /// a counter that tricks WHICH projectile this one is
    /// </summary>
    public int globalProjCounter;
    public int localProjCounter;
    public int startCount;

    public VisualEffect trailFX;


    [SerializeField] float expireTime;
    [SerializeField] float life;
    public bool Alive => life < expireTime;
    [SerializeField] internal Vector3 velocity;
    [SerializeField] internal float gravityMult;
    [SerializeField] internal float bounceThreshold;
    [SerializeField] internal int bounces;
    internal int bouncesDone;
    [SerializeField] internal float bounciness;
    [SerializeField] internal float startSpeed;
    /// <summary>
    /// Our projectile's ability to penetrate something.<br></br>
    /// Multiplied by speed when calculating penetration
    /// </summary>
    [SerializeField] internal float penetratePower;

    int maxPenChecks = 4;
    RaycastHit[] penCheckHits;
    private void Awake()
    {
        penCheckHits = new RaycastHit[maxPenChecks];
    }

    public bool initialised;
    public void Initialise(ProjectileModule source)
    {
        trailFX.Simulate(1);
        transform.position = source.muzzle.position;
        life = 0;
        bouncesDone = 0;
        velocity = source.transform.TransformDirection(Quaternion.Euler(UnityEngine.Random.insideUnitCircle * source.maxMuzzleAngle) * (startSpeed * Vector3.forward));
        if(trailFX != null)
            trailFX.Play();
        initialised = true;

    }
    public void Initialise()
    {
        if (initialised)
            return;
        Debug.Log("particle init");
        trailFX.Simulate(1);
        if (trailFX != null)
            trailFX.Play();

        initialised = true;
    }


    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="delta"></param>
    /// <param name="didHit"></param>
    /// <param name="terminated"></param>
    /// <param name="hitPoint"></param>
    /// <param name="hitNormal"></param>
    /// <param name="direction"></param>
    /// <returns>Whether this projectile was terminated.</returns>
    public bool TickProjectile(float delta, RaycastHit hit, Vector3 direction)
    {
        bool terminated = false;
        transform.position = hit.point;
        bool bounced = false;
        if (bounces > 0 && bouncesDone < bounces)
        {
            if (Mathf.Abs(Vector3.Dot(hit.normal, direction)) < bounceThreshold)
            {
                velocity = Vector3.Reflect(velocity, hit.normal) * bounciness;
                bouncesDone++;
                bounced = true;
            }
        }
        if (!bounced)
        {
            //We can only penetrate if we didn't bounce
            //And we'll terminate the projectile if we can't penetrate.
            terminated = !PenetrateCheck(hit, direction);
        }
        if (terminated || life >= expireTime)
        {
            if(terminated)
                transform.position = hit.point;
            life = expireTime + 1;
            Terminate();
            return true;
        }
        else
        {
            life += delta;
            transform.position += velocity * delta;
            velocity += delta * gravityMult * Physics.gravity;
        }
        return false;
    }
    /// <summary>
    /// Checks whether this projectile can penetrate a surface it collides with.<br></br>
    /// Some projectiles may have other behaviour when embedded in a surface (when it penetrates but cannot escape)
    /// </summary>
    /// <param name="hit"></param>
    /// <param name="direction"></param>
    /// <param name="embedded"></param>
    /// <returns>True if the projectile penetrated the surface it hit</returns>
    bool PenetrateCheck(RaycastHit hit, Vector3 direction)
    {
        if(ProjectileQueryHelper.MaterialData.TryGetValue(hit.collider.sharedMaterial, out var data))
        {
            Debug.Log($"Penetrating object with {hit.collider.sharedMaterial.name} material");
            if (math.abs(Vector3.Dot(hit.normal, direction)) > ProjectileQueryHelper.Instance.penetrateDotThreshold)
            {
                Ray r = new(hit.point + direction * data.maxPenDistance, -direction);
                int i = Physics.RaycastNonAlloc(r, penCheckHits, data.maxPenDistance, ProjectileQueryHelper.Instance.penetrateMask, QueryTriggerInteraction.Ignore);
                Debug.DrawRay(r.origin, r.direction * data.maxPenDistance, Color.orange, 0.2f);
                if (i > 0)
                {
                    for (int x = 0; x < i; x++)
                    {
                        //we need to check that we've hit the same collider as we did on the way in. If not, we can't penetrate
                        //Additionally, if we've got one at d = 0, then it means we've not hit something and should ignore it.
                        if (penCheckHits[x].distance == 0 || penCheckHits[x].distance > data.maxPenDistance || penCheckHits[x].collider != hit.collider)
                            continue;
                        var hit2 = penCheckHits[x];
                        //Our projectile's ability to penetrate something is reliant on the thickness and "resistance" of a surface.
                        //This is only an arbitrary value, but we can tune these later.
                        //If our projectile's power is GREATER than this, then we can penetrate.
                        float thickness = data.maxPenDistance - hit2.distance;
                        float thicknessCoeff = Mathf.InverseLerp(data.maxPenDistance, 0, hit2.distance);
                        Debug.Log(thicknessCoeff);
                        float penResist = math.abs(data.penetrateResist * thickness);
                        float v = velocity.magnitude;
                        if (penResist < penetratePower * v)
                        {
                            //If we get to here, we've done all the following...
                            // > Hit a surface at a steep enough angle to penetrate
                            // > Checked if the surface is penetrable using raycasts
                            // > Checked that our bullet is fast and strong enough to penetrate this surface
                            //So now we can penetrate the surface!
                            velocity = math.lerp(velocity, velocity * data.penetrateVelocityMultiply, thicknessCoeff);
                            life = math.lerp(life, math.lerp(expireTime, life, data.penetrateLifeMult), thicknessCoeff);

                            //offset us forward a tiny bit to make sure we can get through the surface we just hit
                            transform.position += direction * 0.01f;
                            //But if our bullet is slowed down TOO much by penetration, then we'll get stuck in the surface.
                            if (v < data.minVelocityToEscape)
                                return false;

                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }

    public bool TickProjectile(float delta)
    { 
        if (Alive)
        {
            life += delta;
            transform.position += velocity * delta;
            velocity += delta * gravityMult * Physics.gravity;
        }
        if (life >= expireTime)
        {
            Terminate();
            return true;
        }
        return false;
    }


    public void Terminate()
    {
        if (!initialised)
            return;

        if (trailFX != null)
            trailFX.Stop();


        initialised = false;

    }
}
