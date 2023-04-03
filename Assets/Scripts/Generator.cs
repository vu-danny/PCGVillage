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
    
    [SerializeField] private bool refreshResult = true;
    [SerializeField] private UnityEvent onFinished; 

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
        bool success = false;

        yield return new WaitForSeconds(2.0f);
        GameObject initialObject = null;
        while (true)
        {
            while (refreshResult || !success)
            {
                refreshResult = false;

                // Retrieve and spawn initial spawnable   
                ResetPrefabs();
                GameObject initialPrefab = GetRandomSpawnablePrefab();
                remainingPrefabs.Clear();
                
                if (initialObject != null)
                    Destroy(initialObject);
                initialObject = Instantiate(initialPrefab);
                Spawnable initialSpawnable = initialObject.GetComponent<Spawnable>();
                initialSpawnable.AlignUsingRandomAnchor(transform);
                
                /*
                // Attempt to spawn remaining prefabs
                success = SpawnRemainingPrefabs(initialSpawnable);
                if(!success)
                {
                    Debug.Log("fail");
                }
                */
                yield return SpawnRemainingPrefabsEnumerator(initialSpawnable);
                success = true;
                onFinished.Invoke();
                yield return null;
            }
            yield return null;
        }
    }

    private bool SpawnRemainingPrefabs(Spawnable initialSpawnable)
    {
        ClearChildren();

        spawnedBounds.Clear();
        
        mainSpawnersQueue.Clear();
        optionalSpawnersQueue.Clear();

        spawnerBounds.Clear();

        spawnedBounds.AddRange(initialSpawnable.GetTransformedBounds());
        EnqueueSpawners(initialSpawnable);
    
        List<TransformableBounds> boundsToCheck = new List<TransformableBounds>();

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

        while (optionalSpawnersQueue.Count > 0)
        {
            Spawner currentSpawner = optionalSpawnersQueue.Dequeue();

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
            }
        }

        return true;
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
            else 
            {
#if UNITY_EDITOR
                foreach(TransformableBounds bounds in boundsToCheck)
                {
                    bounds.DrawDebug(Color.red, 3.0f);
                }
                currentSpawner.GetTransformedBounds().DrawDebug(Color.blue, 3.0f);
#endif
                yield return new WaitForSeconds(3.0f);
            }
        }

        int optionalYieldCounter = 7;
        while (optionalSpawnersQueue.Count > 0)
        {
            Spawner currentSpawner = optionalSpawnersQueue.Dequeue();

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
                    optionalYieldCounter = Random.Range(5, 10);
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
