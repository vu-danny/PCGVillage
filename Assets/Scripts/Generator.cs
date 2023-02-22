using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utility.UnityExtensions;

public class Generator : Spawner
{
    private Queue<Spawner> spawnersQueue;
    private List<Bounds> spawnedBounds;
    private List<Spawner> subSpawnersContainer;
    
    [SerializeField] private bool refreshResult = true;

    protected override void Awake() 
    {
        base.Awake();
        spawnersQueue = new Queue<Spawner>();
        spawnedBounds = new List<Bounds>();
        subSpawnersContainer = new List<Spawner>();
        refreshResult = true;
    }

    private IEnumerator Start()
    {
        // Retrieve and spawn initial spawnable   
        ResetPrefabs();
        GameObject initialPrefab = GetRandomSpawnablePrefab();
        remainingPrefabs.Clear();
        
        GameObject initialObject = Instantiate(initialPrefab);
        Spawnable initialSpawnable = initialObject.GetComponent<Spawnable>();
        initialSpawnable.PlaceRandomAnchorRelativeTo(transform);

        // "Consume" remaining spawners
        bool success = false;
        while (true)
        {
            while (refreshResult || !success)
            {
                refreshResult = false;
                success = SpawnRemainingPrefabs(initialSpawnable);
                yield return null;
            }
            yield return null;
        }
        // initialSpawnable.ChangeChildrenParent(transform);
        // Destroy(initialObject);
    }

    private bool SpawnRemainingPrefabs(Spawnable initialSpawnable)
    {
        ClearChildren();

        spawnedBounds.Clear();
        spawnersQueue.Clear();

        spawnedBounds.Add(initialSpawnable.GetTransformedBounds());
        EnqueueSubSpawners(initialSpawnable);
        
        while (spawnersQueue.Count > 0)
        {
            Spawner currentSpawner = spawnersQueue.Dequeue();
            currentSpawner.ResetPrefabs();
            Spawnable currentSpawnable = currentSpawner.SpawnWithBoundsCheck(spawnedBounds.AsReadOnly());

            if (currentSpawnable != null)
            {
                spawnedBounds.Add(currentSpawnable.GetTransformedBounds());
                EnqueueSubSpawners(currentSpawnable);
                currentSpawnable.ChangeChildrenParent(transform);
                Destroy(currentSpawnable.gameObject);
            }

            if (currentSpawnable == null)
                return false;
        }

        return true;
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

    private void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
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
