using UnityEngine;
using UnityEngine.UI;

public class RestartUI : MonoBehaviour
{
    [SerializeField] CanvasGroup group;
    [SerializeField] Image fillImage;

    float duration = 3f;
    float t;
    bool counting;

    public void Begin(float dur = 3f)
    {
        if (!group || !fillImage) return;
        duration = dur;
        t = 0;
        fillImage.fillAmount = 0f;
        group.alpha = 1f;
        counting = true;
    }

    public void Cancel()
    {
        if (!group || !fillImage) return;
        counting = false;
        group.alpha = 0f;
        fillImage.fillAmount = 0f;
    }

    void Update()
    {
        if (!counting) return;
        if (!group || !fillImage) { counting = false; return; }

        t += Time.unscaledDeltaTime;
        fillImage.fillAmount = Mathf.Clamp01(t / duration);
    }

    public bool Finished => counting && t >= duration;
}