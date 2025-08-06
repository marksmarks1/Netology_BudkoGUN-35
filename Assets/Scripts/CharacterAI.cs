using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class CharacterAI : MonoBehaviour
{
    public NavMeshAgent Agent { get; private set; }
    public Animator Anim { get; private set; }

    [Header("Search")]
    [SerializeField, Min(0.1f)] private float wanderRadius = 15f;
    [SerializeField, Min(0.1f)] private float sightRadius = 5f;
    [SerializeField] private LayerMask itemMask;

    [Header("Collect")]
    [SerializeField, Min(0.1f)] private float stopDistance = 1.2f;
    public float StopDistance => stopDistance;

    private Transform _currentTarget;

    public Transform CurrentTarget => _currentTarget;
    public float SightRadius => sightRadius;

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
        Anim = GetComponent<Animator>();
    }

    public void SetRandomDestination()
    {
        Vector3 random = Random.insideUnitSphere * wanderRadius + transform.position;
        if (NavMesh.SamplePosition(random, out NavMeshHit hit, wanderRadius, NavMesh.AllAreas))
        {
            Agent.stoppingDistance = 0f;
            Agent.SetDestination(hit.position);
        }
    }

    public bool ReachedDestination()
    {
        if (Agent.pathPending) return false;
        if (Agent.remainingDistance > Agent.stoppingDistance) return false;
        return !Agent.hasPath || Agent.velocity.sqrMagnitude < 0.01f;
    }

    public bool TryFindTarget()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, sightRadius, itemMask);
        if (hits == null || hits.Length == 0) return false;

        Transform best = null;
        float bestDistSq = float.MaxValue;

        foreach (var c in hits)
        {
            float d = (c.transform.position - transform.position).sqrMagnitude;
            if (d < bestDistSq)
            {
                bestDistSq = d;
                best = c.transform;
            }
        }

        if (best != null)
        {
            _currentTarget = best;
            return true;
        }
        return false;
    }

    public void MoveToTarget()
    {
        if (!_currentTarget) return;
        Agent.stoppingDistance = stopDistance;
        Agent.SetDestination(_currentTarget.position);
    }

    public void CollectTarget()
    {
        if (!_currentTarget) return;
        Destroy(_currentTarget.gameObject);
        _currentTarget = null;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRadius);
    }
#endif
}
