using UnityEngine;
using UnityEngine.AI;

public class AiDeathState : AiState
{
    public Vector3 direction = Vector3.zero;

    public AiStateId GetId() => AiStateId.Death;

    public void Enter(AiAgent agent)
    {
        if (agent.weapons) agent.weapons.SetFiring(false);
        var melee = agent.GetComponent<AiMelee>();
        if (melee) melee.CancelAttack();

        var nma = agent.navMeshAgent;
        if (nma && nma.isActiveAndEnabled)
        {
            if (nma.isOnNavMesh)
            {
                nma.isStopped = true;
                nma.updatePosition = false;
                nma.updateRotation = false;
            }
            nma.enabled = false;
        }

        if (agent.sensor) agent.sensor.enabled = false;
        if (agent.ui) agent.ui.gameObject.SetActive(false);

        var animator = agent.GetComponent<Animator>();
        if (animator) animator.enabled = false;

        if (agent.ragdoll)
        {
            agent.ragdoll.ActivateRagdoll();

            var dir = direction;
            if (dir == Vector3.zero) dir = Vector3.up;
            dir.Normalize();
            dir.y = Mathf.Max(dir.y, 0.25f);

            agent.ragdoll.ApplyForce(dir * 12f);
        }

        if (agent.weapons)
        {
            agent.weapons.SetFiring(false);
            if (agent.weapons.HasWeapon())
            {
                agent.weapons.DeactivateWeapon();
                agent.weapons.DropWeapon();
            }
        }

        var cc = agent.GetComponent<CharacterController>();
        if (cc) cc.enabled = false;

        var hi = agent.GetComponent<TargetHighlighter>();
        if (hi) hi.enabled = false;
    }

    public void Update(AiAgent agent) { }
    public void Exit(AiAgent agent) { }
}
