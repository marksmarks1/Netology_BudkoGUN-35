using UnityEngine;

public class MeleeAnimationRelay : MonoBehaviour
{
    public MeleeWeapon weapon;
    AiAgent agent;

    void Awake()
    {
        if (!weapon) weapon = GetComponentInChildren<MeleeWeapon>(true);
        agent = GetComponent<AiAgent>();
    }

    public void Anim_MeleeHit()
    {
        if (!weapon)
            return;

        // если у нас есть точна€ цель из таргетинга Ч бьЄм напр€мую
        if (agent && agent.targeting && agent.targeting.HasTarget)
        {
            var targetGo = agent.targeting.Target;
            var h = targetGo.GetComponentInParent<Health>();
            if (h)
            {
                weapon.PlaySwingSound();

                Vector3 dir = (h.transform.position - transform.position).normalized;
                h.TakeDamage(weapon.damage, dir);

                var col = h.GetComponentInChildren<Collider>();
                if (col) weapon.SpawnBloodOn(col, transform.position);

                weapon.PlayHitSound();
                return;
            }
        }

        weapon.TryHit();
    }
}