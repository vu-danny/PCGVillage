using System.Collections.Generic;
using UnityEngine;

public class Spawnable : MonoBehaviour
{
    [SerializeField] protected Bounds bounds;
    [SerializeField] protected List<Transform> anchors;

    // Returns whether another anchor is available
    public bool PlaceRandomAnchorRelativeTo(Transform referenceTransform)
    {
        int anchorIndex = Random.Range(0, anchors.Count);
        Transform anchorTransform = anchors[anchorIndex].transform;
        transform.Rotate(referenceTransform.localEulerAngles - anchorTransform.eulerAngles, Space.Self);
        transform.position = referenceTransform.position-anchorTransform.localPosition;
        anchors.RemoveAt(anchorIndex);
        return anchors.Count > 0;
    }
    
    public bool InterectsWith(Bounds otherBounds)
    {
        return bounds.Intersects(otherBounds);
    }

    public Bounds GetTransformedBounds()
    {
        Bounds transformedBounds = bounds;
        transformedBounds.center += transform.position;
        return transformedBounds;
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
    #endif
}
