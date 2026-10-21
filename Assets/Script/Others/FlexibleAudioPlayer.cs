using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FlexibleAudioPlayer : MonoBehaviour
{
    [Header("ไฟล์เสียง (ใส่หลายอันจะสุ่มเล่น)")]
    public AudioClip[] soundClips;

    [Header("ตั้งค่าการเล่น")]
    public bool playOnEnable = false;

    [Header("สุ่มความดังและระดับเสียง")]
    [Range(0f, 1f)] public float minVolume = 0.8f;
    [Range(0f, 1f)] public float maxVolume = 1f;
    [Range(0.5f, 1.5f)] public float minPitch = 0.9f;
    [Range(0.5f, 1.5f)] public float maxPitch = 1.1f;

    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    void OnEnable()
    {
        if (playOnEnable)
        {
            PlaySound();
        }
    }

    public void PlaySound()
    {
        if (soundClips == null || soundClips.Length == 0)
        {
            Debug.LogWarning("ลืมใส่ไฟล์เสียงในช่อง Sound Clips");
            return;
        }
        AudioClip randomClip = soundClips[Random.Range(0, soundClips.Length)];
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.volume = Random.Range(minVolume, maxVolume);
        audioSource.PlayOneShot(randomClip);
    }
}