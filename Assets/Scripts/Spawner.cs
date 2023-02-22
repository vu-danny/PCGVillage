using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] protected List<WeightedPrefab> spawnablePrefabs;

    protected void Spawn()
    {
        // TODO : implement and rename method
    }

    protected GameObject SpawnRandomPrefab()
    {
        // Retrieve the element to spawn
        GameObject prefabToSpawn = GetRandomSpawnablePrefab();

        if (prefabToSpawn == null)
            return null;
        
        // Compute its position
        Vector3 spawnPosition = transform.position;

        // Spawn the element and return its instance
        GameObject spawnedObject = Instantiate(prefabToSpawn, spawnPosition, Quaternion.LookRotation(transform.forward, transform.up));

        return spawnedObject;
    }

    protected GameObject GetRandomSpawnablePrefab()
    {
        int weightSum = 0;
        foreach (WeightedPrefab prefab in spawnablePrefabs)
        {
            weightSum += prefab.weight;
        }

        int selectionValue = Random.Range(0, weightSum);
        foreach (WeightedPrefab prefab in spawnablePrefabs)
        {
            selectionValue -= prefab.weight;
            if (selectionValue < 0)
                return prefab.prefab;
        }
        
        return null;
    }
    
    [System.Serializable]
    protected struct WeightedPrefab
    {
        public GameObject prefab;
        public int weight;
    }
}
