using System.Collections.Generic;
using UnityEngine;

public class NumberSpeech : MonoBehaviour
{
    private static Dictionary<int, AudioSource> scoreAudioMap;

    private void FillAudioMap(AudioSource[] a)
    {
        scoreAudioMap = new Dictionary<int, AudioSource>
        {
            { 0, a[0] },
            { 1, a[1] },
            { 2, a[2] },
            { 3, a[3] },
            { 4, a[4] },
            { 5, a[5] },
            { 6, a[6] },
            { 7, a[7] },
            { 8, a[8] },
            { 9, a[9] },
            { 10, a[10] },
            { 11, a[11] },
            { 12, a[12] },
            { 13, a[13] },
            { 14, a[14] },
            { 15, a[15] },
            { 16, a[16] },
            { 17, a[17] },
            { 18, a[18] }
        };
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
