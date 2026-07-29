using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

public class ProjectilePool
{ 
    public ProjectilePool(int baseCount, int poolNum, Projectile projectilePrefab)
    {
        this.poolNum = poolNum;
        aliveProjectiles = new();
        projectiles = new(new Projectile[baseCount]);
        
        prefab = projectilePrefab;
        for (int i = 0; i < baseCount; i++)
        {
            projectiles[i] = CreateProjectile(false);
        }
        ProjectileManager.allProjectiles.AddRange(projectiles);
        Debug.Log($"created {baseCount} projectiles with ID {poolNum} using the prefab {prefab.name}");
    }

    public Projectile FindProjectile(int localID)
    {
        if(localID < projectiles.Count && localID > -1)
        {
            return projectiles[localID];
        }
        return null;
    }

    Projectile CreateProjectile(bool addToArray)
    {
        var p = UnityEngine.Object.Instantiate(prefab);
        p.hideFlags = HideFlags.HideAndDontSave;
        //assign to the projectile counter and then increment it
        p.globalProjCounter = nextProjID;
        p.localProjCounter = nextLocalProjID;
        p.name = $"{poolNum} -- {p.globalProjCounter}";
        nextProjID++;
        nextLocalProjID++;

        if (addToArray)
        {
            ProjectileManager.allProjectiles.Add(p);
        }

        return p;
    }
    public int poolNum;
    //Made static so that all projectile pools should create a sequential list of IDs starting with the first pool created.
    //Pool 1 will have 0-49, pool 2 will have 50-99, etc.
    //with it not being static, each pool will have its own set of IDs, which will make tracking projectiles harder.
    internal static int nextProjID;
    internal int nextLocalProjID;

    Projectile prefab;
    HashSet<Projectile> aliveProjectiles;

    public void TerminateProjectiles()
    {
        aliveProjectiles.RemoveWhere(x => !x.Alive);
    }

    List<Projectile> projectiles;
    public bool TryGetSingle(out Projectile p)
    {
        //we will get the first projectile that is NOT in the 
        p = projectiles.FirstOrDefault(x => !aliveProjectiles.Contains(x));
        if (p == null)
            p = CreateProjectile(true);
        aliveProjectiles.Add(p);
        return true;
    }
    
}
