using UnityEngine;

public class AiMeleeChaseState : AiState
{
    float prevSpeed;
    float prevStoppingDistance;

    public AiStateId GetId() => AiStateId.MeleeChase;

    public void Enter(AiAgent agent)
    {
        agent.navMeshAgent.isStopped = false;

        prevSpeed = agent.navMeshAgent.speed;
        prevStoppingDistance = agent.navMeshAgent.stoppingDistance;

        // ближник подбегает ближе
        var melee = agent.GetComponent<AiMelee>();
        var weapon = agent.GetComponentInChildren<MeleeWeapon>(true);

        float stop = (melee != null) ? melee.stopDistance : 1.2f;
        if (weapon) stop = Mathf.Min(stop, Mathf.Max(0.6f, weapon.range - 0.2f));

        agent.navMeshAgent.stoppingDistance = stop;
    }

    public void Update(AiAgent agent)
    {
        if (agent.targeting == null || !agent.targeting.HasTarget)
        {
            agent.stateMachine.ChangeState(AiStateId.FindTarget);
            return;
        }

        var targetPos = agent.targeting.TargetPosition;
        agent.navMeshAgent.SetDestination(targetPos);

        var melee = agent.GetComponent<AiMelee>();
        if (melee != null)
        {
            melee.Face(agent.targeting.Target.transform);

            float sd = melee.stopDistance;
            float dist = Vector3.Distance(agent.transform.position, targetPos);

            float baseWalk = 2.2f;
            if (agent.config != null)
                baseWalk = Mathf.Max(agent.config.attackSpeed, baseWalk);

            float walkSpeed = baseWalk;
            float runSpeed = Mathf.Max(walkSpeed * 1.9f, 4.8f);

            float near = sd + 0.4f;  // ближе Ч идЄм
            float far = sd + 1.6f;  // дальше Ч бежим

            if (dist <= near) agent.navMeshAgent.speed = walkSpeed;
            else if (dist >= far) agent.navMeshAgent.speed = runSpeed;

            if (melee.InRange(agent.targeting.Target.transform))
                agent.stateMachine.ChangeState(AiStateId.MeleeAttack);
        }
    }

    public void Exit(AiAgent agent)
    {
        agent.navMeshAgent.speed = prevSpeed;
        agent.navMeshAgent.stoppingDistance = prevStoppingDistance;
    }
}