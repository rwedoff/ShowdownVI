using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BallScript : MonoBehaviour
{

    private Rigidbody rb;
    public float inputSpeed;
    private AudioSource ballSoundSource;
    private AudioSource wallSound;
    private AudioSource paddleSound;

    private void Start()
    {

        rb = GetComponent<Rigidbody>();

        AudioSource[] audioSources = GetComponents<AudioSource>();
        ballSoundSource = audioSources[0];
        wallSound = audioSources[1];
        paddleSound = audioSources[2];
    }

    //Used for physics
    private void FixedUpdate()
    {


        float movehorizontal = Input.GetAxis("Horizontal");
        float movevertical = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(movehorizontal, 0.0f, movevertical);
        rb.AddForce(movement * inputSpeed);

        //Sound code        
        //ballSoundSource.volume = Utils.Scale(0, 20, 0, 1, rb.velocity.magnitude);

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            wallSound.Play();
        }
        else if (collision.gameObject.tag == "Player")
        {
            paddleSound.Play();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (ballSoundSource.isPlaying == true && collision.gameObject.tag == "Ground")
        {
            //StartCoroutine(VolumeFade(ballSoundSource, 0, 0.5f));
            //ballSoundSource.Pause();
        }
    }

    //Use StartCoroutine();
    private IEnumerator VolumeFade(AudioSource _AudioSource, float _EndVolume, float _FadeLength)
    {

        float _StartVolume = _AudioSource.volume;

        float _StartTime = Time.time;

        while (Time.time < _StartTime + _FadeLength)
        {

            _AudioSource.volume = _StartVolume + ((_EndVolume - _StartVolume) * ((Time.time - _StartTime) / _FadeLength));

            yield return null;

        }

        if (_EndVolume == 0) { _AudioSource.Pause(); }

    }
}
