using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Netcode;


public class EffectParticleManager : NetworkBehaviour
{
    public static Dictionary<int, EffectParticlePool> pools;
    public static EffectParticleManager Instance;

    public List<EffectScriptableObject> effectTypes;
    public static List<EffectParticle> allVFX;


    private void Awake()
    {
        if (Instance != null)
            return;

        Instance = this;
        pools = new Dictionary<int, EffectParticlePool>();
        allVFX = new();
        EffectParticlePool.nextGlobalID = 0;
        foreach (var item in effectTypes)
        {
            item.effect.poolnum = item.effectPoolNum;
            pools.TryAdd(item.effectPoolNum, new(64, item.effectPoolNum, item.prefab));
        }
    }

    [Rpc(SendTo.NotServer)]
    public void SendActivate_RPC(int poolID, NetworkObjectReference nor)
    {
        if(nor.TryGet(out NetworkObject n) && pools.TryGetValue(poolID, out var pool))
        {
            if(pool.TryGetSingle(out var p))
            {
                p.followTransform = n.transform;
            }
        }
    }
    public void SendReturn_RPC(int poolID, int effectID)
    {
        if(pools.TryGetValue(poolID, out var pool))
        {
            pool.Return(effectID);
        }
    }

    public static void GetEffectParticle(int ID, BaseDamageable attach, out EffectParticle effect, bool replicate = false)
    {
        if (replicate)
            Instance.SendActivate_RPC(ID, attach.NetworkObject);


        if (pools.TryGetValue(ID, out var pool))
        {
            pool.TryGetSingle(out effect);
            effect.Play();

            effect.followTransform = attach.AttachPoint;
        }
        else
            effect = null;
    }

    public static void ReturnEffectParticle(EffectParticle effect, bool replicate = false)
    {
        if (replicate)
            Instance.SendReturn_RPC(effect.poolID, effect.localID);
        if (pools.TryGetValue(effect.poolID, out var pool))
        {
            pool.Return(effect);
        }
    }

}




public class EffectParticlePool
{
    public EffectParticlePool(int baseCount, int poolNum, EffectParticle effectPrefab)
    {
        this.poolNum = poolNum;
        aliveVFX = new();
        VFXs = new(new EffectParticle[baseCount]);

        prefab = effectPrefab;
        for (int i = 0; i < baseCount; i++)
        {
            VFXs[i] = CreateVFX(false);
        }
        EffectParticleManager.allVFX.AddRange(VFXs);
        Debug.Log($"created {baseCount} projectiles with ID {poolNum} using the prefab {prefab.name}");
    }
    public bool FindVFX(int ID, out EffectParticle p)
    {
        p = VFXs.FirstOrDefault(x => x.localID == ID);
        return p != null;
    }
    EffectParticle CreateVFX(bool addToArray)
    {
        var p = UnityEngine.Object.Instantiate(prefab);
        p.gameObject.hideFlags = HideFlags.HideInHierarchy;
        //assign to the projectile counter and then increment it

        p.localID = nextLocalID;
        p.globalID = nextGlobalID;
        nextLocalID++;
        p.name = $"{poolNum} -- {nextGlobalID}";
        nextGlobalID++;
        nextLocalID++;

        if (addToArray)
        {
            EffectParticleManager.allVFX.Add(p);
        }

        return p;
    }
    public int poolNum;
    //Made static so that all projectile pools should create a sequential list of IDs starting with the first pool created.
    //Pool 1 will have 0-49, pool 2 will have 50-99, etc.
    //with it not being static, each pool will have its own set of IDs, which will make tracking projectiles harder.
    internal static int nextGlobalID;
    internal int nextLocalID;

    EffectParticle prefab;
    HashSet<EffectParticle> aliveVFX;

    List<EffectParticle> VFXs;
    public bool TryGetSingle(out EffectParticle v)
    {
        //we will get the first projectile that is NOT in the 
        v = VFXs.FirstOrDefault(x => !aliveVFX.Contains(x));
        if (v == null)
            v = CreateVFX(true);
        aliveVFX.Add(v);
        return true;
    }

    public void Return(EffectParticle p)
    {
        aliveVFX.Remove(p);
        p.Stop();
    }
    public void Return(int ID)
    {
        aliveVFX.RemoveWhere(x => x.localID == ID);
    }
}