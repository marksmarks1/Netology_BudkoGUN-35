using UnityEngine;

/// Хранит исходное положение кегли и сообщает, упала ли она.
public class Pin : MonoBehaviour
{
    Vector3 startPos;
    Quaternion startRot;
    Rigidbody rb;
    const float knockedAngle = 35f; 

    void Awake()
    {
        startPos = transform.position;
        startRot = transform.rotation;
        rb = GetComponent<Rigidbody>();
    }

    public bool IsKnocked() =>
        Vector3.Angle(transform.up, Vector3.up) > knockedAngle;

    public void ResetPose()
    {
        rb.velocity = rb.angularVelocity = Vector3.zero;
        transform.SetPositionAndRotation(startPos, startRot);
    }
}
