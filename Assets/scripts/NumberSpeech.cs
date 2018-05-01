using System.Collections;
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
            { "welcomemus", a[26] },
            { "13", a[27] },
            { "14", a[28] },
            { "15", a[29] },
            { "16", a[30] },
            { "17", a[31] },
            { "18", a[32] },
            { "19", a[33] },
            { "20", a[34] },
            { "30", a[35] },
            { "40", a[36] },
            { "50", a[37] },
            { "60", a[38] },
            { "70", a[39] },
            { "80", a[40] },
            { "90", a[41] },
            { "youhave", a[42] },
            { "points", a[43] },
            { "by", a[44] },
            { "foot", a[45] },
            { "pen", a[46] },
            { "inches", a[47] }
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
        AudioSource selectedAudio;
        string lowArg = arg.ToLower();
        if (scoreAudioMap.TryGetValue(lowArg, out selectedAudio))
        {
            selectedAudio.Play();
        }
        else
        {
            Debug.LogError("Sound Value Not Valid");
        }
        return selectedAudio;
    }

    /// <summary>
    /// Plays audio number in a range of 0 - 99.
    /// Ex: "You Have 84 points"
    /// </summary>
    /// <param name="points"></param>
    /// <returns></returns>
    public IEnumerator PlayExpPointsAudio(int points)
    {
        string pointStr = points.ToString();
        var aud = PlayAudio("youhave");
        yield return new WaitForSeconds(aud.clip.length);
        if (points <= 19)
        {
            var aud0 = PlayAudio(pointStr);
            yield return new WaitForSeconds(aud0.clip.length);
        }
        else
        {
            string firstDigit = pointStr.Substring(0, 1) + "0";
            string secondDigit = pointStr.Substring(1, 1);
            var aud1 = PlayAudio(firstDigit);
            yield return new WaitForSeconds(aud1.clip.length);
            if (!secondDigit.Equals("0"))
            {
                var aud2 = PlayAudio(secondDigit);
                yield return new WaitForSeconds(aud2.clip.length);
            }
        }
        PlayAudio("points");
    }

    public IEnumerator PlayFancyNumberAudio(int num)
    {
        var aud = PlayAudio("by");
        yield return new WaitForSeconds(aud.clip.length);

        int inchNum = (int)(num / 2.54);
        if (inchNum > 10 && inchNum < 15)
        {
            Debug.Log("Foot");
            var aud1 = PlayAudio("foot");
            yield return new WaitForSeconds(aud1.clip.length);
            yield break;
        }
        else if (inchNum > 4 && inchNum < 8)
        {
            Debug.Log("Pen");
            var aud1 = PlayAudio("pen");
            yield return new WaitForSeconds(aud1.clip.length);
            yield break;
        }

        string numStr = inchNum.ToString();
        if (inchNum <= 19)
        {
            Debug.Log("HEHEHEHE: " + numStr);
            var aud0 = PlayAudio(numStr);
            yield return new WaitForSeconds(aud0.clip.length);
        }
        else
        {
            string firstDigit = numStr.Substring(0, 1) + "0";
            string secondDigit = numStr.Substring(1, 1);
            var aud1 = PlayAudio(firstDigit);
            yield return new WaitForSeconds(aud1.clip.length);
            if (!secondDigit.Equals("0"))
            {
                var aud2 = PlayAudio(secondDigit);
                yield return new WaitForSeconds(aud2.clip.length);
            }
        }
        var aud3 = PlayAudio("inches");
        yield return new WaitForSeconds(aud3.clip.length);
    }
}
