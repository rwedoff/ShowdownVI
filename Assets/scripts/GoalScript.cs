using System;
using UnityEngine;
using UnityEngine.UI;

public class GoalScript : MonoBehaviour {
    public Text southScoreText;
    public Text northScoreText;
    public static int SouthScore;
    public static int NorthScore;
    private static AudioSource winPointAudio;
    private static AudioSource losePointAudio;

    private void Start()
    {
        SouthScore = 0;
        NorthScore = 0;
        winPointAudio = transform.parent.GetComponents<AudioSource>()[0];
        losePointAudio = transform.parent.GetComponents<AudioSource>()[1];
    }

    private void Update()
    {
        northScoreText.text = "North Score: " + NorthScore;
        southScoreText.text = "South Score: " + SouthScore;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Ball") {
            if (gameObject.tag == "SouthGoal")
            {
                NorthScore += 2;
                northScoreText.text = "North Score: " + NorthScore;
                PlayLoseSound();
                BallScript.ballStart = true;

            }
            else if (gameObject.tag == "NorthGoal")
            {
                SouthScore += 2;
                southScoreText.text = "South Score: " + SouthScore;
                PlayWinSound();
                BallScript.ballStart = true;
            }
            Utils.ResetBall(other.gameObject, true);
        }
    }

    internal static void PlayWinSound()
    {
        winPointAudio.Play();
    }

    internal static void PlayLoseSound()
    {
        losePointAudio.Play();
    }
}
