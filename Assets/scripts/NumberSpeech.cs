using System.Collections.Generic;
using UnityEngine;

public class NumberSpeech : MonoBehaviour
{
    private static Dictionary<int, AudioSource> scoreAudioMap;

    private void FillAudioMap(AudioSource[] a)
    {
        scoreAudioMap = new Dictionary<int, AudioSource>();
        scoreAudioMap.Add(0, a[0]);
        scoreAudioMap.Add(1, a[1]);
        scoreAudioMap.Add(2, a[2]);
        scoreAudioMap.Add(3, a[3]);
        scoreAudioMap.Add(4, a[4]);
        scoreAudioMap.Add(5, a[5]);
        scoreAudioMap.Add(6, a[6]);
        scoreAudioMap.Add(7, a[7]);
        scoreAudioMap.Add(8, a[8]);
        scoreAudioMap.Add(9, a[9]);
        scoreAudioMap.Add(10, a[10]);
        scoreAudioMap.Add(11, a[11]);
        scoreAudioMap.Add(12, a[12]);
        scoreAudioMap.Add(13, a[13]);
        scoreAudioMap.Add(14, a[14]);

    }

    // Use this for initialization
    void Start()
    {
        AudioSource[] audioSources = GetComponents<AudioSource>();
        FillAudioMap(audioSources);
    }

    public static AudioSource PlayAudio(int num)
    {
        AudioSource val;
        if (scoreAudioMap.TryGetValue(num, out val))
        {
            val.Play();
        }
        return val;
    }
}
