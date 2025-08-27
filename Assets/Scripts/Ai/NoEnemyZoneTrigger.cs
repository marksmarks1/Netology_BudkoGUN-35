using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(BoxCollider))]
public class NoEnemyZonePush : MonoBehaviour
{
    [Header("Repel / Flee")]
    public float pushOut = 1.5f;   // насколько выталкивать от границы, м
    public float pushSpeed = 6f;     // скорость выталкивания, м/с (fallback без навмеша)
    public float fleeDistance = 4f;     
    public float fleeTime = 1.5f;   
    public float minRunSpeed = 5f;     

    BoxCollider box;

    class State
    {
        public float fleeUntil;
        public Vector3 dir;
        public float? prevSpeed;
    }
    readonly Dictionary<AiAgent, State> states = new();

    void Awake()
    {
        box = GetComponent<BoxCollider>();
        box.isTrigger = true;               
    }

    void OnTriggerEnter(Collider other)
    {
        var ai = other.GetComponentInParent<AiAgent>();
        if (!ai) return;

        Vector3 p = ai.transform.position;
        Vector3 closest = box.ClosestPoint(p);
        Vector3 dir = (p - closest); dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) dir = (p - box.bounds.center);
        if (dir.sqrMagnitude < 0.0001f) dir = -ai.transform.forward;
        dir.Normalize();

        var st = new State { dir = dir, fleeUntil = Time.time + fleeTime };
        states[ai] = st;

        var nav = ai.navMeshAgent;
        // Разворот лицом от препятствия (в сторону убегания)
        if (dir.sqrMagnitude > 0.0001f)
        {
            var rot = Quaternion.LookRotation(dir);
            ai.transform.rotation = rot;
        }

        // Если агент активен и стоит на навмешe — задаём цель убежать
        if (nav && nav.enabled && nav.isOnNavMesh)
        {
            st.prevSpeed = nav.speed;
            nav.speed = Mathf.Max(nav.speed, minRunSpeed);

            Vector3 fleeTarget = p + dir * fleeDistance;
            if (NavMesh.SamplePosition(fleeTarget, out var hit, 2f, NavMesh.AllAreas))
            {
                nav.isStopped = false;      
                nav.ResetPath();
                nav.SetDestination(hit.position);
            }
        }
        else
        {
            ai.transform.position = Vector3.MoveTowards(p, p + dir * pushOut, pushSpeed * Time.deltaTime);
        }
    }

    void OnTriggerStay(Collider other)
    {
        var ai = other.GetComponentInParent<AiAgent>();
        if (!ai) return;
        if (!states.TryGetValue(ai, out var st)) return;

        st.fleeUntil = Time.time + fleeTime;

        Vector3 p = ai.transform.position;
        Vector3 closest = box.ClosestPoint(p);
        Vector3 dir = (p - closest); dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) dir = st.dir;
        dir.Normalize();
        st.dir = dir;

        var nav = ai.navMeshAgent;
        if (nav && nav.enabled && nav.isOnNavMesh)
        {
            Vector3 target = closest + dir * Mathf.Max(pushOut, fleeDistance);
            if (NavMesh.SamplePosition(target, out var hit, 2f, NavMesh.AllAreas))
            {
                if (nav.isStopped) nav.isStopped = false;
                nav.SetDestination(hit.position);
            }
        }
        else
        {
            ai.transform.position = Vector3.MoveTowards(p, closest + dir * pushOut, pushSpeed * Time.deltaTime);
        }
    }

    void OnTriggerExit(Collider other)
    {
        var ai = other.GetComponentInParent<AiAgent>();
        if (!ai) return;
        Restore(ai);
    }

    void Update()
    {
        if (states.Count == 0) return;

        var toRestore = new List<AiAgent>();
        foreach (var kv in states)
            if (Time.time > kv.Value.fleeUntil) toRestore.Add(kv.Key);
        foreach (var ai in toRestore) Restore(ai);
    }

    void Restore(AiAgent ai)
    {
        if (!states.TryGetValue(ai, out var st)) return;
        var nav = ai.navMeshAgent;
        if (nav && nav.enabled && nav.isOnNavMesh && st.prevSpeed.HasValue)
            nav.speed = st.prevSpeed.Value;
        states.Remove(ai);
    }
}