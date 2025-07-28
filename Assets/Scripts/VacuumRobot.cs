using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody))]
public class VacuumRobot : MonoBehaviour
{
    // ---------- Config (инспектор) ----------
    [Header("Movement")]
    [SerializeField, Min(0f)] private float moveSpeed = 2f;
    [SerializeField, Range(60f, 360f)] private float rotationSpeed = 200f;

    [Header("Sensing")]
    [Tooltip("Дистанция прямого луча (вперёд)")]
    [SerializeField, Min(0.1f)] private float rayDistance = 1.4f;

    [Tooltip("Угол наклонных 'усов' от направления вперёд")]
    [SerializeField, Range(10f, 60f)] private float whiskerAngle = 30f;

    [Tooltip("Дистанция наклонных лучей")]
    [SerializeField, Min(0.5f)] private float whiskerDistance = 1.1f;

    [Tooltip("Радиус корпуса для SphereCast")]
    [SerializeField, Range(0.05f, 0.5f)] private float bodyRadius = 0.22f;

    [Tooltip("Слой(и) препятствий (стены/мебель)")]
    [SerializeField] private LayerMask obstacleMask = ~0;

    [Header("Avoidance")]
    [Tooltip("Если препятствие спереди ближе этого порога — выполняем разворот")]
    [SerializeField, Range(0.2f, 0.8f)] private float avoidFrontAt = 0.45f;

    [Tooltip("Время активного поворота")]
    [SerializeField] private Vector2 turnTime = new Vector2(0.35f, 0.6f);

    [Tooltip("Небольшой откат при касании")]
    [SerializeField, Range(0f, 0.5f)] private float backOffDuration = 0.15f;

    [Header("Randomness")]
    [Tooltip("Вероятность небольшого случайного подруливания за кадр")]
    [SerializeField, Range(0f, 0.1f)] private float smallJitterChance = 0.01f;
    [Tooltip("Макс. угол случайного подруливания (градусы)")]
    [SerializeField, Range(0f, 30f)] private float smallJitterAngle = 15f;

    public float MoveSpeed => moveSpeed;
    public float RotationSpeed => rotationSpeed;

    private Rigidbody rb;

    private enum State { Move, Turn, BackOff }
    private State state = State.Move;

    private float stateEndTime;
    private float turnSign = 1f;   // -1 влево, +1 вправо
    private Quaternion targetRot;

    private void Reset()
    {
        obstacleMask = LayerMask.GetMask("Obstacles");
        if (TryGetComponent(out Rigidbody rigid))
        {
            rigid.interpolation = RigidbodyInterpolation.Interpolate;
            rigid.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rigid.constraints = RigidbodyConstraints.FreezeRotationX |
                                RigidbodyConstraints.FreezeRotationZ |
                                RigidbodyConstraints.FreezePositionY;
        }
    }

    private void OnValidate()
    {
        // Страхуемся от некорректных значений из инспектора
        if (turnTime.x < 0f) turnTime.x = 0f;
        if (turnTime.y < turnTime.x) turnTime.y = turnTime.x;
        moveSpeed = Mathf.Max(0f, moveSpeed);
        rayDistance = Mathf.Max(0.1f, rayDistance);
        whiskerDistance = Mathf.Max(0.1f, whiskerDistance);
        bodyRadius = Mathf.Clamp(bodyRadius, 0.05f, 0.5f);
        avoidFrontAt = Mathf.Clamp(avoidFrontAt, 0.2f, 0.8f);
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
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

        // Прямой "бампер" — шириной корпуса
        bool hitFront = Physics.SphereCast(origin, bodyRadius * 0.9f,
                                           transform.forward, out RaycastHit hitF,
                                           rayDistance, obstacleMask);

        // Наклонные "усы": измеряем фактические дистанции
        Vector3 leftDir = Quaternion.Euler(0f, -whiskerAngle, 0f) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0f, whiskerAngle, 0f) * transform.forward;

        float leftDist = whiskerDistance;
        float rightDist = whiskerDistance;

        if (Physics.Raycast(origin, leftDir, out RaycastHit leftHit, whiskerDistance, obstacleMask))
            leftDist = leftHit.distance;
        if (Physics.Raycast(origin, rightDir, out RaycastHit rightHit, whiskerDistance, obstacleMask))
            rightDist = rightHit.distance;

        // Жёсткий разворот, если спереди слишком близко
        if (hitFront && hitF.distance < avoidFrontAt)
        {
            turnSign = (rightDist > leftDist) ? +1f : -1f; // в сторону с большей свободой
            float angle = Random.Range(70f, 110f) * turnSign;
            targetRot = Quaternion.Euler(0f, angle, 0f) * rb.rotation;
            stateEndTime = Time.time + Random.Range(turnTime.x, turnTime.y);
            rb.velocity = Vector3.zero;
            state = State.Turn;
            return;
        }

        // Мягкое руление без потери скорости (держим зазор от стен)
        float steerGain = 60f; // град/м разницы
        float diff = Mathf.Clamp(rightDist - leftDist, -whiskerDistance, whiskerDistance);
        float steerThisFrame = (diff / Mathf.Max(whiskerDistance, 0.01f)) * steerGain * Time.fixedDeltaTime;
        rb.MoveRotation(Quaternion.Euler(0f, steerThisFrame, 0f) * rb.rotation);

        // Небольшая рандомизация траектории
        if (Random.value < smallJitterChance)
        {
            float small = Random.Range(-smallJitterAngle, smallJitterAngle);
            rb.MoveRotation(Quaternion.Euler(0f, small, 0f) * rb.rotation);
        }

        // Ровное поступательное движение
        rb.MovePosition(rb.position + transform.forward * moveSpeed * Time.fixedDeltaTime);
    }

    private void TurnState()
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));

        bool reached = Quaternion.Angle(rb.rotation, targetRot) < 1f;
        if (reached || Time.time >= stateEndTime)
        {
            state = State.Move;
        }
    }

    private void BackOffState()
    {
        // Небольшой откат назад, чтобы разорвать контакт
        Vector3 backward = -transform.forward * moveSpeed * 0.75f;
        rb.MovePosition(rb.position + backward * Time.fixedDeltaTime);

        if (Time.time >= stateEndTime)
        {
            state = State.Move;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsObstacle(collision.collider))
        {
            state = State.BackOff;
            stateEndTime = Time.time + backOffDuration;

            // запланируем поворот после отката
            float angle = Random.Range(80f, 120f) * ((Random.value < 0.5f) ? -1f : +1f);
            targetRot = Quaternion.Euler(0f, angle, 0f) * rb.rotation;
        }
    }

    private bool IsObstacle(Collider col)
    {
        return (obstacleMask.value & (1 << col.gameObject.layer)) != 0;
    }

    private void OnDrawGizmos()
    {
        Vector3 origin = transform.position + Vector3.up * 0.15f;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(origin, origin + transform.forward * rayDistance);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(origin, bodyRadius * 0.9f);

        Gizmos.color = Color.yellow;
        Vector3 leftDir = Quaternion.Euler(0f, -whiskerAngle, 0f) * transform.forward;
        Vector3 rightDir = Quaternion.Euler(0f, whiskerAngle, 0f) * transform.forward;
        Gizmos.DrawLine(origin, origin + leftDir * whiskerDistance);
        Gizmos.DrawLine(origin, origin + rightDir * whiskerDistance);
    }
}