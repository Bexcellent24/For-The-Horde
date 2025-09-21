using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class HordeManager : MonoBehaviour
{
    public static HordeManager Instance;

    public Transform player; // main zombie
    public GameObject zombiePrefab;
    

    private List<Zombie> horde = new List<Zombie>();

    void Awake()
    {
        Instance = this;
    }

    public void AddZombie(Vector3 spawnPos)
    {
        GameObject z = Instantiate(zombiePrefab, spawnPos, Quaternion.identity);
        Zombie zombieScript = z.GetComponent<Zombie>();

        zombieScript.SetFollowTarget(player);
        horde.Add(zombieScript);
        
        Debug.Log("Zombie added! Horde size = " + horde.Count);
    }

    public void RemoveFromHorde(Zombie zombie)
    {
        if (horde.Contains(zombie))
        {
            horde.Remove(zombie);
            Destroy(zombie.gameObject);
        }
    }

    public int GetHordeCount()
    {
        return horde.Count;
    }
    
   
   public void IncreaseHordeSpeed(float speedIncrease)
   {
       Debug.Log($"Increasing zombie speed by {speedIncrease}");
       
       //Upgrade player
       player.GetComponent<NavMeshAgent>().speed += speedIncrease;
       
       // Find all zombies only
       GameObject[] zombies = GameObject.FindGameObjectsWithTag("Zombie");
    
       foreach (GameObject zombie in zombies)
       {
           EnemyActor enemyActor = zombie.GetComponent<EnemyActor>();
           if (enemyActor != null && enemyActor.data != null)
           {
               enemyActor.EditSpeed(speedIncrease);
           }
       }
    
       Debug.Log($"Updated speed for {zombies.Length} zombies");
   }
   
    public void SpawnZombies(float zombies)
    {
        Debug.Log("Spawning zombies");

        float spawnRadius = 10f; 

        for (int i = 0; i < zombies; i++)
        {
            
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            
            Vector3 spawnPos = new Vector3(
                player.position.x + randomCircle.x,
                player.position.y,
                player.position.z + randomCircle.y
            );

            AddZombie(spawnPos);
        }
    }

}