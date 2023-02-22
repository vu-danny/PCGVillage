using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Spawnable Set", fileName = "New Spawnable Set")]
public class SpawnableSet : ScriptableObject
{
    [SerializeField] private WeightedPrefab[] spawnablePrefabs;
    
    public void CopyTo(ICollection<WeightedPrefab> resultsContainer)
    {
        foreach (WeightedPrefab prefab in spawnablePrefabs)
        {
            resultsContainer.Add(prefab);
        }
    }
}

[System.Serializable]
public struct WeightedPrefab
{
    public GameObject prefab;
    public int weight;
}
