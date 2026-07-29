using UnityEngine;
using UnityEngine.VFX;

public class Projectile : MonoBehaviour
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
    public bool aliveIndicator;
    [SerializeField] internal Vector3 velocity;
    [SerializeField] internal float gravityMult;
    [SerializeField] internal float bounceThreshold;
    [SerializeField] internal int bounces;
    internal int bouncesDone;
    [SerializeField] internal float bounciness;
    [SerializeField] internal float startSpeed;
    public bool initialised;
    public void Initialise(ProjectileModule source)
    {
        trailFX.Simulate(1);
        transform.position = source.muzzle.position;
        life = 0;
        bouncesDone = 0;
        velocity = startSpeed * source.transform.forward;
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
    public bool TickProjectile(float delta, bool didHit, bool terminated, Vector3 hitPoint, Vector3 hitNormal, Vector3 direction)
    {
        if (didHit)
        {
            transform.position = hitPoint;
            if (bounces > 0 && bouncesDone < bounces)
            {
                if (Vector3.Dot(hitNormal, direction) < bounceThreshold)
                {
                    velocity = Vector3.Reflect(velocity, hitNormal) * bounciness;
                    bouncesDone++;
                }
            }
            else
            {
                //Debug.Log($"Terminated projectile {name}");
                life = expireTime + 1;
            }
        }
        else if (!terminated && Alive)
        {
            life += delta;
            transform.position += velocity * delta;
            velocity += delta * gravityMult * Physics.gravity;
        }
        aliveIndicator = Alive;
        if (terminated || life >= expireTime)
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
