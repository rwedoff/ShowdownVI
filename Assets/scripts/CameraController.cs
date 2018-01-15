using UnityEngine;
using Windows.Kinect;

public class CameraController : MonoBehaviour
{
    public float smooth = 0.01F;
    public static float CameraDeltaZ;
    private float yVelocity = 0.0F;
    private float xVelocity = 0.0F;
    private float zVelocity = 0.0F;
    private float startingZPosition;

    private void Start()
    {
        startingZPosition = transform.position.z;
        CameraDeltaZ = 0;
    }

    private void FixedUpdate()
    {
        CameraSpacePoint closestZPosition = BodySourceView.closestZPosition;
        CameraSpacePoint furtherestZPosition = BodySourceView.baseKinectPosition;

        CameraDeltaZ = (furtherestZPosition.Z - closestZPosition.Z) * 100;
        //transform.position =
        //    new Vector3(0,
        //        (closestZPosition.Y * 100) + 30.16f,
        //        (startingZPosition + CameraDeltaZ) + 20f);

        //No Y movement
        transform.position =
            new Vector3(0, transform.position.y, startingZPosition + CameraDeltaZ);
        

        Quaternion fr = BodySourceView.faceRotation;
        if (fr != null)
        {

            float yAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, -fr.eulerAngles.y, ref yVelocity, smooth);
            float xAngle = Mathf.SmoothDampAngle(transform.eulerAngles.x, -fr.eulerAngles.x, ref xVelocity, smooth);
            float zAngle = Mathf.SmoothDampAngle(transform.eulerAngles.z, fr.eulerAngles.z, ref zVelocity, smooth);

            transform.localRotation = Quaternion.Euler(xAngle, yAngle, zAngle);
        }

    }

}