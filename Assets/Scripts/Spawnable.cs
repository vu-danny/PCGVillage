using System.Collections.Generic;
using UnityEngine;

public class Spawnable : MonoBehaviour
{
    [Header("Positioning")]
    [SerializeField] protected Transform[] boundsParts;
    [SerializeField] protected float boundsInnerMargin = 1e-3f;
    [SerializeField] protected List<Transform> anchors;

    [Header("Spawners")]
    [SerializeField] protected List<Spawner> subSpawners;
    [SerializeField] protected List<Spawner> optionalSpawners;

    // Returns whether an anchor was found and was used for placement
    public bool AlignUsingRandomAnchor(Transform referenceTransform)
    {
        if (anchors.Count <= 0)
            return false;

        int anchorIndex = Random.Range(0, anchors.Count);
        Transform anchorTransform = anchors[anchorIndex];
        transform.rotation = referenceTransform.rotation * Quaternion.Inverse(anchorTransform.rotation);
        transform.position = referenceTransform.position-(anchorTransform.position - transform.position);
        anchors.RemoveAt(anchorIndex);
        
        return true;
    }
    
    public bool InterectsWith(TransformableBounds otherBounds)
    {
        if (boundsParts.Length == 0)
            return false;
        
        TransformableBounds[] transformedBounds = GetTransformedBounds();
        for(int i = 0; i<transformedBounds.Length; i++)
        {
            if (transformedBounds[i].Intersects(otherBounds))
                return true;
        }
        return false;
    }

    public TransformableBounds[] GetTransformedBounds()
    {
        TransformableBounds[] transformedBounds = new TransformableBounds[boundsParts.Length];
        Bounds baseBounds = new Bounds(Vector3.zero,Vector3.one);
        for(int i = 0; i < boundsParts.Length; i++)
        {
            Matrix4x4 boundsPartMatrix = Matrix4x4.TRS(boundsParts[i].position, boundsParts[i].rotation, boundsParts[i].lossyScale - Vector3.one*boundsInnerMargin);
            transformedBounds[i] = new TransformableBounds(baseBounds, boundsPartMatrix);
        }
        return transformedBounds;
    }

    public List<TransformableBounds> GetSpawnerBounds()
    {
        List<TransformableBounds> subSpawnerBounds = new List<TransformableBounds>();
        foreach(Spawner subSpawner in subSpawners)
        {
            subSpawnerBounds.Add(subSpawner.GetTransformedBounds());
        }
        return subSpawnerBounds;
    }

    public void GetSubSpawnersInRandomOrder(ICollection<Spawner> resultsContainer)
    {
        GetContainerInRandomOrder(subSpawners, resultsContainer);
    }

    public void GetOptionalSpawnersInRandomOrder(ICollection<Spawner> resultsContainer)
    {
        GetContainerInRandomOrder(optionalSpawners, resultsContainer);
    }

    private void GetContainerInRandomOrder(List<Spawner> sourceContainer, ICollection<Spawner> resultsContainer)
    {
        List<int> indicesToRetrieve = new List<int>(sourceContainer.Count);
        for (int i = 0; i < sourceContainer.Count; i++)
            indicesToRetrieve.Add(i);

        while (indicesToRetrieve.Count > 0)
        {
            int randomIndex = Random.Range(0, indicesToRetrieve.Count);
            resultsContainer.Add(sourceContainer[indicesToRetrieve[randomIndex]]);
            indicesToRetrieve.RemoveAt(randomIndex);
        }
    }

    public void ChangeChildrenParent(Transform newParent)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            transform.GetChild(i).parent = newParent;
        }
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos() 
    {
        TransformableBounds[] transformedBounds = GetTransformedBounds();
        for(int i = 0; i<transformedBounds.Length; i++){
            Bounds drawnBounds = transformedBounds[i].bounds;
            
            if (drawnBounds.size.x < 0 || drawnBounds.size.y < 0 || drawnBounds.size.z < 0)
            {
                Gizmos.color = Color.Lerp(Color.white, Color.red, 0.75f);
            }
            else
            {
                Gizmos.color = Color.white;
            }
            Gizmos.matrix = transformedBounds[i].transformationMatrix;
            Gizmos.DrawWireCube(drawnBounds.center, drawnBounds.size);
        }
        Gizmos.matrix = Matrix4x4.identity;
    }

    private void OnDrawGizmosSelected()
    {
        if (anchors == null)
            return;
        
        foreach (Transform anchor in anchors)
        {
            if (anchor == null)
                continue;
            float arrowSize = 0.25f;
            UnityEditor.Handles.color = Color.red;
            UnityEditor.Handles.ConeHandleCap(
                0, 
                anchor.position + anchor.forward * (arrowSize * 0.5f), 
                anchor.rotation, 
                arrowSize, 
                EventType.Repaint
            );
        }
    }
    #endif
}
