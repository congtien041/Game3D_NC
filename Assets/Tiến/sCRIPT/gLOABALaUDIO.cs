using UnityEngine;

public class GlobalAudio : MonoBehaviour
{
    public static GlobalAudio Instance;
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource nitroSource;
    public AudioSource engineSource;

    [Header("Clips")]
    public AudioClip backgroundMusic;
    public AudioClip acceleration;
    public AudioClip nitroBoost;
    public AudioClip brakeLoop;
    public AudioClip buttonClick; // THÊM LẠI BIẾN NÀY ĐỂ FIX LỖI UPGRADEMENUUI
    public AudioClip ignition;

    void Awake() {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    void Start() { PlayMusic(backgroundMusic); }

    public void StartEngineSound() {
        if (engineSource == null || engineSource.isPlaying) return;
        engineSource.clip = acceleration; engineSource.loop = true; engineSource.Play();
    }

    public void PlayNitroSound(bool play) {
        if (nitroSource == null) return;
        if (play) {
            if (!nitroSource.isPlaying) { nitroSource.clip = nitroBoost; nitroSource.loop = true; nitroSource.Play(); }
        } else nitroSource.Stop();
    }

    public void StartBrakeLoop() {
        if (sfxSource == null || (sfxSource.clip == brakeLoop && sfxSource.isPlaying)) return;
        sfxSource.clip = brakeLoop; sfxSource.loop = true; sfxSource.Play();
    }

    public void StopBrakeLoop() { 
        if (sfxSource != null && sfxSource.clip == brakeLoop) sfxSource.Stop(); 
    }

    public void PlayMusic(AudioClip clip) {
        if (musicSource == null || clip == null) return;
        musicSource.loop = true; musicSource.clip = clip; musicSource.Play();
    }

    public void PlaySFX(AudioClip clip) { 
        if (sfxSource != null && clip != null) sfxSource.PlayOneShot(clip); 
    }
}