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
}
