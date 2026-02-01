using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Enums;
using Random = UnityEngine.Random;

public class CollisionSounds : MonoBehaviour
{
    private AudioManager audioManager;
    private AudioSource OneShotAudioSource = new AudioSource();
    public float hardCrashSpeedTreshold, softCrashSpeedTreshold, scratchSpeedTreshold;
    void Start()
    {
        audioManager = transform.GetComponent<AudioManager>();
        OneShotAudioSource = transform.gameObject.AddComponent<AudioSource>();
        OneShotAudioSource.volume = 1;
    }

    private void OnCollisionEnter(Collision collision)
    {
        PhysicsMaterial pm = collision.collider.material;
        foreach (string sst in Enum.GetNames(typeof(SubSoundType)))
        {
            if (pm.name.Contains(sst))
            {
                if (collision.relativeVelocity.magnitude > hardCrashSpeedTreshold)
                {
                    try
                    {
                        IList<Sound> sounds = audioManager.sounds.Where(x => x.Type == SoundType.HardCrash && x.SubType.ToString() == sst).ToList();
                        int index = Random.Range(0, sounds.Count);
                        Sound sound = sounds.Skip(index).First();
                        OneShotAudioSource.volume = sound.volume;
                        OneShotAudioSource.PlayOneShot(sound.clip);
                        break;
                    }
                    catch (Exception)
                    {
                    }

                }
                else if (collision.relativeVelocity.magnitude > softCrashSpeedTreshold)
                {
                    try
                    {
                        IList<Sound> sounds = audioManager.sounds.Where(x => x.Type == SoundType.SoftCrash && x.SubType.ToString() == sst).ToList();
                        int index = Random.Range(0, sounds.Count);
                        Sound sound = sounds.Skip(index).First();
                        OneShotAudioSource.volume = sound.volume;
                        OneShotAudioSource.PlayOneShot(sound.clip);
                        break;
                    }
                    catch (Exception) 
                    {
                    }
                }
                else if (collision.relativeVelocity.magnitude > scratchSpeedTreshold)
                {
                    try
                    {
                        IList<Sound> sounds = audioManager.sounds.Where(x => x.Type == SoundType.Scratch && x.SubType.ToString() == sst).ToList();
                        int index = Random.Range(0, sounds.Count);
                        Sound sound = sounds.Skip(index).First();
                        OneShotAudioSource.volume = sound.volume;
                        OneShotAudioSource.PlayOneShot(sound.clip);
                        break;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }
    }

    void Update()
    {
        
    }
}
