using UnityEngine;
using System.Collections.Generic;

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

    public void RemoveZombie(Zombie zombie)
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
}