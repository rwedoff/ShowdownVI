using System;
using UnityEngine;
using Windows.Kinect;

public class PaddleScript : MonoBehaviour
{
    public static bool ScreenPressDown { get; internal set; }

    private Rigidbody rb;
    private UnityEngine.AudioSource wallCollideAudio;
    private UnityEngine.AudioSource batUpAudio;
    private UnityEngine.AudioSource batDownAudio;
    private bool batUpOnce;
    private bool batDownOnce;
    private float oldTime;
    private float halfBatLen;
    private float halfBatThick;
    private const float estAvgError = 7f; //Prev 26
    private const float unityTableEdge = 130f;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        UnityEngine.AudioSource [] auds = GetComponents<UnityEngine.AudioSource>();
        wallCollideAudio = auds[0];
        batUpAudio = auds[1];
        batDownAudio = auds[2];
        wallCollideAudio.loop = true;
        halfBatLen = transform.localScale.x / 2;
        halfBatThick = transform.localScale.z / 2;
        batUpOnce = false;
        batDownOnce = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        CameraSpacePoint midSpinePosition = BodySourceView.baseKinectPosition;
        CameraSpacePoint handPosition = BodySourceView.handPosition;
        float centerXPoint, maxZPoint;

        if (GameUtils.playState ==  GameUtils.GamePlayState.ExpMode)
        {
            centerXPoint = ExpManager.CenterX != 0 ? ExpManager.CenterX : midSpinePosition.X;
            maxZPoint = ExpManager.TableEdge != 0 ? ExpManager.TableEdge : midSpinePosition.Z;
        }
        else
        {
            centerXPoint = SinglePManager.CenterX != 0 ? SinglePManager.CenterX : midSpinePosition.X;
            maxZPoint = SinglePManager.TableEdge != 0 ? SinglePManager.TableEdge : midSpinePosition.Z;
        }

        //Add buffer to be able to reach the opposite side easier.
        //Not sure why leftyMode needs to be more of a buffer.
        if (BodySourceView.leftyMode)
        {
            centerXPoint -= 0.15f;
        }
        else
        {
            centerXPoint += 0.06f;
        }

        //Calculate the position of the paddle based on the distance from the mid spine join
        float xPos = (centerXPoint - handPosition.X) * 100,
              zPos = (maxZPoint - handPosition.Z) * 100,
              yPos = transform.position.y;

        //If screen press, lift bat
        if (JoyconController.ButtonPressed || ScreenPressDown)
        {
            yPos = 20;
            batDownOnce = true;
            if (batUpOnce)
            {
                PlayBatUpAudio();
                batUpOnce = false;
            }
        }
        else
        {
            yPos = 5f;
            batUpOnce = true;
            if (batDownOnce)
            {
                PlayBatDownAudio();
                batDownOnce = false;
            }
        }

        //Smoothing applied to slow down bat so it doesn't phase through ball
        Vector3 newPosition = new Vector3(-xPos, yPos, (zPos - unityTableEdge - estAvgError));
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

    private void PlayBatUpAudio()
    {
        batUpAudio.Play();
        batUpOnce = false;
    }

    private void PlayBatDownAudio()
    {
        if (batDownOnce)
        {
            batDownAudio.Play();
        }
    }

    private void CheckBatInGame()
    {
        float outOfBoundsBy = 0;
        float xSide = 50 - halfBatLen;
        float zSide = 130 - halfBatThick;
        if ((transform.position.x > xSide || transform.position.x < -xSide || transform.position.z < -zSide))
        {
            
            if(Math.Abs(transform.position.x) > xSide)
            {
                outOfBoundsBy = Math.Abs(transform.position.x) - xSide;
            }
            else if(transform.position.z < -zSide)
            {
                outOfBoundsBy = Math.Abs(transform.position.z) -zSide;
            }
        }

        if (outOfBoundsBy == 0 && wallCollideAudio.isPlaying)
        {
            wallCollideAudio.Pause();
            JoyconController.RumbleJoycon(0, 0, 0);
        }
        else if (!wallCollideAudio.isPlaying && outOfBoundsBy != 0)
        {
            wallCollideAudio.Play();
        }
        if(outOfBoundsBy != 0)
        {
            float rumbleAmp = 0;
            if (outOfBoundsBy < 10)
            {
                rumbleAmp = 0.4f;
            }
            else if (outOfBoundsBy < 30)
            {
                rumbleAmp = 0.7f;
            }
            else
            {
                rumbleAmp = 0.9f;
            }
            JoyconController.RumbleJoycon(90, 270, rumbleAmp);
            wallCollideAudio.volume = GameUtils.Scale(0, 45, 0.1f, 1, outOfBoundsBy);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.tag == "GoalTrigger")
        {
            JoyconController.RumbleJoycon(100, 400, 0.2f);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        JoyconController.RumbleJoycon(0, 0, 0);
    }

    /// <summary>
    /// Calculates the rotation of the bat in the virtual world
    /// </summary>
    /// <param name="handBasePos">Distance of the base of the hand from the Kinect</param>
    /// <param name="handTipPos">Distance of the tip of the hand from the Kinect</param>
    private void RotateBat(CameraSpacePoint handBasePos, CameraSpacePoint handTipPos)
    {
        float o = handBasePos.Z - handTipPos.Z,
              a = handBasePos.X - handTipPos.X,
              angle = Mathf.Rad2Deg * Mathf.Atan2(o, a);

        Quaternion newRotation = Quaternion.AngleAxis(0, Vector3.up);

        if (-35 <= angle && angle < 35)
        {
            newRotation = Quaternion.AngleAxis(0, Vector3.up);
        }
        else if (angle >= 35 && angle < 90)
        {
            newRotation = Quaternion.AngleAxis(45, Vector3.up);
        }
        else if (angle >= 90 && angle < 135)
        {
            newRotation = Quaternion.AngleAxis(135, Vector3.up);
        }
        else if (angle >= 135)
        {
            newRotation = Quaternion.AngleAxis(180, Vector3.up);
        }
        else if (angle < -35)
        {
            newRotation = Quaternion.AngleAxis(-45, Vector3.up);
        }
        rb.rotation = Quaternion.Slerp(transform.rotation, newRotation, .05f);

        //No snapping or smoothing
        //rb.MoveRotation(Quaternion.Euler(0, angle, 0));
    }

}
