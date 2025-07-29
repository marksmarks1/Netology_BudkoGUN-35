using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class VacuumRobotOptimized : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0f)] private float moveSpeed = 2f;
    [SerializeField, Range(120f, 360f)] private float rotationSpeed = 220f;

    [Header("Sensing")]
    [SerializeField, Min(0.1f)] private float rayDistance = 1.3f;
    [SerializeField, Range(10f, 60f)] private float whiskerAngle = 30f;
    [SerializeField, Min(0.5f)] private float whiskerDistance = 1.1f;
    [SerializeField, Range(0.05f, 0.5f)] private float bodyRadius = 0.22f;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Avoidance")]
    [Tooltip("Если спереди ближе этого порога — выполняем разворот")]
    [SerializeField, Range(0.2f, 0.8f)] private float avoidFrontAt = 0.45f;
    [Tooltip("Время активного поворота")]
    [SerializeField] private Vector2 turnTime = new Vector2(0.35f, 0.6f);

    [Header("Randomness")]
    [SerializeField, Range(0f, 0.1f)] private float smallJitterChance = 0.01f;
    [SerializeField, Range(0f, 30f)] private float smallJitterAngle = 15f;

    private Rigidbody rb;

    private enum State { Move, Turn, BackOff }
    private State state = State.Move;
    private float stateEndTime;
    private Quaternion targetRot;

    // NonAlloc буферы
    private readonly RaycastHit[] frontHits = new RaycastHit[4];
    private readonly RaycastHit[] whiskerHit = new RaycastHit[1];
    private readonly Collider[] overlap = new Collider[8];

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationZ |
                         RigidbodyConstraints.FreezePositionY;

        moveSpeed = Mathf.Max(0f, moveSpeed);
        rotationSpeed = Mathf.Clamp(rotationSpeed, 60f, 720f);
        rayDistance = Mathf.Max(0.1f, rayDistance);
        whiskerDistance = Mathf.Max(0.1f, whiskerDistance);
        bodyRadius = Mathf.Clamp(bodyRadius, 0.05f, 0.5f);
        avoidFrontAt = Mathf.Clamp(avoidFrontAt, 0.2f, 0.8f);
    }

    private void FixedUpdate()
    {
        switch (state)
        {
            case State.Move: MoveState(); break;
            case State.Turn: TurnState(); break;
            case State.BackOff: BackOffState(); break;
        }
    }

    private void MoveState()
    {
        Vector3 origin = transform.position + Vector3.up * 0.15f;

        int nFront = Physics.SphereCastNonAlloc(
            origin, bodyRadius * 0.9f, transform.forward,
            frontHits, rayDistance, obstacleMask, QueryTriggerInteraction.Ignore);

        bool frontTooClose = false;
        Vector3 frontNormal = Vector3.zero;
        float bestDist = float.MaxValue;

        for (int i = 0; i < nFront; i++)
        {
            if (frontHits[i].distance < bestDist)
            {
                bestDist = frontHits[i].distance;
                frontNormal = frontHits[i].normal;
            }
        }
        if (nFront > 0 && bestDist < avoidFrontAt)
            frontTooClose = true;

        // Боковые "усы" через RaycastNonAlloc
        Vector3 leftDir = Quaternion.Euler(0f, -whiskerAngle, 0f) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0f, whiskerAngle, 0f) * transform.forward;

        float leftDist = whiskerDistance;
        float rightDist = whiskerDistance;

        if (Physics.RaycastNonAlloc(origin, leftDir, whiskerHit, whiskerDistance, obstacleMask) > 0)
            leftDist = whiskerHit[0].distance;
        if (Physics.RaycastNonAlloc(origin, rightDir, whiskerHit, whiskerDistance, obstacleMask) > 0)
            rightDist = whiskerHit[0].distance;

        // Жёсткий разворот через Reflect, когда очень близко
        if (frontTooClose)
        {
            if (frontNormal == Vector3.zero)
            {
                int n = Physics.OverlapSphereNonAlloc(
                    origin + transform.forward * bestDist, bodyRadius, overlap, obstacleMask,
                    QueryTriggerInteraction.Ignore);

                if (n > 0)
                {
                    Vector3 cp = overlap[0].ClosestPoint(origin); // точка на препятствии
                    frontNormal = (origin - cp).normalized;       // приближённая нормаль
                    frontNormal.y = 0f;
                }
                else frontNormal = -transform.forward;
            }

            Vector3 reflect = Vector3.Reflect(transform.forward, frontNormal);
            reflect.y = 0f;
            if (reflect.sqrMagnitude < 1e-4f) reflect = Quaternion.Euler(0f, 90f, 0f) * transform.forward;

            targetRot = Quaternion.LookRotation(reflect, Vector3.up);
            stateEndTime = Time.time + Random.Range(turnTime.x, turnTime.y);
            rb.velocity = Vector3.zero;
            state = State.Turn;
            return;
        }

        // Мягкое руление без потери скорости
        float steerGain = 60f; // град/м разницы
        float diff = Mathf.Clamp(rightDist - leftDist, -whiskerDistance, whiskerDistance);
        float steerThisFrame = (diff / Mathf.Max(whiskerDistance, 0.01f)) * steerGain * Time.fixedDeltaTime;
        rb.MoveRotation(Quaternion.Euler(0f, steerThisFrame, 0f) * rb.rotation);

        if (Random.value < smallJitterChance)
        {
            float j = Random.Range(-smallJitterAngle, smallJitterAngle);
            rb.MoveRotation(Quaternion.Euler(0f, j, 0f) * rb.rotation);
        }

        // движение вперёд
        rb.MovePosition(rb.position + transform.forward * moveSpeed * Time.fixedDeltaTime);
    }

    private void TurnState()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));

        bool reached = Quaternion.Angle(rb.rotation, targetRot) < 1f;
        if (reached || Time.time >= stateEndTime)
            state = State.Move;
    }

    private void BackOffState()
    {
        Vector3 backward = -transform.forward * moveSpeed * 0.75f;
        rb.MovePosition(rb.position + backward * Time.fixedDeltaTime);

        if (Time.time >= stateEndTime)
            state = State.Move;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & obstacleMask) != 0)
        {
            state = State.BackOff;
            stateEndTime = Time.time + 0.15f;

            // план на поворот после отката: отражаемся от предполагаемой нормали
            Vector3 n = collision.contacts.Length > 0 ? collision.contacts[0].normal : -transform.forward;
            Vector3 reflect = Vector3.Reflect(transform.forward, n);
            reflect.y = 0f;
            targetRot = Quaternion.LookRotation(reflect, Vector3.up);
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 origin = transform.position + Vector3.up * 0.15f;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin, origin + transform.forward * rayDistance);
        Gizmos.color = Color.yellow;
        Vector3 L = Quaternion.Euler(0f, -whiskerAngle, 0f) * transform.forward;
        Vector3 R = Quaternion.Euler(0f, whiskerAngle, 0f) * transform.forward;
        Gizmos.DrawLine(origin, origin + L * whiskerDistance);
        Gizmos.DrawLine(origin, origin + R * whiskerDistance);
    }
}