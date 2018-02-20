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

    internal static bool ExpBallLose { get; set; }

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
        PlayerScore = 11;
        OpponentScore = 10;
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
        if(GameUtils.playState != GameUtils.GamePlayState.ExpMode)
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
        else if(other.tag == "Ball" && GameUtils.playState == GameUtils.GamePlayState.ExpMode)
        {
            if(gameObject.tag == "SouthGoal")
            {
                PlayLoseSound();
                ExpBallLose = true;
                Destroy(other.gameObject);
            }
        }
    }

    /// <summary>
    /// Static method that reads the score of the current game. Note it is ugly because you can't
    /// static Coroutines in static methods.
    /// </summary>
    /// <returns></returns>
    public static IEnumerator ReadScore()
    {
        var ball = GameObject.FindGameObjectWithTag("Ball");
        ball.transform.position = new Vector3(0, 5, 0);
        ball.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);

        if(PlayerScore <= 12 && OpponentScore <= 12)
        {
            //Read the score normal
            theScoreIsAudio.Play();
            yield return new WaitForSeconds(theScoreIsAudio.clip.length);
            AudioSource audioScore = null;
            if (PlayerScore <= 12)
                audioScore = NumberSpeech.PlayAudio(PlayerScore.ToString());
            yield return new WaitForSeconds(audioScore.clip.length);
            toScoreAudio.Play();
            yield return new WaitForSeconds(toScoreAudio.clip.length);
            AudioSource oppoScore = null;
            if (OpponentScore <= 12)
                oppoScore = NumberSpeech.PlayAudio(OpponentScore.ToString());
            yield return new WaitForSeconds(oppoScore.clip.length);
        }
        else if(PlayerScore >= 10 && OpponentScore >= 10)
        {
            //Must win by two
            if (PlayerScore == OpponentScore)
            {
                //Tied
                tiedAudio.Play();
                yield return new WaitForSeconds(tiedAudio.clip.length);
            }
            else if(PlayerScore > OpponentScore)
            {
                //Player Up1
                playerUpAudio.Play();
                yield return new WaitForSeconds(playerUpAudio.clip.length);
            }
            else if(OpponentScore > PlayerScore)
            {
                opponentUpAudio.Play();
                yield return new WaitForSeconds(opponentUpAudio.clip.length);
            }
        }
        if((PlayerScore >= 11 && OpponentScore < 10) ||  
            ((PlayerScore >= 10 && OpponentScore >= 10) && (PlayerScore > OpponentScore + 1)))
        {
            gameOver = true;
            playerWins.Play();
            yield return new WaitForSeconds(playerWins.clip.length);
            SceneManager.LoadSceneAsync("Menu", LoadSceneMode.Single);
        }
        else if ((OpponentScore >= 11 && PlayerScore < 10)||
            ((OpponentScore >= 10 && PlayerScore >= 10) && (OpponentScore > PlayerScore + 1)))
        {
            gameOver = true;
            opponentWins.Play();
            yield return new WaitForSeconds(opponentWins.clip.length);
            SceneManager.LoadSceneAsync("Menu", LoadSceneMode.Single);
        }
        if (GameUtils.PlayerServe)
        {
            AudioSource t = NumberSpeech.PlayAudio("yourserve");
            yield return new WaitForSeconds(t.clip.length - 0.7f);
        }
        else
        {
            AudioSource t = NumberSpeech.PlayAudio("oppserve");
            yield return new WaitForSeconds(t.clip.length - 0.7f);
        }
    }

    private static IEnumerator PlayServeSound()
    {
        if (GameUtils.PlayerServe)
        {
            AudioSource t = NumberSpeech.PlayAudio("yourserve");
            yield return new WaitForSeconds(t.clip.length - 0.7f);
        }
        else
        {
            AudioSource t = NumberSpeech.PlayAudio("oppserve");
            yield return new WaitForSeconds(t.clip.length - 0.7f);
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
