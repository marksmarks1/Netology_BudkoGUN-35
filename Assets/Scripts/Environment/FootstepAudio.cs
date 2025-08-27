using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FootstepAudio : MonoBehaviour
{
    public AudioClip[] defaultClips;

    [Range(0.1f, 2f)] public float volumeMin = 0.9f, volumeMax = 1.1f;
    [Range(0.5f, 1.5f)] public float pitchMin = 0.95f, pitchMax = 1.05f;

    AudioSource src;

    void Awake()
    {
        src = GetComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = false;
        src.spatialBlend = 1f;   
    }

    public void PlayStepAt(Vector3 pos, float loud = 1f)
    {
        if (!src || defaultClips == null || defaultClips.Length == 0) return;
        var clip = defaultClips[Random.Range(0, defaultClips.Length)];
        src.transform.position = pos;
        src.pitch = Random.Range(pitchMin, pitchMax);
        src.PlayOneShot(clip, Random.Range(volumeMin, volumeMax) * loud);
    }
}