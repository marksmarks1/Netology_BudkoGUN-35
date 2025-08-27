using System.Collections;
using UnityEngine;

public class PropHealth : Health
{
    [Header("Destruction")]
    public GameObject destroyedPrefab;   
    public GameObject deathFx;           
    public float deathFxLife = 3f;
    public bool destroyOriginal = true;
    public float destroyDelay = 0f;

    [Header("Hit flash (ξοφ.)")]
    public Renderer[] blinkRenderers;    
    public Color hitColor = new Color(1f, .2f, .2f);
    public float hitFlashTime = 0.07f;

    protected override void OnStart()
    {
        if (blinkRenderers == null || blinkRenderers.Length == 0)
            blinkRenderers = GetComponentsInChildren<Renderer>(true);
    }

    protected override void OnDamage(Vector3 direction)
    {
        if (blinkRenderers == null) return;
        StopAllCoroutines();
        StartCoroutine(FlashOnce());
    }

    IEnumerator FlashOnce()
    {
        foreach (var r in blinkRenderers) if (r) r.material.color = hitColor;
        yield return new WaitForSeconds(hitFlashTime);
        foreach (var r in blinkRenderers) if (r) r.material.color = Color.white;
    }

    protected override void OnDeath(Vector3 direction)
    {
        if (deathFx)
        {
            var fx = Instantiate(deathFx, transform.position, Quaternion.identity);
            if (deathFxLife > 0) Destroy(fx, deathFxLife);
        }

        if (destroyedPrefab)
        {
            var inst = Instantiate(destroyedPrefab, transform.position, transform.rotation);
            foreach (var rb in inst.GetComponentsInChildren<Rigidbody>())
                rb.AddExplosionForce(5f, transform.position - direction.normalized, 2f, 0.2f, ForceMode.Impulse);
        }

        if (destroyOriginal) Destroy(gameObject, destroyDelay);
        else
        {
            foreach (var c in GetComponentsInChildren<Collider>()) c.enabled = false;
            foreach (var r in GetComponentsInChildren<Renderer>()) r.enabled = false;
        }
    }

    void Update() { }
    void LateUpdate() { }
}