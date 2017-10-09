using System;
using UnityEngine;

public class BallScript : MonoBehaviour
{
    public float inputSpeed;

    public static bool ballStart;
    public static bool tutorialMode;

    private Rigidbody rb;
    private AudioSource ballSoundSource;
    private AudioSource paddleSound;
    private AudioSource winSound;
    private AudioSource lostSound;
    private AudioSource outofTableSound;
    private float maxspeed = 250;
    private float oldTime;
    private bool timerStarted;
    private bool debugMode = true;
    private GoalScript gs;

    private void Start()
    {
        ballStart = true;
        rb = GetComponent<Rigidbody>();
        oldTime = 0;
        timerStarted = false;
        AudioSource[] audioSources = GetComponents<AudioSource>();
        ballSoundSource = audioSources[0];
        paddleSound = audioSources[1];
        outofTableSound = audioSources[2];
        gs = GetComponent<GoalScript>();

        //DEBUG ONLY
        tutorialMode = false;
    }

    //Used for physics
    private void FixedUpdate()
    {
        //Manual Keyboard Control of Ball
        float movehorizontal = Input.GetAxis("Horizontal");
        float movevertical = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(movehorizontal, 0.0f, movevertical);
        rb.AddForce(movement * inputSpeed);

        //Change rolling sounds based on speed of ball
        ballSoundSource.volume = GameUtils.Scale(0, maxspeed, 0, 1, Math.Abs(rb.velocity.magnitude));
        ballSoundSource.pitch = GameUtils.Scale(0, maxspeed, 0.5f, 1.25f, Math.Abs(rb.velocity.magnitude));

        //Add a speed limit to the ball
        Vector3 oldVel = rb.velocity;
        rb.velocity = Vector3.ClampMagnitude(oldVel, maxspeed);

        if (tutorialMode)
        {
            BallTutorial();
        }
        else
        {
            //Toggle Ball Speed Points
            BallSpeedPoints();
        }

        BallServe();

        CheckBallInGame();

        if (GoalScript.gameOver)
        {
            Destroy(this);
            Destroy(GetComponent<Rigidbody>());
        }
    }

    private void CheckBallInGame()
    {
        if(rb.position.x < -61 || rb.position.x > 61 || rb.position.z < -175 || rb.position.z > 175)
        {
            outofTableSound.Play();
            //RESET SERVE
            ballStart = true;
            ResetBallPosition();
        }
    }

    private void BallServe()
    {
        if (!debugMode)
        {
            if (BodySourceView.oppositeHand.Z == 0 && GameUtils.playerServe && ballStart)
            {
                ResetBallPosition();
            }
            else if (GameUtils.playerServe && ballStart)
            {
                Windows.Kinect.CameraSpacePoint serveHand = BodySourceView.oppositeHand;
                Windows.Kinect.CameraSpacePoint midSpinePosition = BodySourceView.baseKinectPosition;

                //Calculate the position of the paddle based on the distance from the mid spine join
                float xPos = (midSpinePosition.X - serveHand.X) * 100,
                      zPos = (midSpinePosition.Z - serveHand.Z) * 100;

                //Smooth and set the position of the paddle
                //Smoothing used so paddle won't phase through ball
                Vector3 direction = (new Vector3(-xPos, 0, (zPos - 188.5f)) - transform.position).normalized;
                rb.MovePosition(transform.position + (direction * 400 * Time.deltaTime));
            }
        }
    }

    private void ResetBallPosition()
    {
        if (GameUtils.playerServe && ballStart)
        {
            rb.MovePosition(new Vector3(0, transform.position.y, -120));
        }
        else
        {
            rb.MovePosition(new Vector3(0, transform.position.y, 120));
        }
    }

    private void BallSpeedPoints()
    {
        if (rb.velocity.magnitude < 8 && !ballStart)
        {
            if (timerStarted)
            {
                if (Time.time > oldTime + 2)
                {
                    GameUtils.ResetBall(transform.gameObject);
                    timerStarted = false;
                    ballStart = true;

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
                    GoalScript.ReadScore();
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
        if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "oppo")
        {
            paddleSound.Play();
            ballStart = false;
        }
    }

    private void BallTutorial()
    {
        //TODO remove point system in tutoiral mode.

    }
}
