using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallScript : MonoBehaviour {
    private AudioSource wallSound;

    // Use this for initialization
    void Start () {
        wallSound = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Ball" && !wallSound.isPlaying)
        {
            wallSound.Play();
        }
    }
}
