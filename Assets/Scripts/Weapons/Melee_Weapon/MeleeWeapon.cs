using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : MonoBehaviour
{
    [Header("Setup")]
    public Transform origin;       // AttackOrigin
    public float range = 1.8f;
    public float radius = 0.5f;
    public float damage = 15f;
    public LayerMask hitMask;

    [Header("Control")]
    public float hitCooldown = 0.15f;
    public bool debugDraw = false;

    [Header("Blood FX")]
    public GameObject bloodEffect;
    public float bloodEffectLife = 2f;
    public bool attachBloodToTarget = true;

    [Header("SFX")]
    public AudioSource audioSource;          
    public AudioClip swingClip;              // «вжух»
    public AudioClip hitClip;                // удар
    [Range(0f, 1f)] public float swingVolume = 0.8f;
    [Range(0f, 1f)] public float hitVolume = 0.9f;
    public Vector2 pitchJitter = new Vector2(0.97f, 1.03f); 

    float _lastHitTime = -999f;
    readonly Collider[] _hits = new Collider[32];
    readonly HashSet<Health> _already = new HashSet<Health>();
    Transform _root;

    void Awake()
    {
        _root = transform.root;
        if (!origin) origin = transform;

        if (!audioSource) audioSource = GetComponent<AudioSource>();
        if (!audioSource) audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
        audioSource.spatialBlend = 0.4f;   
    }

    public void TryHit()
    {
        if (Time.time < _lastHitTime + hitCooldown) return;
        _lastHitTime = Time.time;

        _already.Clear();

        // проигрываем свист замаха в момент «удара»
        PlaySwingSound();

        Vector3 f = origin.forward;
        const float backreach = 0.25f;
        Vector3 a = origin.position - f * backreach;
        Vector3 b = origin.position + f * (range + 0.15f);

        int count = Physics.OverlapCapsuleNonAlloc(
            a, b, radius, _hits, hitMask, QueryTriggerInteraction.Collide);

        if (debugDraw)
        {
            Debug.DrawLine(a, b, Color.red, 0.5f);
            Debug.DrawRay(a, Vector3.up * 0.1f, Color.yellow, 0.5f);
            Debug.DrawRay(b, Vector3.up * 0.1f, Color.yellow, 0.5f);
        }

        bool hitSomeone = false;

        for (int i = 0; i < count; i++)
        {
            var col = _hits[i];
            if (!col) continue;
            if (col.transform.root == _root) continue;

            var h = col.GetComponentInParent<Health>();
            if (!h || _already.Contains(h)) continue;
            if (h is PropHealth) continue;

            _already.Add(h);

            Vector3 dir = (col.transform.position - origin.position).normalized;
            h.TakeDamage(damage, dir);

            SpawnBloodOn(col, origin.position);
            hitSomeone = true;
        }

        if (hitSomeone) PlayHitSound();
    }

    public void SpawnBloodOn(Collider hitCol, Vector3 fromPos, float lifeOverride = -1f)
    {
        if (!bloodEffect || !hitCol) return;

        Vector3 pos = hitCol.ClosestPoint(fromPos);
        Vector3 normal = (pos - fromPos);
        normal.y = 0f;
        if (normal.sqrMagnitude < 0.0001f) normal = -origin.forward;
        else normal.Normalize();

        var fx = Instantiate(bloodEffect, pos, Quaternion.LookRotation(normal));
        if (attachBloodToTarget) fx.transform.SetParent(hitCol.transform, true);
        Destroy(fx, lifeOverride > 0 ? lifeOverride : bloodEffectLife);
    }

    public void PlaySwingSound()
    {
        if (!audioSource || !swingClip) return;
        audioSource.pitch = Random.Range(pitchJitter.x, pitchJitter.y);
        audioSource.PlayOneShot(swingClip, swingVolume);
    }

    public void PlayHitSound()
    {
        if (!audioSource || !hitClip) return;
        audioSource.pitch = Random.Range(pitchJitter.x, pitchJitter.y);
        audioSource.PlayOneShot(hitClip, hitVolume);
    }
}