using UnityEngine;

// Базовый MonoBehaviour, чтобы его можно было "перетаскивать" в инспекторе.
public abstract class ItemProvider : MonoBehaviour, IItemProvider
{
    public abstract bool TryGetNearest(Vector3 origin, float radius, out Transform item);
}
