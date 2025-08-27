using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AiHealth : Health
{
    AiAgent agent;

    protected override void OnStart() {
        agent = GetComponent<AiAgent>();
    }

    protected override void OnDeath(Vector3 direction) {
        var h = GetComponent<Health>();
        if (KillCounter.Instance && h && h.lastHitByPlayer)
            KillCounter.Instance.RegisterKill(this);

        AiDeathState deathState = agent.stateMachine.GetState(AiStateId.Death) as AiDeathState;
        deathState.direction = direction;
        agent.stateMachine.ChangeState(AiStateId.Death);
    }

    protected override void OnDamage(Vector3 direction) {

    }
}
