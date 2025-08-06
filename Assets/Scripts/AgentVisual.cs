using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AgentVisual : MonoBehaviour
{
    [SerializeField] private Transform visual; 
    [SerializeField] private float rotateSpeed = 10f;

    private NavMeshAgent _agent;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        if (!_agent || !visual) return;

        Vector3 v = _agent.desiredVelocity; v.y = 0f;
        if (v.sqrMagnitude > 0.001f)
        {
            var target = Quaternion.LookRotation(v);
            visual.rotation = Quaternion.Slerp(visual.rotation, target, rotateSpeed * Time.deltaTime);
        }
    }
}
