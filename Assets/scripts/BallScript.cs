using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class BallScript : MonoBehaviour
{
    public float inputSpeed;
    public AudioMixerSnapshot farSideSnap;
    public AudioMixerSnapshot closeSideSnap;
    public bool keyboardControl;

    public static bool GameInit;

    internal static bool BallHitOnce { get; set; }

    private Rigidbody rb;
    private static AudioSource ballSoundSource;
    private AudioSource paddleSound;
    private AudioSource winSound;
    private AudioSource lostSound;
    private AudioSource outofTableSound;
    private AudioSource wallAudioSource;
    private const float maxspeed = 250;
    private float oldTime;
    private bool timerStarted;
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
        paddleSound = audioSources[1];
        outofTableSound = audioSources[2];
        wallAudioSource = audioSources[3];
        BallHitOnce = false;
    }

    //Used for physics
    //Game logic placed in here...bad idea, sorry.
    //A todo item would be to move the game state stuff in a game manager script or something.
    private void FixedUpdate()
    {
        if (keyboardControl)
        {
            //DEBUG ONLY
            //Manual Keyboard Control of Ball
            float movehorizontal = Input.GetAxis("Horizontal");
            float movevertical = Input.GetAxis("Vertical");
            Vector3 movement = new Vector3(movehorizontal, 0.0f, movevertical);
            rb.AddForce(movement * inputSpeed);
            //END DEBUG
        }
        //Check ball state
        if (GameInit)
        {
            //DEBUG
            //GameUtils.playState = GameUtils.GamePlayState.InPlay;
            //END DEBUG
            return;
        }
        else if(GameUtils.playState == GameUtils.GamePlayState.SettingBall)
        {
            SettingBall();
        }
        else if(GameUtils.playState == GameUtils.GamePlayState.InPlay ||
            GameUtils.playState == GameUtils.GamePlayState.ExpMode)
        {
            StartBallSound();

            DynamicAudioChanges();

            //Add a speed limit to the ball
            Vector3 oldVel = rb.velocity;
            rb.velocity = Vector3.ClampMagnitude(oldVel, maxspeed);

            //DEBUG ONLY uncomment this in Prod
            if (GameUtils.ballSpeedPointsEnabled)
            {
                BallSpeedPoints();
            }

            CheckBallInGame();
        }
        else if(GameUtils.playState == GameUtils.GamePlayState.BallSet)
        {
            BallSet();
        }

        if (GoalScript.gameOver)
        {
            rb.position = new Vector3(0, transform.position.y, 0);
        }

    }

    /// <summary>
    /// Modifys the ball sound based on where the ball is and the speed of the ball
    /// </summary>
    private void DynamicAudioChanges()
    {
        //Change and limit pitch change on ball
        ballSoundSource.pitch = GameUtils.Scale(0, maxspeed, 0.8f, 1.25f, Math.Abs(rb.velocity.magnitude));

        //Change rolling sounds based on speed of ball
        //ballSoundSource.volume = GameUtils.Scale(0, maxspeed, 0, 1, Math.Abs(rb.velocity.magnitude));

        //Dynamic LowPass Audio filter snapshot changing when ball passes halfway point
        if (rb.position.z > 0)
        {
            farSideSnap.TransitionTo(0.1f);
        }
        else
        {
            closeSideSnap.TransitionTo(0.1f);
        }

        //Change Stereo Pan in buckets
        if (rb.position.x < -30)
        {
            ballSoundSource.panStereo = -1;
        }
        else if (rb.position.x >= -30 && rb.position.x < -10)
        {
            ballSoundSource.panStereo = -0.5f;
        }
        else if (rb.position.x >= -10 && rb.position.x < 10)
        {
            ballSoundSource.panStereo = 0;
        }
        else if (rb.position.x >= 10 && rb.position.x < 30)
        {
            ballSoundSource.panStereo = 0.5f;
        }
        else if (rb.position.x >= 30)
        {
            ballSoundSource.panStereo = 1;
        }
    }

    /// <summary>
    /// State changes when the ball is set for a serve
    /// </summary>
    public void BallSet()
    {
        ballSoundSource.pitch = 0.35f;
        StartBallSound();
    }

    /// <summary>
    /// Sets the state of the ball when the player is setting the ball for a serve.
    /// </summary>
    public void SettingBall()
    {
        ballSoundSource.Pause();
        SetBall();
    }

    /// <summary>
    /// Method that checks if the ball is within the bounds of the table. If not, then reset the serve.
    /// This is just a fail safe if the ball phases through the table or something.
    /// </summary>
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
        if (JoyconController.ButtonPressed || PaddleScript.ScreenPressDown)
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
        yield return new WaitForSeconds(6);
        GameUtils.playState = GameUtils.GamePlayState.BallSet;
        StartCoroutine(PauseForBallSetAudio(new Vector3(0, transform.position.y, 100)));
    }

    private IEnumerator PauseForBallSetAudio(Vector3 ballPos)
    {
        rb.velocity = new Vector3(0, 0, 0);
        rb.position = ballPos;
        AudioSource setAudioSource = NumberSpeech.PlayAudio("readygo");
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
            float impulse = collision.impulse.sqrMagnitude;
            if(collision.gameObject.tag == "Player")
            {
                float rumbleAmp = GameUtils.Scale(0, 243382, 0.3f, 0.9f, impulse);
                JoyconController.RumbleJoycon(160, 320, rumbleAmp, 200);
                if(GameUtils.playState == GameUtils.GamePlayState.ExpMode)
                {
                    ExpManager.CollisionSnapshot = new ExpManager.Snapshot() {
                        batPos = collision.gameObject.transform.position,
                        ballPos = transform.position
                    };
                }
            }
            paddleSound.volume = GameUtils.Scale(0, 243382, 0.07f, 0.3f, impulse);
            paddleSound.Play();
            BallHitOnce = true;
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
