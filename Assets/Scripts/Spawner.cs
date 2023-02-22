using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] protected SpawnableSet spawnableSet;
    protected List<WeightedPrefab> remainingPrefabs;
    
    protected virtual void Awake() 
    {
        remainingPrefabs = new List<WeightedPrefab>();
    }

    public void ResetPrefabs()
    {
        remainingPrefabs.Clear();
        spawnableSet.CopyTo(remainingPrefabs);
    }

    // Returns spawnable if successfully spawned
    public Spawnable SpawnWithBoundsCheck(ReadOnlyCollection<Bounds> spawnedBounds)
    {
        do
        {
            GameObject spawnedObject = SpawnRandomPrefab();
            if (spawnedObject == null)
                return null;
            else
            {
                Spawnable spawnable = spawnedObject.GetComponent<Spawnable>();
                while (spawnable.PlaceRandomAnchorRelativeTo(transform))
                {
                    bool intersectionFound = false;
                    foreach (Bounds bounds in spawnedBounds)
                    {
                        if (spawnable.InterectsWith(bounds))
                        {
                            intersectionFound = true;
                            Destroy(spawnedObject);
                            break;
                        }
                    }

                    if (!intersectionFound)
                    {
                        remainingPrefabs.Clear();
                        return spawnable;
                    }
                }
            }
        }
        while (remainingPrefabs.Count > 0);

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
        foreach (WeightedPrefab prefab in remainingPrefabs)
        {
            weightSum += prefab.weight;
        }

        int selectionValue = Random.Range(0, weightSum);
        for (int i = 0; i < remainingPrefabs.Count; i++)
        {
            WeightedPrefab prefab = remainingPrefabs[i];
            selectionValue -= prefab.weight;
            if (selectionValue < 0)
            {
                remainingPrefabs.RemoveAt(i);
                return prefab.prefab;
            }
        }
        
        return null;
    }

    #if UNITY_EDITOR
    private void OnDrawGizmosSelected() 
    {
        float arrowSize = 0.25f;
        UnityEditor.Handles.color = Color.cyan;
        UnityEditor.Handles.ConeHandleCap(
            0, 
            transform.position + transform.forward * (arrowSize * 0.5f), 
            transform.rotation, 
            arrowSize, 
            EventType.Repaint
        );
    }
    #endif
}
