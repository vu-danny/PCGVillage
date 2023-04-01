using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Generator : Spawner
{
    private Queue<Spawner> mainSpawnersQueue;
    private Queue<Spawner> optionalSpawnersQueue;
    private List<TransformableBounds> spawnedBounds;
    private Dictionary<Spawner, TransformableBounds> spawnerBounds;
    private List<Spawner> subSpawnersContainer;
    private List<Spawner> optionalSpawnersContainer;
    
    [SerializeField] private bool refreshResult = true;

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

                // Attempt to spawn remaining prefabs
                success = SpawnRemainingPrefabs(initialSpawnable);
                if(!success)
                {
                    Debug.Log("fail");
                }
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
                //EnqueueSpawners(currentSpawnable);
                currentSpawnable.ChangeChildrenParent(transform);
                Destroy(currentSpawnable.gameObject);
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

    private void EnqueueSpawners(Spawnable spawnable)
    {
        subSpawnersContainer.Clear();
        spawnable.GetSubSpawnersInRandomOrder(subSpawnersContainer);

        optionalSpawnersContainer.Clear();

        spawnable.GetOptionalSpawnersInRandomOrder(optionalSpawnersContainer);


        foreach (Spawner subSpawner in subSpawnersContainer){
            mainSpawnersQueue.Enqueue(subSpawner);
            spawnerBounds.Add(subSpawner, subSpawner.GetTransformedBounds());
        }

        foreach (Spawner optionalSpawner in optionalSpawnersContainer){
            optionalSpawnersQueue.Enqueue(optionalSpawner);
        }
    }
}
