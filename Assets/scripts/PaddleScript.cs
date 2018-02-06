using System;
using UnityEngine;
using Windows.Kinect;

public class PaddleScript : MonoBehaviour
{
    private Rigidbody rb;

    private bool batOutofBounds;

    private UnityEngine.AudioSource wallCollideAudio;
    //private UnityEngine.AudioSource batDroneAudio;

    public static bool ScreenPressDown { get; internal set; }

    private float oldTime;
    private float timerInterval;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        wallCollideAudio = GetComponents<UnityEngine.AudioSource>()[0];
       // batDroneAudio = GetComponents<UnityEngine.AudioSource>()[1];
        batOutofBounds = true;
        wallCollideAudio.loop = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (PhoneServer.Init)
        {
            //DEBUG ONLY
            //Time.timeScale = 1;
            //END DEBUG
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
        //MoveBat(BodySourceView.spineMidPosition, BodySourceView.handPosition);
        CameraSpacePoint midSpinePosition = BodySourceView.baseKinectPosition;
        CameraSpacePoint handPosition = BodySourceView.handPosition;

        CameraSpacePoint sholderPos = BodySourceView.shoulderPosition;
        float centerXPoint = midSpinePosition.X + (Math.Abs(sholderPos.X - midSpinePosition.X) / 2) ;

        //Calculate the position of the paddle based on the distance from the mid spine join
        float xPos = (centerXPoint - handPosition.X) * 100,
              zPos = (midSpinePosition.Z - handPosition.Z) * 100;
        float yPos = transform.position.y;
        if (ScreenPressDown)
        {
            yPos = 20;
        }
        else
        {
            yPos = 5f;
        }

        Vector3 newPosition = new Vector3(-xPos, yPos, (zPos - 142f));
        rb.MovePosition(Vector3.Lerp(rb.position, newPosition, Time.fixedDeltaTime * 30));

        //DEBUG ONLY
        //float movehorizontal = Input.GetAxis("Horizontal");
        //float movevertical = Input.GetAxis("Vertical");
        //Vector3 movement = new Vector3(movehorizontal, 0.0f, movevertical);
        //rb.MovePosition(transform.position + movement * Time.deltaTime * 300);
        //END DEBUG

        RotateBat(BodySourceView.wristPosition, BodySourceView.handPosition);

        CheckBatInGame();
    }

    private void CheckBatInGame()
    {
        batOutofBounds = false;
        timerInterval = 5;
        if ((transform.position.x > 50 || transform.position.x < -50 || transform.position.z < -145))
        {
            float outOfBoundsBy = 0;
            if(Math.Abs(transform.position.x) > 50)
            {
                outOfBoundsBy = Math.Abs(transform.position.x) - 50;
            }
            else if(transform.position.z < -145)
            {
                outOfBoundsBy = Math.Abs(transform.position.z) -145;
            }

            if (outOfBoundsBy < 10)
            {
                timerInterval = 0.25f;
            }
            else if (outOfBoundsBy < 30)
            {
                timerInterval = 0.2f;
            }
            else if(outOfBoundsBy < 50 )
            {
                timerInterval = 0.15f;
            }
            else
            {
                timerInterval = 0f;
            }
            batOutofBounds = true;

            if(Time.time > oldTime + timerInterval)
            {
                PhoneServer.SendMessageToPhone("wall;");
                oldTime = Time.time;
            }
        }
        if (!batOutofBounds && wallCollideAudio.isPlaying)
        {
            wallCollideAudio.Pause();
            oldTime = Time.time;
        }
        else if (!wallCollideAudio.isPlaying && batOutofBounds)
        {
            wallCollideAudio.Play();
        }
    }

    /// <summary>
    /// Calculates the rotation of the bat in the virtual world
    /// </summary>
    /// <param name="handBasePos">Distance of the base of the hand from the Kinect</param>
    /// <param name="handTipPos">Distance of the tip of the hand from the Kinect</param>
    private void RotateBat(CameraSpacePoint handBasePos, CameraSpacePoint handTipPos)
    {
        float o = handBasePos.Z - handTipPos.Z;
        float a = handBasePos.X - handTipPos.X;
        float angle = Mathf.Rad2Deg * Mathf.Atan2(o, a);

        Quaternion newRotation = Quaternion.AngleAxis(0, Vector3.up);

        if (-35 <= angle && angle < 35)
        {
            //rb.MoveRotation(Quaternion.Euler(0, 0, 0));
            newRotation = Quaternion.AngleAxis(0, Vector3.up);
        }
        else if (angle >= 35 && angle < 90)
        {
            //rb.MoveRotation(Quaternion.Euler(0, 45, 0));
            newRotation = Quaternion.AngleAxis(45, Vector3.up);
        }
        else if (angle >= 90 && angle < 135)
        {
            //rb.MoveRotation(Quaternion.Euler(0, 135, 0));
            newRotation = Quaternion.AngleAxis(135, Vector3.up);
        }
        else if (angle >= 135)
        {
            //rb.MoveRotation(Quaternion.Euler(0, 180, 0));
            newRotation = Quaternion.AngleAxis(180, Vector3.up);
        }
        else if (angle < -35)
        {
            //rb.MoveRotation(Quaternion.Euler(0, -45, 0));
            newRotation = Quaternion.AngleAxis(-45, Vector3.up);
        }
        rb.rotation = Quaternion.Slerp(transform.rotation, newRotation, .05f);

        //No snapping or smoothing
        //rb.MoveRotation(Quaternion.Euler(0, angle, 0));
    }

}
