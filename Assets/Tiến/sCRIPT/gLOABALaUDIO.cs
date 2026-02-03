using UnityEngine;

public class GlobalAudio : MonoBehaviour
{
    public static GlobalAudio Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music")]
    public AudioClip backgroundMusic;

    [Header("Car Sounds")]
    public AudioSource engineSource;
    public AudioClip acceleration;
    public AudioClip reverseSound;

    public AudioClip brakeLoop;     
    public AudioClip ignition;
    public AudioClip braking;       

    [Header("UI")]
    public AudioClip buttonClick;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        Debug.Log("GlobalAudio Start() is RUNNING!");
        PlayMusic(backgroundMusic);
    }

    

    public void StartEngineSound()
    {
        if (engineSource.isPlaying && engineSource.clip == acceleration)
            return;

        engineSource.clip = acceleration;
        engineSource.loop = true;
        engineSource.Play();
    }

    public void StopEngineSound()
    {
        if (engineSource.isPlaying && engineSource.clip == acceleration)
            engineSource.Stop();
    }

   

    public void StartReverseSound()
    {
        if (engineSource.isPlaying && engineSource.clip == reverseSound)
            return;

        engineSource.clip = reverseSound;
        engineSource.loop = true;
        engineSource.Play();
    }

    public void StopReverseSound()
    {
        if (engineSource.isPlaying && engineSource.clip == reverseSound)
            engineSource.Stop();
    }

   

    public void StartBrakeLoop()
    {
        if (engineSource.isPlaying && engineSource.clip == brakeLoop)
            return;  // already playing brake loop

        engineSource.clip = brakeLoop;
        engineSource.loop = true;
        engineSource.Play();
    }

    public void StopBrakeLoop()
    {
        if (engineSource.isPlaying && engineSource.clip == brakeLoop)
            engineSource.Stop();
    }

    

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;

        musicSource.loop = true;
        musicSource.clip = clip;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }
}
