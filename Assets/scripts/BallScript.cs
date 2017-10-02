using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BallScript : MonoBehaviour
{
    public float inputSpeed;

    public static bool ballStart;

    private Rigidbody rb;
    private AudioSource ballSoundSource;
    private AudioSource paddleSound;
    private AudioSource winSound;
    private AudioSource lostSound;
    private float maxspeed = 250;
    private float oldTime;
    private bool timerStarted;

    private void Start()
    {
        ballStart = true;
        rb = GetComponent<Rigidbody>();
        oldTime = 0;
        timerStarted = false;
        AudioSource[] audioSources = GetComponents<AudioSource>();
        ballSoundSource = audioSources[0];
        paddleSound = audioSources[1];
    }

    //Used for physics
    private void FixedUpdate()
    {
        float movehorizontal = Input.GetAxis("Horizontal");
        float movevertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(movehorizontal, 0.0f, movevertical);
        rb.AddForce(movement * inputSpeed);

        //Sound code
        ballSoundSource.volume = Utils.Scale(0, maxspeed, 0, 1, Math.Abs(rb.velocity.magnitude));
        ballSoundSource.pitch = Utils.Scale(0, maxspeed, 0.5f, 1.25f, Math.Abs(rb.velocity.magnitude));

        //Add a speed limit to the ball
        Vector3 oldVel = rb.velocity;
        rb.velocity = Vector3.ClampMagnitude(oldVel, maxspeed);
        
        if(rb.velocity.magnitude < 8 && !ballStart)
        {
            if (timerStarted)
            {
                if(Time.time > oldTime + 2)
                {
                    Utils.ResetBall(transform.gameObject, true);
                    timerStarted = false;
                    ballStart = true;
                    
                    if(transform.position.z > 0)
                    {
                        GoalScript.SouthScore++;
                        GoalScript.PlayWinSound();

                    }
                    else
                    {
                        GoalScript.NorthScore++;
                        GoalScript.PlayLoseSound();
                    }
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
        if (collision.gameObject.tag == "Player")
        {
            paddleSound.Play();
            ballStart = false;
        }
    }
}
