using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using Utility.UnityExtensions;

public class Spawner : MonoBehaviour
{
    [SerializeField] protected SpawnableSet spawnableSet;
    protected List<WeightedPrefab> remainingPrefabs;

    [SerializeField] protected Bounds bounds;
    
    protected virtual void Awake() 
    {
        remainingPrefabs = new List<WeightedPrefab>();
    }

    public void ResetPrefabs()
    {
        remainingPrefabs.Clear();
        spawnableSet.CopyTo(remainingPrefabs);
    }

    public TransformableBounds GetTransformedBounds()
    {
        return new TransformableBounds(bounds, transform.localToWorldMatrix);
    }

    public bool InterectsWith(TransformableBounds otherBounds)
    {
        TransformableBounds transformedBounds = GetTransformedBounds();
        return transformedBounds.Intersects(otherBounds);
    }

    // Returns spawnable if successfully spawned
    public Spawnable SpawnWithBoundsCheck(ReadOnlyCollection<TransformableBounds> spawnedBounds)
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
                    foreach (TransformableBounds bounds in spawnedBounds)
                    {
                        if (spawnable.InterectsWith(bounds))
                        {
                            intersectionFound = true;
                            Destroy(spawnedObject);
                        }

                        foreach(TransformableBounds spawnerBound in spawnable.GetSpawnerBounds())
                        {
                            if (spawnerBound.Intersects(bounds))
                            {
                                intersectionFound = true;
                                Destroy(spawnedObject);
                                break;
                            }
                        }

                        if (intersectionFound)
                            break;
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

        TransformableBounds transformedBounds = GetTransformedBounds();
        Bounds drawnBounds = transformedBounds.bounds;
        if (drawnBounds.size.x < 0 || drawnBounds.size.y < 0 || drawnBounds.size.z < 0)
        {
            Gizmos.color = Color.Lerp(Color.white, Color.red, 0.75f);
        }
        else
        {
            Gizmos.color = Color.cyan;
        }
        Gizmos.matrix = transformedBounds.transformationMatrix;
        Gizmos.DrawWireCube(drawnBounds.center, drawnBounds.size);
        Gizmos.matrix = Matrix4x4.identity;
    }
    #endif
}
