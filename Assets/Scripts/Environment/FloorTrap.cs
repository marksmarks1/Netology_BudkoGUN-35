using UnityEngine;

public class FloorTrap : MonoBehaviour
{
    [Header("FX & Sound")]
    public GameObject explosionFxPrefab;     
    public AudioClip explosionSfx;           
    [Range(0f, 1f)] public float sfxVolume = 0.9f;

    [Header("Damage")]
    public bool killInstantly = false;       // если true Ч ваншот
    public float damage = 50f;               // иначе наносим фикс. урон

    [Header("Lifecycle")]
    public bool oneShot = true;              // одноразова€ ловушка
    public bool autoDestroy = true;          // удалить объект после срабатывани€
    public float destroyDelay = 0.5f;

    bool triggered;

    void OnTriggerEnter(Collider other)
    {
        if (oneShot && triggered) return;

        var health = other.GetComponentInParent<Health>();
        if (!health) return;

        triggered = true;

        Vector3 dir = (other.transform.position - transform.position).normalized;
        dir.y = Mathf.Max(dir.y, 0.25f);

        float dmg = killInstantly ? 9999f : damage;
        health.TakeDamage(dmg, dir);

        if (explosionFxPrefab)
        {
            var fx = Instantiate(explosionFxPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 3f);
        }

        if (explosionSfx) AudioSource.PlayClipAtPoint(explosionSfx, transform.position, sfxVolume);

        if (autoDestroy) { Destroy(gameObject, destroyDelay); }
        else if (oneShot)
        {
            var col = GetComponent<Collider>(); if (col) col.enabled = false;
            var rend = GetComponentInChildren<Renderer>(); if (rend) rend.enabled = false;
        }
    }
}