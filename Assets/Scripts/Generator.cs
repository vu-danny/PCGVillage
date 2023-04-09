using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Generator : Spawner
{
    private Queue<Spawner> mainSpawnersQueue;
    public int MainSpawnersQueueLength { get => mainSpawnersQueue.Count; }
    private Queue<Spawner> optionalSpawnersQueue;
    public int OptionalSpawnersQueueLength { get => optionalSpawnersQueue.Count; }
    private List<TransformableBounds> spawnedBounds;
    private Dictionary<Spawner, TransformableBounds> spawnerBounds;
    private List<Spawner> subSpawnersContainer;
    private List<Spawner> optionalSpawnersContainer;
    
    public int decorationIterationsBeforeYield = 1; 
    public bool useSeed = false;
    public int seed = 0;
    public bool refreshResult = true;
    public UnityEvent onFinished;

    protected override void Awake() 
    {
        base.Awake();
        mainSpawnersQueue = new Queue<Spawner>();
        optionalSpawnersQueue = new Queue<Spawner>();
        spawnedBounds = new List<TransformableBounds>();
        subSpawnersContainer = new List<Spawner>();
        optionalSpawnersContainer = new List<Spawner>();
        refreshResult = true;
        spawnerBounds = new Dictionary<Spawner, TransformableBounds>();
    }

    private IEnumerator Start()
    {
        // "Consume" remaining spawners
        yield return new WaitForSeconds(2.0f);
        GameObject initialObject = null;
        
        while (true)
        {
            while (refreshResult)
            {
                refreshResult = false;

                if (useSeed)
                    Random.InitState(seed);

                // Retrieve and spawn initial spawnable
                ResetPrefabs();
                GameObject initialPrefab = GetRandomSpawnablePrefab();
                remainingPrefabs.Clear();
                
                if (initialObject != null)
                    Destroy(initialObject);
                initialObject = Instantiate(initialPrefab);
                Spawnable initialSpawnable = initialObject.GetComponent<Spawnable>();
                initialSpawnable.AlignUsingRandomAnchor(transform);

                yield return SpawnRemainingPrefabsEnumerator(initialSpawnable);
                onFinished.Invoke();
                yield return null;
            }
            yield return null;
        }
    }


    private IEnumerator SpawnRemainingPrefabsEnumerator(Spawnable initialSpawnable)
    {
        ClearChildren();

        spawnedBounds.Clear();
        
        mainSpawnersQueue.Clear();
        optionalSpawnersQueue.Clear();

        spawnerBounds.Clear();

        spawnedBounds.AddRange(initialSpawnable.GetTransformedBounds());
        EnqueueSpawners(initialSpawnable);
    
        List<TransformableBounds> boundsToCheck = new List<TransformableBounds>();

        yield return null;
        while (mainSpawnersQueue.Count > 0)
        {
            Spawner currentSpawner = mainSpawnersQueue.Dequeue();
            spawnerBounds.Remove(currentSpawner);

            boundsToCheck.Clear();
            boundsToCheck.AddRange(spawnedBounds);
            boundsToCheck.AddRange(spawnerBounds.Values);

            currentSpawner.ResetPrefabs();
            Spawnable currentSpawnable = currentSpawner.SpawnWithBoundsCheck(boundsToCheck.AsReadOnly());

            Destroy(currentSpawner.gameObject);

            if (currentSpawnable != null)
            {
                spawnedBounds.AddRange(currentSpawnable.GetTransformedBounds());
                EnqueueSpawners(currentSpawnable);
                currentSpawnable.ChangeChildrenParent(transform);
                Destroy(currentSpawnable.gameObject);
                yield return null;
            }
        }

        int optionalYieldCounter = decorationIterationsBeforeYield;
        while (optionalSpawnersQueue.Count > 0)
        {
            Spawner currentSpawner = optionalSpawnersQueue.Dequeue();
            if (currentSpawner == null)
                continue;

            boundsToCheck.Clear();
            boundsToCheck.AddRange(spawnedBounds);

            currentSpawner.ResetPrefabs();
            Spawnable currentSpawnable = currentSpawner.SpawnWithBoundsCheck(boundsToCheck.AsReadOnly());
            
            Destroy(currentSpawner.gameObject);

            if (currentSpawnable != null)
            {
                spawnedBounds.AddRange(currentSpawnable.GetTransformedBounds());
                EnqueueSpawners(currentSpawnable, true);
                currentSpawnable.ChangeChildrenParent(transform);
                Destroy(currentSpawnable.gameObject);
                optionalYieldCounter--;
                if (optionalYieldCounter <= 0)
                {
                    optionalYieldCounter = decorationIterationsBeforeYield;
                    yield return null;
                }
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

    private void EnqueueSpawners(Spawnable spawnable, bool onlyOptional = false)
    {
        optionalSpawnersContainer.Clear();
        spawnable.GetOptionalSpawnersInRandomOrder(optionalSpawnersContainer);

        foreach (Spawner optionalSpawner in optionalSpawnersContainer){
            optionalSpawnersQueue.Enqueue(optionalSpawner);
        }

        if (onlyOptional)
            return;
        
        subSpawnersContainer.Clear();
        spawnable.GetSubSpawnersInRandomOrder(subSpawnersContainer);

        foreach (Spawner subSpawner in subSpawnersContainer){
            mainSpawnersQueue.Enqueue(subSpawner);
            spawnerBounds.Add(subSpawner, subSpawner.GetTransformedBounds());
        }
    }
}
