using UnityEngine;
using System.Collections.Generic;

namespace Utility.UnityExtensions
{
    public static class BoundsExtensions
    {
        public static Vector3[] GetCorners(this Bounds bounds)
        {
            Vector3[] minMax = { bounds.min, bounds.max }; 

            Vector3[] corners = new Vector3[2*2*2];
            for (int z = 0; z < 2; z++)
                for (int y = 0; y < 2; y++) 
                    for (int x = 0; x < 2; x++)
                        corners[z*2*2 + y*2 + x] = new Vector3(
                            minMax[x].x,
                            minMax[y].y,
                            minMax[z].z
                        );

            return corners;
        }

        public static bool GetCorners(this Bounds bounds, Vector3[] corners)
        {
            if (corners.Length < 8)
                return false;

            Vector3[] minMax = { bounds.min, bounds.max }; 
            
            for (int z = 0; z < 2; z++)
                for (int y = 0; y < 2; y++) 
                    for (int x = 0; x < 2; x++)
                        corners[z*2*2 + y*2 + x] = new Vector3(
                            minMax[x].x,
                            minMax[y].y,
                            minMax[z].z
                        );

            return true;
        }

        public static void GetCorners(this Bounds bounds, List<Vector3> corners, bool append)
        {
            Vector3[] minMax = { bounds.min, bounds.max };
            
            for (int z = 0; z < 2; z++)
                for (int y = 0; y < 2; y++) 
                    for (int x = 0; x < 2; x++)
                    {
                        if (append || corners.Count < 8)
                        {
                            corners.Add(new Vector3(
                                minMax[x].x,
                                minMax[y].y,
                                minMax[z].z
                            ));
                        }
                        else
                        {
                            corners[z*2*2 + y*2 + x] = new Vector3(
                                minMax[x].x,
                                minMax[y].y,
                                minMax[z].z
                            );
                        }
                    }
        }
        
        // Mostly taken from https://answers.unity.com/questions/1840803/calculate-collision-between-2-rotated-boxes-withou.html
        public static bool Intersects(this Bounds bounds, Matrix4x4 transformation, Bounds other, Matrix4x4 otherTransformation)
        {
            TwoBoxes data = new TwoBoxes();
            data.A.pos = transformation.MultiplyPoint(bounds.center);
            data.A.n1 = transformation.MultiplyVector(Vector3.right * bounds.extents.x);
            data.A.n2 = transformation.MultiplyVector(Vector3.up * bounds.extents.y);
            data.A.n3 = transformation.MultiplyVector(Vector3.forward * bounds.extents.z);
            data.B.pos = otherTransformation.MultiplyPoint(other.center);
            data.B.n1 = otherTransformation.MultiplyVector(Vector3.right * other.extents.x);
            data.B.n2 = otherTransformation.MultiplyVector(Vector3.up * other.extents.y);
            data.B.n3 = otherTransformation.MultiplyVector(Vector3.forward * other.extents.z);

            if (data.SAT(data.A.n1)) return false;
            if (data.SAT(data.A.n2)) return false;
            if (data.SAT(data.A.n3)) return false;
            if (data.SAT(data.B.n1)) return false;
            if (data.SAT(data.B.n2)) return false;
            if (data.SAT(data.B.n3)) return false;

            if (data.SAT(Vector3.Cross(data.A.n1, data.B.n1))) return false;
            if (data.SAT(Vector3.Cross(data.A.n1, data.B.n2))) return false;
            if (data.SAT(Vector3.Cross(data.A.n1, data.B.n3))) return false;
            if (data.SAT(Vector3.Cross(data.A.n2, data.B.n1))) return false;
            if (data.SAT(Vector3.Cross(data.A.n2, data.B.n2))) return false;
            if (data.SAT(Vector3.Cross(data.A.n2, data.B.n3))) return false;
            if (data.SAT(Vector3.Cross(data.A.n3, data.B.n1))) return false;
            if (data.SAT(Vector3.Cross(data.A.n3, data.B.n2))) return false;
            if (data.SAT(Vector3.Cross(data.A.n3, data.B.n3))) return false;

            return true;
        }

        public static bool Intersects(this Bounds bounds, Transform transform, Bounds other, Transform otherTransform, bool ignoreScale = false)
        {
            if (ignoreScale)
            {
                return bounds.Intersects(
                    Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one),
                    other,
                    Matrix4x4.TRS(otherTransform.position, otherTransform.rotation, Vector3.one)
                );
            }
            else
            {
                return bounds.Intersects(transform.localToWorldMatrix, other, otherTransform.localToWorldMatrix);
            }
        }

        private struct IntersectionTestBox
        {
            public Vector3 pos, n1, n2, n3;
            public float min, max;

            private void UpdateMinMax(Vector3 aPos, ref Vector3 aNormal)
            {
                float p = Vector3.Dot(aPos, aNormal);
                if (p > max) max = p;
                if (p < min) min = p;
            }

            public void GetMinMax(ref Vector3 aAxis)
            {
                min = float.PositiveInfinity;
                max = float.NegativeInfinity;
                UpdateMinMax(pos + n1 + n2 + n3, ref aAxis);
                UpdateMinMax(pos + n1 + n2 - n3, ref aAxis);
                UpdateMinMax(pos + n1 - n2 + n3, ref aAxis);
                UpdateMinMax(pos + n1 - n2 - n3, ref aAxis);
                UpdateMinMax(pos - n1 + n2 + n3, ref aAxis);
                UpdateMinMax(pos - n1 + n2 - n3, ref aAxis);
                UpdateMinMax(pos - n1 - n2 + n3, ref aAxis);
                UpdateMinMax(pos - n1 - n2 - n3, ref aAxis);
            }
        }
    
        private struct TwoBoxes
        {
            public IntersectionTestBox A;
            public IntersectionTestBox B;

            public bool SAT(Vector3 aAxis)
            {
                A.GetMinMax(ref aAxis);
                B.GetMinMax(ref aAxis);
                return A.min > B.max || B.min > A.max;
            }
        }
    }
}
