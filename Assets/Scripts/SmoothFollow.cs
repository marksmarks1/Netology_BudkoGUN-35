using UnityEngine;

public class SmoothFollow : MonoBehaviour
{
    [SerializeField]              
    private Transform target;

    [Header("ѕозици€ относительно цели")]
    [SerializeField] private Vector3 offset = new(0f, 2.5f, -5f);

    [Header("—глаживание")]
    [SerializeField, Min(0.1f)] private float smooth = 5f;

    public Transform Target => target;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPos = target.position + target.rotation * offset;

        transform.position = Vector3.Lerp(transform.position,
                                          desiredPos,
                                          smooth * Time.deltaTime);

        // смотрим чуть выше центра персонажа
        transform.LookAt(target.position + Vector3.up);
    }
}