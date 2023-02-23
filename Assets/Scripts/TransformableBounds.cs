using UnityEngine;
using Utility.UnityExtensions;

public struct TransformableBounds
{
    public Bounds bounds;
    public Matrix4x4 transformationMatrix;

    public TransformableBounds(Bounds bounds)
    {
        this.bounds = bounds;
        this.transformationMatrix = Matrix4x4.identity;
    }

    public TransformableBounds(Bounds bounds, Matrix4x4 transformationMatrix)
    {
        this.bounds = bounds;
        this.transformationMatrix = transformationMatrix;
    }

    public TransformableBounds(Bounds bounds, Transform transform)
    {
        this.bounds = bounds;
        this.transformationMatrix = transform.localToWorldMatrix;
    }

    public TransformableBounds GetTransformed(Transform transform)
    {
        return new TransformableBounds(bounds, transformationMatrix * transform.localToWorldMatrix);
    }

    public TransformableBounds GetTransformed(Matrix4x4 transformationMatrix)
    {
        return new TransformableBounds(bounds, this.transformationMatrix * transformationMatrix);
    }

    public bool Intersects(TransformableBounds other)
    {
        return this.bounds.Intersects(transformationMatrix, other.bounds, other.transformationMatrix);
    }

    #if UNITY_EDITOR
    public void DrawDebug(Color color, float duration)
    {
        Vector3[] corners = bounds.GetCorners();
        for (int i = 0; i < corners.Length; i++)
        {
            for (int j = i; j < corners.Length; j++)
            {
                Debug.DrawLine(transformationMatrix.MultiplyPoint(corners[i]), transformationMatrix.MultiplyPoint(corners[j]), color, duration);
            }
        }
    }
    #endif
}
