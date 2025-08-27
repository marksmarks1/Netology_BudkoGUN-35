using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
public class LightingManager : MonoBehaviour
{
    public enum LightingMode { Day, Night, Dynamic }
    [Header("Mode")]
    public LightingMode mode = LightingMode.Day;   // выбирать
    [Range(0f, 1f)] public float timeOfDay = 0.35f; // 0=ночь, 0.5=полдень (для Dynamic)
    public float dayLengthSeconds = 120f;

    [Header("References")]
    public Light sun;                 
    public Volume globalVolume;        
    public Material skybox;            

    [Header("Day preset")]
    public float daySunIntensity = 1.8f;
    public Color daySunColor = new Color(1f, 0.96f, 0.84f);
    public float dayAmbientIntensity = 1.0f;
    public Color dayAmbientColor = Color.white;
    public float dayExposure = 0.0f;      
    public float dayBloom = 6.0f;         
    public float dayVignette = 0.05f;     

    [Header("Night preset")]
    public float nightSunIntensity = 0.12f;
    public Color nightSunColor = new Color(0.55f, 0.65f, 1.0f);
    public float nightAmbientIntensity = 0.2f;
    public Color nightAmbientColor = new Color(0.1f, 0.12f, 0.18f);
    public float nightExposure = -1.2f;
    public float nightBloom = 2.0f;
    public float nightVignette = 0.22f;

    ColorAdjustments _colorAdj;
    Vignette _vignette;
    Bloom _bloom;

    void OnEnable()
    {
        if (!sun) sun = FindFirstObjectByType<Light>();
        if (!skybox) skybox = RenderSettings.skybox;
        CacheOverrides();
        ApplyNow();
    }

    void CacheOverrides()
    {
        if (globalVolume && globalVolume.profile)
        {
            globalVolume.profile.TryGet(out _colorAdj);
            globalVolume.profile.TryGet(out _vignette);
            globalVolume.profile.TryGet(out _bloom);
        }
    }

    void Update()
    {
        if (mode == LightingMode.Dynamic && Application.isPlaying)
        {
            timeOfDay += Time.deltaTime / Mathf.Max(1f, dayLengthSeconds);
            if (timeOfDay > 1f) timeOfDay -= 1f;
            ApplyDynamic(timeOfDay);
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
            ApplyNow(); // чтобы правки видно было сразу в редакторе
#endif
    }

    void OnValidate() => ApplyNow();

    public void ApplyNow()
    {
        switch (mode)
        {
            case LightingMode.Day: ApplyPreset(1f); break;
            case LightingMode.Night: ApplyPreset(0f); break;
            case LightingMode.Dynamic: ApplyDynamic(timeOfDay); break;
        }
    }

    void ApplyDynamic(float t01)
    {
        if (sun)
            sun.transform.rotation = Quaternion.Euler(Mathf.Lerp(-10f, 170f, t01), sun.transform.eulerAngles.y, 0f);

        float k = Mathf.Sin(t01 * Mathf.PI);
        ApplyPreset(k);
    }

    void ApplyPreset(float day01)
    {
        if (sun)
        {
            sun.intensity = Mathf.Lerp(nightSunIntensity, daySunIntensity, day01);
            sun.color = Color.Lerp(nightSunColor, daySunColor, day01);
        }

        RenderSettings.ambientIntensity = Mathf.Lerp(nightAmbientIntensity, dayAmbientIntensity, day01);
        RenderSettings.ambientLight = Color.Lerp(nightAmbientColor, dayAmbientColor, day01);

        if (_colorAdj != null) { _colorAdj.postExposure.overrideState = true; _colorAdj.postExposure.value = Mathf.Lerp(nightExposure, dayExposure, day01); }
        if (_bloom != null) { _bloom.intensity.overrideState = true; _bloom.intensity.value = Mathf.Lerp(nightBloom, dayBloom, day01); }
        if (_vignette != null) { _vignette.intensity.overrideState = true; _vignette.intensity.value = Mathf.Lerp(nightVignette, dayVignette, day01); }

        if (skybox && skybox.HasProperty("_Exposure"))
            skybox.SetFloat("_Exposure", Mathf.Lerp(0.8f, 1.3f, day01));

        DynamicGI.UpdateEnvironment(); 
    }
}