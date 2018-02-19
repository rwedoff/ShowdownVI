using System;
using System.Collections.Generic;
using UnityEngine;

public class NumberSpeech : MonoBehaviour
{
    private static Dictionary<string,AudioSource> scoreAudioMap;

    private void FillAudioMap(AudioSource[] a)
    {
        scoreAudioMap = new Dictionary<string, AudioSource>
        {
            { "0", a[0] },
            { "1", a[1] },
            { "2", a[2] },
            { "3", a[3] },
            { "4", a[4] },
            { "5", a[5] },
            { "6", a[6] },
            { "7", a[7] },
            { "8", a[8] },
            { "9", a[9] },
            { "10", a[10] },
            { "11", a[11] },
            { "12", a[12] },
            { "tied", a[13] },
            { "youup1", a[14] },
            { "oppup1", a[15] },
            { "scoreis", a[16] },
            { "to", a[17] },
            { "yourserve", a[18] },
            { "oppserve", a[19] },
            { "readygo", a[20] },
            { "nextball", a[21] },
            { "congrats", a[22] },
            { "lost", a[23] },
            { "thanks", a[24] },
            { "welcome", a[25] },
            { "welcomemus", a[26] }

        };
    }

    // Use this for initialization
    void Start()
    {
        AudioSource[] audioSources = GetComponents<AudioSource>();
        FillAudioMap(audioSources);
    }

    public static AudioSource PlayAudio(string arg)
    {
        AudioSource val;
        if (scoreAudioMap.TryGetValue(arg, out val))
        {
            val.Play();
        }
        else
        {
            Debug.LogError("Sound Value Not Valid");
        }
        return val;
    }
}
