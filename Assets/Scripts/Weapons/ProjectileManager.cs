using System.Collections.Generic;
using System.Collections;
using Unity.Collections;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using Unity.Mathematics;

public struct ProjectileRequest
{
    public Vector3 origin, direction;
    public ProjectileModule source;
    public int projTypeNum;
}



public class ProjectileManager : NetworkBehaviour
{
    public static ProjectileManager Instance { get; private set; }

    public List<Projectile> projectilePrefabs;
    HashSet<int> initialisedPools;
    public Dictionary<int, ProjectilePool> pools;
    public List<int> poolProjectileNums;


    public static List<Projectile> allProjectiles;
    public List<Projectile> aliveProjectiles;
    HashSet<Projectile> removeAfterSim;
    int activeCount = 0;
    bool staggering;
    int staggerChunkSize = 128;
    public LayerMask projectileMask;

    [SerializeField] int maxRequestsPerFrame, maxHits;

    int[] poolIDs;
    int[] poolNums;
    Vector3[] positions;
    bool[] terminated;

    QueryParameters qp ;


    public static void TerminateProjectile(Projectile p)
    {
        Instance.pools[p.projTypeNum].TerminateProjectiles();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            //If we dont currently have a manager, we'll make this one the manager.
            Instance = this;
        }
        else
            //but if we're not the current manager, then we'll skip the next bit.
            return;

        qp = new(projectileMask, true, QueryTriggerInteraction.UseGlobal, false);

        ProjectilePool.nextProjID = 0;
        initialisedPools = new();
        allProjectiles = new();
        aliveProjectiles = new();
        pools = new();
        
        
        poolIDs = new int[maxRequestsPerFrame];
        poolNums = new int[maxRequestsPerFrame];
        positions = new Vector3[maxRequestsPerFrame];
        terminated = new bool[maxRequestsPerFrame];

