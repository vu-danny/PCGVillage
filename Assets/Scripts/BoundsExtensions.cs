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

        public static Bounds TransformBounds(this Bounds bounds, Matrix4x4 transformation)
        {
            return GeometryUtility.CalculateBounds(bounds.GetCorners(), transformation);
        }
    }
}

