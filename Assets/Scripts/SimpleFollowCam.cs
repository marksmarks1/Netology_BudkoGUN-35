using UnityEngine;

public class SimpleFollowCam : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0, 7f, -8f);
    [SerializeField] private float smooth = 5f;

    private void LateUpdate()
    {
        if (!target) return;
        Vector3 desired = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desired, smooth * Time.deltaTime);
        transform.LookAt(target.position);
    }

    public void SetTarget(Transform t) => target = t;
}
