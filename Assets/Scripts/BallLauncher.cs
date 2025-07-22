using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class BallLauncher : MonoBehaviour
{
    [Header("Сила броска")]
    [Tooltip("Ньютоны на 1 пиксель перемещения мыши вперёд. 0.05-0.1 — реалистично.")]
    public float perPixelForce = 0.08f;

    [Tooltip("Сколько пикселей 'вперёд' максимум участвует в расчёте силы.")]
    public float maxForwardPixels = 500f;

    [Header("Детекция остановки / тайм-аут")]
    public float stopSpeed = 0.05f;      // м/с — считаем «лежит»
    public float stillTime = 1.2f;       // сколько секунд лежит
    public float timeoutAfter = 7f;      // макс. длительность броска

    // -------------------------------------------------------------------------
    Rigidbody rb;
    Vector3 dragStart;
    bool dragging;
    bool launched;

    void Awake() => rb = GetComponent<Rigidbody>();

    void Update()
    {
        // 1. начали тянуть
        if (Input.GetMouseButtonDown(0) && !launched)
        {
            dragging = true;
            dragStart = Input.mousePosition;
        }

        // 2. отпустили мышь → бросок
        if (Input.GetMouseButtonUp(0) && dragging)
        {
            dragging = false;

            Vector3 delta = Input.mousePosition - dragStart;

            // a) сила вперёд (Z) — только из компоненты Y
            float forwardPx = Mathf.Clamp(delta.y, -maxForwardPixels, maxForwardPixels);
            Vector3 force = new Vector3(          // вбок = X, вперёд = Y
                delta.x * perPixelForce,
                0,
                forwardPx * perPixelForce);

            rb.AddForce(force, ForceMode.Impulse);

            launched = true;
            StartCoroutine(MonitorFlight());
        }
    }

    IEnumerator MonitorFlight()
    {
        float stillT = 0f;
        float totalT = 0f;
        float maxT = timeoutAfter;

        float laneHalf = 5f;
        bool killSide = Mathf.Abs(transform.position.x) > laneHalf;

        yield return new WaitForSeconds(0.3f); // время на разгон

        while (true)
        {
            if (rb.velocity.sqrMagnitude <= stopSpeed * stopSpeed)
                stillT += Time.deltaTime;
            else
                stillT = 0f;

            totalT += Time.deltaTime;

            bool tooLong = totalT >= maxT;
            bool stillLong = stillT >= stillTime;

            // Килл-зона — шар упал или улетел
            bool killFall = transform.position.y < -1f;
            bool killFwd = transform.position.z >
                             GameManager.I.ballSpawnPoint.position.z + 30f;

            if (stillLong || tooLong || killFall || killFwd || killSide)
            {
                GameManager.I.OnBallStopped();

                // дожидаемся кадра, пока он телепортнёт мяч
                yield return null;

                // ручной break, чтобы корутина завершилась сама и не зависла
                break;
            }

            yield return null;
        }

        launched = false;
    }

    public void HardReset(Vector3 pos, Quaternion rot)
    {
        StartCoroutine(HardResetRoutine(pos, rot));
    }

    IEnumerator HardResetRoutine(Vector3 pos, Quaternion rot)
    {
        rb.isKinematic = true;
        transform.SetPositionAndRotation(pos, rot);

        yield return new WaitForFixedUpdate();

        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        launched = false;
    }
}
