using static Enums;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    void Awake()
    {
        foreach(Sound s in sounds)
        {
            s.pitch = 1f;
            s.loop = false;
        }
    }
}
