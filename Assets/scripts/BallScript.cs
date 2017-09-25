using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BallScript : MonoBehaviour
{

    private Rigidbody rb;
    public float inputSpeed;
    private AudioSource ballSoundSource;
    
    private AudioSource paddleSound;
    private float maxspeed = 250;

    private void Start()
    {

        rb = GetComponent<Rigidbody>();

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
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            paddleSound.Play();
        }
    }
}