        for (int i = 0; i < projectilePrefabs.Count; i++)
        {
            var currProj = projectilePrefabs[i];
            //If we've already initialised a pool with this typenum, we'll skip over it for now.
            //that likely means there was a setup error, so we'll be nice and 
            if (initialisedPools.Contains(currProj.projTypeNum))
                continue;

            ProjectilePool pool = new(currProj.startCount, currProj.projTypeNum, currProj);
            pools.Add(currProj.projTypeNum, pool);
        }

    }
    private void FixedUpdate()
    {
        if (!IsServer && !IsHost)
            return;

        if (allProjectiles == null || allProjectiles.Count == 0)
            return;
        SimulateProjectiles();

        if (aliveProjectiles.Count > 0)
            SyncProjectiles();
    }

    void SyncProjectiles()
    {

        //Make sure that we never execute more projectiles than we can per frame
        for (int i = 0; i < math.min(aliveProjectiles.Count, maxRequestsPerFrame); i++)
        {
            poolIDs[i] = aliveProjectiles[i].localProjCounter;
            poolNums[i] = aliveProjectiles[i].projTypeNum;
            positions[i] = aliveProjectiles[i].transform.position;
            terminated[i] = !aliveProjectiles[i].Alive;
        }

        if (activeCount > 128)
        {
            if (!staggering)
            {
                StartCoroutine(StaggerSync(poolIDs, poolNums, positions, terminated, staggerChunkSize));
            }
        }
        else
        {
            int end = activeCount;
            SyncProjectilePosition_RPC(poolIDs[0..end], poolNums[0..end], positions[0..end], terminated[0..end]);
        }
        //since we need projectiles for syncing once they've been simulated, we'll do the cleanup AFTER simulation instead
        aliveProjectiles.RemoveAll(x => !x.Alive);
        foreach (var item in pools)
        {
            item.Value.TerminateProjectiles();
        }
    }

    NativeArray<RaycastCommand> commands;
    NativeArray<RaycastHit> hits;
    int projIndex;
    RaycastHit closestHit;
    float closestDistance;
    public void SimulateProjectiles()
    {
        activeCount = aliveProjectiles.Count;
        if(activeCount == 0)
        {
            return;
        }

        commands = new(activeCount, Allocator.TempJob);
        hits = new(commands.Length * maxHits, Allocator.TempJob);

        projIndex = 0;
        for (int i = 0; i < aliveProjectiles.Count; i++)
        {
            Projectile v = aliveProjectiles[i];
            if (v == null || !v.Alive)
                continue;

            //Debug.Log("creating raycast command for this object", v.gameObject);
            commands[projIndex] = new(v.transform.position, v.velocity.normalized, qp, v.velocity.magnitude * Time.fixedDeltaTime);
            projIndex++;
        }
        //Debug.Log($"simulating with {activeCount}/{commands.Length}/{aliveProjectiles.Count} projectiles. ");
        var handle = RaycastCommand.ScheduleBatch(commands, hits, (int)Mathf.Max(activeCount / JobsUtility.JobWorkerCount, 1), maxHits);
        handle.Complete();

        projIndex = 0;

        for (int x = 0; x < aliveProjectiles.Count; x++)
        {
            Projectile p = aliveProjectiles[x];
            if (p == null || !p.Alive)
                continue;
            //Debug.Log($"operating on projectile {p.name}");
            bool didHit = false;
            closestDistance = 10000;
            int offset = projIndex * maxHits;
            for (int y = 0; y < maxHits; y++)
            {
                RaycastHit hit = hits[offset + y];
                if(hit.collider != null)
                {
                    if (hit.distance > 0 && hit.distance < closestDistance)
                    {
                        closestHit = hit;
                        didHit = true;
                    }
                }
            }
            if (didHit)
            {
                Debug.DrawLine(commands[projIndex].from, closestHit.point, Color.green, 1f);
                //If the projectile is termianted, we will continue onto the next one immediately.
                if(ProjectileQueryHelper.TryGetDamageable(closestHit.collider, out var damageable))
                {
                    damageable.ReceiveDamage(p, closestHit.point, commands[projIndex].direction, p.CalculateDamage());
                }
                if (p.TickProjectile(Time.fixedDeltaTime, closestHit, commands[projIndex].direction))
                {
                    //Debug.Log($"Terminated projectile {p.name}, likely hit.", p);
                }
            }
            else
            {
                Debug.DrawRay(commands[projIndex].from, commands[projIndex].direction * commands[projIndex].distance, Color.red, 0.04f);
                if (p.TickProjectile(Time.fixedDeltaTime))
                {
                    //Debug.Log($"Terminated projectile {p.name}, likely expired", p);
                }
            }
            projIndex++;
        }
        commands.Dispose();
        hits.Dispose();

    }

    public static void QueueProjectile(ProjectileModule source)
    {
        for (int i = 0; i < source.projectileCount; i++)
        {
            if (Instance.pools[source.projTypeNum].TryGetSingle(out Projectile p))
            {
                p.trailFX.Stop();
                p.Initialise(source);
                Instance.aliveProjectiles.Add(p);

            }
        }
    }

    //sync projectiles with whoever is not the server. This should mean that the host/server does not update twice.
    [Rpc(SendTo.NotServer, DeferLocal = true)]
    public void SyncProjectilePosition_RPC(int[] localIDs, int[] poolNums, Vector3[] newPos, bool[] terminated)
    {
        //If we are server or host and this somehow makes it in, then we'll ignore it here too
        if (IsServer || IsHost)
            return;
        Debug.Log("Trying to sync projectiles");
        //When we receive a sync message, we'll execute this stuff
        for (int i = 0; i < localIDs.Length; i++)
        {
            Projectile p = pools[poolNums[i]].FindProjectile(localIDs[i]);
            if (p == null)
                continue;
            //If a bullet we want to sync is disabled/hidden, we will turn it enable/show it.
            //Replace this with better logic later that is not so costly.
            if (!p.initialised)
            {
                p.Initialise();
            }
            p.transform.position = newPos[i];
            if (terminated[i])
            {
                //If a projectile was terminated this frame,
                p.Terminate();
            }
        }

    }
    IEnumerator StaggerSync(int[] globalIDs, int[] poolNums, Vector3[] newPos, bool[] terminated, int chunkSize)
    {
        staggering = true;
        int num = 0;
        var wff = new WaitForSeconds(0.02f);
        while (num < globalIDs.Length - 1)
        {
            int endIndex = math.min(globalIDs.Length, num + chunkSize);
            SyncProjectilePosition_RPC(globalIDs[num..endIndex], poolNums[num..endIndex], newPos[num..endIndex], terminated[num..endIndex]);
            num += chunkSize;
            yield return wff;
        }
    }

}
