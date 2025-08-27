using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform target;
    public float height = 40f;
    public bool rotateWithTarget = true;
    public float smooth = 10f;

    void LateUpdate()
    {
        if (!target) return;
        var to = target.position + Vector3.up * height;
        transform.position = Vector3.Lerp(transform.position, to, Time.deltaTime * smooth);
        transform.rotation = Quaternion.Euler(90f, rotateWithTarget ? target.eulerAngles.y : 0f, 0f);
    }
}
