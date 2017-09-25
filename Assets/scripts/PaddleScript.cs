using UnityEngine;
using Windows.Kinect;

public class PaddleScript : MonoBehaviour
{
    private Rigidbody rb;

    public float smooth = 0.5F;
    private float yVelocity = 0.0F;
    private bool batOutofBounds;

    private UnityEngine.AudioSource wallCollideAudio;


    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        wallCollideAudio = GetComponent<UnityEngine.AudioSource>();
        batOutofBounds = true;
        wallCollideAudio.loop = true;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        //MoveBat(BodySourceView.spineMidPosition, BodySourceView.handPosition);
        CameraSpacePoint midSpinePosition = BodySourceView.spineMidPosition;
        CameraSpacePoint handPosition = BodySourceView.handPosition;
        //Calculate the position of the paddle based on the distance from the mid spine join
        float xPos = (midSpinePosition.X - handPosition.X) * 100,
              zPos = (midSpinePosition.Z - handPosition.Z) * 100;

        //Smooth and set the position of the paddle
        //Vector3 direction = (new Vector3(-xPos, 0, (zPos - 188.5f)) - transform.position).normalized;
        //rb.MovePosition(transform.position + (direction * 75 * Time.deltaTime));

        rb.MovePosition(new Vector3(-xPos, 4.5f, (zPos - 188.5f)));

        RotateBat(BodySourceView.wristPosition, BodySourceView.handPosition);

        CheckBatInGame();
    }



    private void CheckBatInGame()
    {
        batOutofBounds = false;
        if ((transform.position.x > 60 || transform.position.x < -50 || transform.position.z < -175))
        {
            batOutofBounds = true;
        }

        if (!batOutofBounds && wallCollideAudio.isPlaying)
        {
            wallCollideAudio.Pause();
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
        float h = handBasePos.Z - handTipPos.Z;
        float a = handBasePos.X - handTipPos.X;
        float angle = Mathf.Rad2Deg * Mathf.Atan2(h, a);

        float smoothAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, angle, ref yVelocity, smooth);

        rb.MoveRotation(Quaternion.Euler(0, smoothAngle, 0));
    }

}
