using UnityEngine;

public class AiMeleeAttackState : AiState
{
    public AiStateId GetId() => AiStateId.MeleeAttack;

    public void Enter(AiAgent agent)
    {
        agent.navMeshAgent.isStopped = true;
    }

    public void Update(AiAgent agent)
    {
        if (agent.targeting == null || !agent.targeting.HasTarget)
        {
            agent.stateMachine.ChangeState(AiStateId.FindTarget);
            return;
        }

        var targetTf = agent.targeting.Target.transform;
        var melee = agent.GetComponent<AiMelee>();

        if (melee == null || !melee.InRange(targetTf))
        {
            agent.stateMachine.ChangeState(AiStateId.MeleeChase);
            return;
        }

        melee.Face(targetTf);
        melee.TryAttack(targetTf); 
    }

    public void Exit(AiAgent agent)
    {
        agent.navMeshAgent.isStopped = false;
    }
}
