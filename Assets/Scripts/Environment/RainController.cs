using UnityEngine;

public class RainController : MonoBehaviour
{
    [Header("Follow")]
    public Transform followTarget;                      
    public Vector3 followOffset = new Vector3(0, 8, 0);

    [Header("References")]
    public ParticleSystem rainStreaks;                  
    public AudioSource rainAudio;                       

    [Header("Intensity (0..1)")]
    [Range(0, 1)] public float intensity = 0f;          // сила дождя
    public float baseRateStreaks = 1200f;               // эмиссия при intensity=1
    public float baseVolume = 0.5f;                     // громкость при intensity=1

    [Header("Wind")]
    public Vector3 wind = new Vector3(2, -30, 0);       // X/Z ветер, Y отрицательный (вниз)

    ParticleSystem.EmissionModule _eStreaks;
    ParticleSystem.VelocityOverLifetimeModule _vStreaks;

    void Awake()
    {
        if (!rainStreaks) rainStreaks = GetComponentInChildren<ParticleSystem>();

        if (!rainAudio)
        {
            rainAudio = gameObject.GetComponent<AudioSource>();
            if (!rainAudio) rainAudio = gameObject.AddComponent<AudioSource>();
            rainAudio.playOnAwake = false;
            rainAudio.loop = true;
            rainAudio.spatialBlend = 0.0f;   
            rainAudio.dopplerLevel = 0f;
            rainAudio.volume = 0f;           
        }

        if (rainStreaks)
        {
            _eStreaks = rainStreaks.emission;
            _vStreaks = rainStreaks.velocityOverLifetime;
        }

        ApplyWind();
        ApplyIntensity();
    }

    void LateUpdate()
    {
        if (followTarget)
            transform.position = followTarget.position + followOffset;

        ApplyWind();
        ApplyIntensity();
    }

    public void SetIntensity(float t)
    {
        intensity = Mathf.Clamp01(t);
        ApplyIntensity();
    }

    public void BeginRain(float t = 1f) => SetIntensity(t);
    public void StopRain() => SetIntensity(0f);

    void ApplyIntensity()
    {
        if (rainStreaks)
            _eStreaks.rateOverTime = baseRateStreaks * intensity;

        if (rainAudio)
        {
            if (intensity > 0.01f)
            {
                if (!rainAudio.isPlaying) rainAudio.Play();
                rainAudio.volume = baseVolume * intensity;
            }
            else
            {
                rainAudio.volume = 0f;
                if (rainAudio.isPlaying) rainAudio.Stop();
            }
        }
    }

    void ApplyWind()
    {
        if (rainStreaks)
        {
            _vStreaks.x = wind.x;
            _vStreaks.y = wind.y;   
            _vStreaks.z = wind.z;
        }
    }
}