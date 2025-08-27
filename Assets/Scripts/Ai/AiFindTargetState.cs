using UnityEngine;

public class AiFindTargetState : AiState
{
    public AiStateId GetId() => AiStateId.FindTarget;

    public void Enter(AiAgent agent)
    {
        agent.navMeshAgent.isStopped = false;
    }

    public void Update(AiAgent agent)
    {
        if (agent.targeting != null && agent.targeting.HasTarget)
        {
            bool isMelee = agent.GetComponent<AiMelee>() != null;

            if (isMelee)
                agent.stateMachine.ChangeState(AiStateId.MeleeChase);
            else
                agent.stateMachine.ChangeState(AiStateId.AttackTarget);

            return;
        }
    }

    public void Exit(AiAgent agent) { }
}
