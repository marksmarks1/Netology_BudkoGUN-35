using UnityEngine;

public interface IItemProvider
{
    // Ищет ближайший собираемый объект к origin в пределах radius.
    // Возвращает true/false и даёт Transform найденной цели.
    bool TryGetNearest(Vector3 origin, float radius, out Transform item);
}
