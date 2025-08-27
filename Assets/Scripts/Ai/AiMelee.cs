using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class AiMelee : MonoBehaviour
{
    public string attackTrigger = "melee_attack";
    public float windup = 0.18f;
    public float cooldown = 1.0f;
    public float stopDistance = 1.4f;

    Animator animator;
    NavMeshAgent agent;
    MeleeWeapon weapon;

    bool attacking;
    float nextTime;
    Coroutine attackCo;

    void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        weapon = GetComponentInChildren<MeleeWeapon>(true);
    }

    bool AgentValid()
    {
        return agent && agent.isActiveAndEnabled && agent.gameObject.activeInHierarchy && agent.isOnNavMesh;
    }

    void SafeStop(bool stop)
    {
        if (AgentValid()) agent.isStopped = stop;
    }

    public void CancelAttack()
    {
        if (attackCo != null) { StopCoroutine(attackCo); attackCo = null; }
        attacking = false;
        SafeStop(false);
    }

    void OnDisable() => CancelAttack();

    public bool InRange(Transform target)
    {
        if (!weapon || !target) return false;
        return Vector3.Distance(transform.position, target.position) <= Mathf.Max(weapon.range, stopDistance);
    }

    public void Face(Transform target, float lerp = 10f)
    {
        if (!target) return;
        var to = target.position - transform.position; to.y = 0;
        if (to.sqrMagnitude < 0.001f) return;
        var rot = Quaternion.LookRotation(to);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, lerp * Time.deltaTime);
    }

    public void TryAttack(Transform target)
    {
        if (attacking || Time.time < nextTime || !weapon) return;
        attackCo = StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        attacking = true;
        nextTime = Time.time + cooldown;

        SafeStop(true);

        if (animator && !string.IsNullOrEmpty(attackTrigger))
            animator.SetTrigger(attackTrigger);

        // задержка под замах
        yield return new WaitForSeconds(windup);
        if (!attacking) { SafeStop(false); yield break; }

        if (weapon) weapon.TryHit();

        yield return new WaitForSeconds(0.1f);
        SafeStop(false);

        attacking = false;
        attackCo = null;
    }
}