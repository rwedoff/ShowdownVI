using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GoalScript : MonoBehaviour {
    public Text southScoreText;
    public Text northScoreText;
    public static int PlayerScore;
    public static int OpponentScore;
    public static bool gameOver;

    private static AudioSource winPointAudio;
    private static AudioSource losePointAudio;
    private static AudioSource theScoreIsAudio;
    private static AudioSource toScoreAudio;
    private static AudioSource playerUpAudio;
    private static AudioSource opponentUpAudio;
    private static AudioSource tiedAudio;
    private static AudioSource playerWins;
    private static AudioSource opponentWins;
    private static Dictionary<int, AudioSource> scoreAudioMap;

    private void Start()
    {
        PlayerScore = 0;
        OpponentScore = 0;
        AudioSource[] audioSources = transform.parent.GetComponents<AudioSource>();
        winPointAudio = audioSources[0];
        losePointAudio = audioSources[1];
        theScoreIsAudio = audioSources[14];
        toScoreAudio = audioSources[15];
        playerUpAudio = audioSources[16];
        opponentUpAudio = audioSources[17];
        tiedAudio = audioSources[18];
        playerWins = audioSources[19];
        opponentWins = audioSources[20];
        gameOver = false;

        FillAudioMap(audioSources);
    }

    private void FillAudioMap(AudioSource[] a)
    {
        scoreAudioMap = new Dictionary<int, AudioSource>();
        scoreAudioMap.Add(0, a[2]);
        scoreAudioMap.Add(1, a[3]);
        scoreAudioMap.Add(2, a[4]);
        scoreAudioMap.Add(3, a[5]);
        scoreAudioMap.Add(4, a[6]);
        scoreAudioMap.Add(5, a[7]);
        scoreAudioMap.Add(6, a[8]);
        scoreAudioMap.Add(7, a[9]);
        scoreAudioMap.Add(8, a[10]);
        scoreAudioMap.Add(9, a[11]);
        scoreAudioMap.Add(10, a[12]);
        scoreAudioMap.Add(11, a[13]);
        
    }

    private void Update()
    {
        northScoreText.text = "North Score: " + OpponentScore;
        southScoreText.text = "South Score: " + PlayerScore;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Ball") {
            if (gameObject.tag == "SouthGoal")
            {
                OpponentScore += 2;
                northScoreText.text = "North Score: " + OpponentScore;
                PlayLoseSound();
                BallScript.ballStart = true;

            }
            else if (gameObject.tag == "NorthGoal")
            {
                PlayerScore += 2;
                southScoreText.text = "South Score: " + PlayerScore;
                PlayWinSound();
                BallScript.ballStart = true;
            }
            GameUtils.ResetBall(other.gameObject);
            StartCoroutine(ReadScore());
        }
    }

    public static IEnumerator ReadScore()
    {
        if(PlayerScore >= 11 && OpponentScore <= 10)
        {
            gameOver = true;
            playerWins.Play();
            yield return new WaitForSeconds(playerWins.clip.length - 1f);
            SceneManager.LoadSceneAsync("Main");
        }
        else if(OpponentScore >= 11 && PlayerScore <= 10)
        {
            gameOver = true;
            opponentWins.Play();
            yield return new WaitForSeconds(opponentWins.clip.length - 1f);
            //Destroy(GetComponent<MenuSpeech>());
            SceneManager.LoadSceneAsync("Main");
        }
        else if (PlayerScore >= 11 && OpponentScore >= 11)
        {
            if(PlayerScore > OpponentScore)
            {
                playerUpAudio.Play();
            }
            else if(PlayerScore < OpponentScore)
            {
                opponentUpAudio.Play();
            }
            else
            {
                tiedAudio.Play();
            }
        }
        else
        {
            theScoreIsAudio.Play();
            yield return new WaitForSeconds(theScoreIsAudio.clip.length - 0.7f);
            scoreAudioMap[PlayerScore].Play();
            yield return new WaitForSeconds(scoreAudioMap[PlayerScore].clip.length -0.5f);
            toScoreAudio.Play();
            yield return new WaitForSeconds(toScoreAudio.clip.length - 0.7f);
            scoreAudioMap[OpponentScore].Play();
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
