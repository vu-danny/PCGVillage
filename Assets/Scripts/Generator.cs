using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utility.UnityExtensions;

public class Generator : Spawner
{
    private Queue<Spawner> spawnersQueue;
    private List<Bounds> spawnedBounds;
    private List<Spawner> subSpawnersContainer;

    protected override void Awake() 
    {
        base.Awake();
        spawnersQueue = new Queue<Spawner>();
        spawnedBounds = new List<Bounds>();
        subSpawnersContainer = new List<Spawner>();
    }

    private IEnumerator Start()
    {
        // Retrieve and spawn initial spawnable   
        GameObject initialPrefab = GetRandomSpawnablePrefab();
        GameObject initialObject = Instantiate(initialPrefab);
        Spawnable initialSpawnable = initialObject.GetComponent<Spawnable>();
        initialSpawnable.PlaceRandomAnchorRelativeTo(transform);
        spawnedBounds.Add(initialSpawnable.GetTransformedBounds());
        EnqueueSubSpawners(initialSpawnable);
        
        yield return new WaitForSeconds(0.1f);

        // "Consume" remaining spawners
        while (spawnersQueue.Count > 0)
        {
            Spawner currentSpawner = spawnersQueue.Dequeue();
            Spawnable currentSpawnable = currentSpawner.SpawnWithBoundsCheck(spawnedBounds.AsReadOnly());
            if (currentSpawnable != null)
            {
                spawnedBounds.Add(currentSpawnable.GetTransformedBounds());
                EnqueueSubSpawners(currentSpawnable);
                yield return new WaitForSeconds(0.1f);
            }
        }

        Debug.Log("DONE");
    }

    private void DrawBounds(Bounds bounds)
    {
        Vector3[] corners = bounds.GetCorners();
        for (int i = 0; i < corners.Length; i++)
        {
            for (int j = i; j < corners.Length; j++)
            {
                Debug.DrawLine(corners[i], corners[j], Color.red, 1.0f);
            }
        }
    }

    private void EnqueueSubSpawners(Spawnable spawnable)
    {
        subSpawnersContainer.Clear();
        spawnable.GetSubSpawnersInRandomOrder(subSpawnersContainer);

        foreach (Spawner subSpawner in subSpawnersContainer)
            spawnersQueue.Enqueue(subSpawner);
    }
}
