using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] protected List<WeightedPrefab> spawnablePrefabs;

    // Returns spawnable if successfully spawned
    protected Spawnable SpawnWithBoundsCheck(ReadOnlyCollection<Bounds> spawnedBounds)
    {
        do
        {
            GameObject spawnedObject = SpawnRandomPrefab();
            if (spawnedObject == null)
                return null;
            else
            {
                Spawnable spawnable = spawnedObject.GetComponent<Spawnable>();

                bool intersectionFound = false;
                foreach (Bounds bounds in spawnedBounds)
                {
                    if (spawnable.InterectsWith(bounds))
                    {
                        intersectionFound = true;
                        break;
                    }
                }

                if (!intersectionFound)
                    return spawnable;
            }
        }
        while (spawnablePrefabs.Count > 0);

        return null;
    }

    protected GameObject SpawnRandomPrefab()
    {
        // Retrieve the element to spawn
        GameObject prefabToSpawn = GetRandomSpawnablePrefab();

        if (prefabToSpawn == null)
            return null;

        // Spawn the element and return its instance
        GameObject spawnedObject = Instantiate(prefabToSpawn);

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
