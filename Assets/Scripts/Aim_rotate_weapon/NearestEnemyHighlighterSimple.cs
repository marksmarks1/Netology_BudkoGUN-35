using UnityEngine;

public class NearestEnemyHighlighterSimple : MonoBehaviour
{
    public float radius = 10f;
    public float scanInterval = 0.1f;
    public bool requireLineOfSight = false;
    public LayerMask occlusionMask;

    float nextScan;
    TargetHighlighter current;

    void OnEnable() { SetCurrent(null); nextScan = 0f; }
    void OnDisable() { SetCurrent(null); }

    void Update()
    {
        if (Time.time < nextScan) return;
        nextScan = Time.time + scanInterval;

        // Держим текущего, пока подходит
        if (current && current.isActiveAndEnabled &&
            current.gameObject.activeInHierarchy &&
            (current.transform.position - transform.position).sqrMagnitude <= radius * radius &&
            Visible(current.transform))
        {
            return;
        }

        TargetHighlighter best = null;
        float bestSqr = float.MaxValue;
        int candidates = 0;

        foreach (var hi in TargetHighlighter.All)
        {
            if (!hi || !hi.isActiveAndEnabled || !hi.gameObject.activeInHierarchy) continue;
            if (hi.transform.root == transform.root) continue; // не себя

            float sqr = (hi.transform.position - transform.position).sqrMagnitude;
            if (sqr > radius * radius) continue;
            candidates++;

            if (!Visible(hi.transform)) continue;

            if (sqr < bestSqr) { bestSqr = sqr; best = hi; }
        }

        SetCurrent(best);
    }

    bool Visible(Transform t)
    {
        if (!requireLineOfSight) return true;
        Vector3 from = transform.position + Vector3.up * 1.5f;
        Vector3 to = t.position + Vector3.up * 1.5f;
        return !Physics.Linecast(from, to, occlusionMask, QueryTriggerInteraction.Ignore);
    }

    void SetCurrent(TargetHighlighter hi)
    {
        if (current == hi) return;
        if (current) current.SetHighlight(false);
        current = hi;
        if (current) current.SetHighlight(true);
    }

}