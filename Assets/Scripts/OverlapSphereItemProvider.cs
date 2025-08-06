using UnityEngine;

public class OverlapSphereItemProvider : ItemProvider
{
    [SerializeField] private LayerMask itemMask;

    public override bool TryGetNearest(Vector3 origin, float radius, out Transform item)
    {
        item = null;
        Collider[] hits = Physics.OverlapSphere(origin, radius, itemMask);
        if (hits == null || hits.Length == 0) return false;

        Transform best = null;
        float bestDistSq = float.MaxValue;

        foreach (var c in hits)
        {
            float d = (c.transform.position - origin).sqrMagnitude;
            if (d < bestDistSq)
            {
                bestDistSq = d;
                best = c.transform;
            }
        }

        if (best == null) return false;
        item = best;
        return true;
    }
}
