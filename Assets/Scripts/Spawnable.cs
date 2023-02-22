using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

using Utility.UnityExtensions;

public class Spawnable : MonoBehaviour
{
    [Header("Positioning")]
    [SerializeField] protected Bounds bounds;
    [SerializeField] protected float boundsInnerMargin = 1e-3f;
    [SerializeField] protected List<Transform> anchors;

    [Header("Spawners")]
    [SerializeField] protected List<Spawner> subSpawners;
    [SerializeField] protected List<Spawner> decorationSpawners;
    public ReadOnlyCollection<Spawner> DecorationSpawners { get => decorationSpawners.AsReadOnly(); }

    // Returns whether an anchor was found and was used for placement
    public bool PlaceRandomAnchorRelativeTo(Transform referenceTransform)
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
    
    public bool InterectsWith(Bounds otherBounds)
    {
        return GetTransformedBounds().Intersects(otherBounds);
    }

    public Bounds GetTransformedBounds()
    {
        Bounds transformedBounds = bounds.TransformBounds(Matrix4x4.TRS(
                transform.position, 
                transform.rotation, 
                Vector3.one
            ));
        transformedBounds.extents -= Vector3.one * boundsInnerMargin;
        return transformedBounds;
    }

    public void GetSubSpawnersInRandomOrder(ICollection<Spawner> resultsContainer)
    {
        List<int> indicesToRetrieve = new List<int>(subSpawners.Count);
        for (int i = 0; i < subSpawners.Count; i++)
            indicesToRetrieve.Add(i);

        while (indicesToRetrieve.Count > 0)
        {
            int randomIndex = Random.Range(0, indicesToRetrieve.Count);
            resultsContainer.Add(subSpawners[indicesToRetrieve[randomIndex]]);
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
        Bounds drawnBounds = GetTransformedBounds();
        if (drawnBounds.size.x < 0 || drawnBounds.size.y < 0 || drawnBounds.size.z < 0)
        {
            Gizmos.color = Color.Lerp(Color.white, Color.red, 0.75f);
        }
        else
        {
            Gizmos.color = Color.white;
        }
        Gizmos.DrawWireCube(drawnBounds.center, drawnBounds.size);
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
