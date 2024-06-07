using System.Collections.Generic;
using Unity.Physics;
using UnityEngine;

namespace KindredVignettes;
static class AabbHelper
{
    static public bool MatchesOnTwoAxesAndOverlaps(this Aabb thisOne, Aabb other, float tolerance=0.01f)
    {
        bool matchX = Mathf.Abs(thisOne.Min.x - other.Min.x) <= tolerance && Mathf.Abs(thisOne.Max.x - other.Max.x) <= tolerance;
        bool matchY = Mathf.Abs(thisOne.Min.y - other.Min.y) <= tolerance && Mathf.Abs(thisOne.Max.y - other.Max.y) <= tolerance;
        bool matchZ = Mathf.Abs(thisOne.Min.z - other.Min.z) <= tolerance && Mathf.Abs(thisOne.Max.z - other.Max.z) <= tolerance;

        if (matchX && matchY)
        {
            return (thisOne.Min.z <= other.Max.z && thisOne.Max.z >= other.Min.z);
        }
        if (matchX && matchZ)
        {
            return (thisOne.Min.y <= other.Max.y && thisOne.Max.y >= other.Min.y);
        }
        if (matchY && matchZ)
        {
            return (thisOne.Min.x <= other.Max.x && thisOne.Max.x >= other.Min.x);
        }

        return false;
    }

    static public void MergeAabbsTogether(List<Aabb> aabbs)
    {
        for(int i = 0; i < aabbs.Count - 1; i++)
        {
            for(int j = aabbs.Count - 1; j > i; j--)
            {
                if (aabbs[i].MatchesOnTwoAxesAndOverlaps(aabbs[j]))
                {
                    aabbs[i].Include(aabbs[j]);
                    aabbs.RemoveAt(j);
                }
                else if (aabbs[i].Contains(aabbs[j]))
                {
                    aabbs.RemoveAt(j);
                }
                else if (aabbs[j].Contains(aabbs[i]))
                {
                    aabbs[i] = aabbs[j];
                    aabbs.RemoveAt(j);
                }
            }
        }
    }
}
