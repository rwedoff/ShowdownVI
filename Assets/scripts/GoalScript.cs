using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GoalScript : MonoBehaviour {
    public Text scoreText;
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

    private void Start()
    {
        PlayerScore = 0;
        OpponentScore = 0;
        AudioSource[] audioSources = transform.parent.GetComponents<AudioSource>();
        winPointAudio = audioSources[0];
        losePointAudio = audioSources[1];
        theScoreIsAudio = audioSources[2];
        toScoreAudio = audioSources[3];
        playerUpAudio = audioSources[4];
        opponentUpAudio = audioSources[5];
        tiedAudio = audioSources[6];
        playerWins = audioSources[7];
        opponentWins = audioSources[8];
        gameOver = false;
    }

    private void Update()
    {
        scoreText.text = "Player " + PlayerScore + " - " + OpponentScore + " Opponent";
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Ball" && GameUtils.playState == GameUtils.GamePlayState.InPlay) {
            if (gameObject.tag == "SouthGoal")
            {
                OpponentScore += 2;
                PlayLoseSound();
                GameUtils.playState = GameUtils.GamePlayState.SettingBall;

            }
            else if (gameObject.tag == "NorthGoal")
            {
                PlayerScore += 2;
                PlayWinSound();
                GameUtils.playState = GameUtils.GamePlayState.SettingBall;
            }
            GameUtils.ResetBall(other.gameObject);
            scoreText.text = "Player " + PlayerScore + " - " + OpponentScore + " Opponent";
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
            SceneManager.LoadSceneAsync("Main", LoadSceneMode.Single);
        }
        else if(OpponentScore >= 11 && PlayerScore <= 10)
        {
            gameOver = true;
            opponentWins.Play();
            yield return new WaitForSeconds(opponentWins.clip.length - 1f);
            //Destroy(GetComponent<MenuSpeech>());
            SceneManager.LoadSceneAsync("Main", LoadSceneMode.Single);
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
            AudioSource audioScore = null;
            if (PlayerScore <= 11)
                audioScore = NumberSpeech.PlayAudio(PlayerScore);
            yield return new WaitForSeconds(audioScore.clip.length -0.5f);
            toScoreAudio.Play();
            yield return new WaitForSeconds(toScoreAudio.clip.length - 0.7f);
            AudioSource oppoScore = null;
            if (OpponentScore <= 11)
                oppoScore = NumberSpeech.PlayAudio(OpponentScore);
            yield return new WaitForSeconds(oppoScore.clip.length - 0.7f);
            if (GameUtils.PlayerServe)
            {
                AudioSource t = NumberSpeech.PlayAudio(17);
                yield return new WaitForSeconds(t.clip.length - 0.7f);
            }
            else
            {
                AudioSource t = NumberSpeech.PlayAudio(18);
                yield return new WaitForSeconds(t.clip.length - 0.7f);
            }
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
