using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : Spawner
{
    private Queue<Spawner> spawnersQueue;
    private List<TransformableBounds> spawnedBounds;
    private Dictionary<Spawner, TransformableBounds> spawnerBounds;
    private List<Spawner> subSpawnersContainer;
    
    [SerializeField] private bool refreshResult = true;

    protected override void Awake() 
    {
        base.Awake();
        spawnersQueue = new Queue<Spawner>();
        spawnedBounds = new List<TransformableBounds>();
        subSpawnersContainer = new List<Spawner>();
        refreshResult = true;
        spawnerBounds = new Dictionary<Spawner, TransformableBounds>();
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
                if(!success)
                {
                    Debug.Log("fail");
                }
                success = false;
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
        spawnerBounds.Clear();

        spawnedBounds.AddRange(initialSpawnable.GetTransformedBounds());
        EnqueueSubSpawners(initialSpawnable);
    
        List<TransformableBounds> boundsToCheck = new List<TransformableBounds>();

        while (spawnersQueue.Count > 0)
        {
            Spawner currentSpawner = spawnersQueue.Dequeue();
            spawnerBounds.Remove(currentSpawner);

            boundsToCheck.Clear();
            boundsToCheck.AddRange(spawnedBounds);
            boundsToCheck.AddRange(spawnerBounds.Values);

            currentSpawner.ResetPrefabs();
            Spawnable currentSpawnable = currentSpawner.SpawnWithBoundsCheck(boundsToCheck.AsReadOnly());

            if (currentSpawnable != null)
            {
                spawnedBounds.AddRange(currentSpawnable.GetTransformedBounds());
                
                EnqueueSubSpawners(currentSpawnable);
                currentSpawnable.ChangeChildrenParent(transform);
                Destroy(currentSpawnable.gameObject);
                //yield return null;
            }
            else 
            {
#if UNITY_EDITOR
                foreach(TransformableBounds bounds in boundsToCheck)
                {
                    bounds.DrawDebug(Color.red, 3.0f);
                }
                currentSpawner.GetTransformedBounds().DrawDebug(Color.blue, 3.0f);
#endif
                return false;
                //yield return new WaitForSeconds(3.0f);
            }
        }

        return true;
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

        foreach (Spawner subSpawner in subSpawnersContainer){
            spawnersQueue.Enqueue(subSpawner);
            spawnerBounds.Add(subSpawner, subSpawner.GetTransformedBounds());
        }
    }
}
