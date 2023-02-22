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
        // TODO : implement
        return false;
    }
    
    public bool InterectsWith(Bounds otherBounds)
    {
        return bounds.Intersects(otherBounds);
    }

    #if UNITY_EDITOR
    private void OnDrawGizmos() 
    {
        if (bounds.size.x < 0 || bounds.size.y < 0 || bounds.size.z < 0)
        {
            Gizmos.color = Color.Lerp(Color.white, Color.red, 0.75f);
        }
        else
        {
            Gizmos.color = Color.white;
        }
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
    #endif
}
