using System;
using System.Collections;
using UnityEngine;

public class BallScriptTwoTone : MonoBehaviour
{
    public float inputSpeed;
    private Rigidbody rb;
    private static AudioSource ballSoundSource;
    private static AudioSource opBallSoundSource;
    private AudioSource paddleSound;
    private AudioSource winSound;
    private AudioSource lostSound;
    private AudioSource outofTableSound;
    private AudioSource wallAudioSource;
    private float maxspeed = 250;
    private float oldTime;
    private bool timerStarted;
    private bool debugMode = true;
    private GameObject bat;
    private bool aiSettingBall = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        oldTime = 0;
        timerStarted = false;
        AudioSource[] audioSources = GetComponents<AudioSource>();
        bat = GameObject.FindGameObjectWithTag("Player");
        ballSoundSource = audioSources[0];
        opBallSoundSource = audioSources[1];
        paddleSound = audioSources[2];
        outofTableSound = audioSources[3];
        wallAudioSource = audioSources[4];
    }

    //Used for physics
    private void FixedUpdate()
    {
        //DEBUG ONLY

        //Manual Keyboard Control of Ball
        float movehorizontal = Input.GetAxis("Horizontal");
        float movevertical = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(movehorizontal, 0.0f, movevertical);
        rb.AddForce(movement * inputSpeed);

        //END DEBUG

        //Check ball state
        if (PhoneServer.Init)
        {
            //DEBUG
            GameUtils.playState = GameUtils.GamePlayState.InPlay;
            //END DEBUG
            return;
        }
        else if(GameUtils.playState == GameUtils.GamePlayState.SettingBall)
        {
            SettingBall();
        }
        else if(GameUtils.playState == GameUtils.GamePlayState.InPlay)
        {
            StartBallSound();
            //Change rolling sounds based on speed of ball
            //ballSoundSource.volume = GameUtils.Scale(0, maxspeed, 0, 1, Math.Abs(rb.velocity.magnitude));
            opBallSoundSource.pitch = GameUtils.Scale(0, maxspeed, 0.25f, 1f, Math.Abs(rb.velocity.magnitude));
            ballSoundSource.pitch = GameUtils.Scale(0, maxspeed, 0.25f, 1f, Math.Abs(rb.velocity.magnitude));

            //Add a speed limit to the ball
            Vector3 oldVel = rb.velocity;
            rb.velocity = Vector3.ClampMagnitude(oldVel, maxspeed);

            //DEBUG ONLY uncomment this
            BallSpeedPoints();

            CheckBallInGame();
        }
        else if(GameUtils.playState == GameUtils.GamePlayState.BallSet)
        {
            BallSet();
        }

        if (GoalScript.gameOver)
        {
            Destroy(this);
            Destroy(GetComponent<Rigidbody>());
        }
    }

    public void BallSet()
    {
        ballSoundSource.pitch = 0.35f;
        StartBallSound();
    }

    public void SettingBall()
    {
        ballSoundSource.Pause();
        SetBall();
    }

    private void CheckBallInGame()
    {
        if(rb.position.x < -51 || rb.position.x > 51 || rb.position.z < -130 || rb.position.z > 130)
        {
            outofTableSound.Play();
            //RESET SERVE
            GameUtils.playState = GameUtils.GamePlayState.SettingBall;
            rb.isKinematic = true;
            SetBallForServe();
        }
    }

    private void SetBall()
    {
        if (GameUtils.PlayerServe && GameUtils.playState == GameUtils.GamePlayState.SettingBall)
        {
            SetBallForServe();
        }
        else if (!GameUtils.PlayerServe && GameUtils.playState == GameUtils.GamePlayState.SettingBall)
        {
            SetBallForAIServe();
        }
    }

    private void SetBallForAIServe()
    {
        if (!aiSettingBall)
        {
            StartCoroutine(PauseSettingForScoreAudio());
        }
    }

    //Sets the position of the ball
    private void SetBallForServe()
    {
        var paddlePos = bat.transform.position;
        if (PaddleScript.ScreenPressDown)
        {
            if (paddlePos.x < 50 && paddlePos.x > -50 && paddlePos.z > -130)
            {
                GameUtils.playState = GameUtils.GamePlayState.BallSet;
                StartCoroutine(PauseForBallSetAudio(new Vector3(paddlePos.x, transform.position.y, paddlePos.z + 10)));
            }
        }
        else //Still thinking where to place ball
        {
            rb.MovePosition(new Vector3(paddlePos.x, transform.position.y, paddlePos.z + 10));
            GameUtils.playState = GameUtils.GamePlayState.SettingBall;
            rb.isKinematic = true;
        }
    }

    private IEnumerator PauseSettingForScoreAudio()
    {
        aiSettingBall = true;
        rb.velocity = new Vector3(0, 0, 0);
        yield return new WaitForSeconds(5);
        GameUtils.playState = GameUtils.GamePlayState.BallSet;
        StartCoroutine(PauseForBallSetAudio(new Vector3(0, transform.position.y, 100)));
    }

    private IEnumerator PauseForBallSetAudio(Vector3 ballPos)
    {
        rb.velocity = new Vector3(0, 0, 0);
        rb.position = ballPos;
        AudioSource setAudioSource = NumberSpeech.PlayAudio(16);
        yield return new WaitForSeconds(setAudioSource.clip.length - 0.5f); //Give 1 second to move the bat way bc of jitters
        GameUtils.playState = GameUtils.GamePlayState.InPlay; //Maybe remove this
        aiSettingBall = false;
        rb.isKinematic = false;
    }

    private void BallSpeedPoints()
    {
        if (rb.velocity.magnitude < 8 && GameUtils.playState == GameUtils.GamePlayState.InPlay)
        {
            if (timerStarted)
            {
                if (Time.time > oldTime + 2)
                {
                    GameUtils.ResetBall(transform.gameObject);
                    timerStarted = false;
                    GameUtils.playState = GameUtils.GamePlayState.SettingBall;
                    rb.isKinematic = true;
                    if (transform.position.z > 0)
                    {
                        GoalScript.PlayerScore++;
                        GoalScript.PlayWinSound();
                    }
                    else
                    {
                        GoalScript.OpponentScore++;
                        GoalScript.PlayLoseSound();
                    }
                    StartCoroutine(GoalScript.ReadScore());
                }
            }
            else
            {
                timerStarted = true;
                oldTime = Time.time;
            }
        }
        else
        {
            timerStarted = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if ((collision.gameObject.tag == "Player" || collision.gameObject.tag == "oppo") && GameUtils.playState == GameUtils.GamePlayState.BallSet)
        {
            paddleSound.Play();
            GameUtils.playState = GameUtils.GamePlayState.InPlay;
            rb.isKinematic = false;
        }
        if(collision.gameObject.tag == "Player"  || collision.gameObject.tag == "oppo" && GameUtils.playState != GameUtils.GamePlayState.SettingBall)
        {
            paddleSound.Play();
            if(collision.gameObject.tag == "Player")
            {
                PhoneServer.SendMessageToPhone("ball;");
            }
        }
        if(collision.gameObject.tag == "Wall" && !wallAudioSource.isPlaying)
        {
            wallAudioSource.Play();
        }
    }

    private static void StartBallSound()
    {
        if (!ballSoundSource.isPlaying)
        {
            ballSoundSource.Play();
        }
    }
}
