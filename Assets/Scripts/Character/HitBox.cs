using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitBox : MonoBehaviour
{
    public Health health;

    public void OnRaycastHit(RaycastWeapon weapon, Vector3 direction)
    {
        if (!health) health = GetComponentInParent<Health>();
        if (!health) return;

        bool byPlayer = weapon && weapon.transform.root.CompareTag("Player");
        health.MarkLastHitByPlayer(byPlayer);

        float dmg = weapon ? weapon.damage : 10f;
        health.TakeDamage(dmg, direction);
    }
}
