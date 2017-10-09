using UnityEngine;
using Windows.Kinect;

public class CameraController : MonoBehaviour
{
    public float smooth = 0.01F;
    private float yVelocity = 0.0F;
    private float xVelocity = 0.0F;
    private float zVelocity = 0.0F;
    private float startingZPosition;

    private void Start()
    {
        startingZPosition = transform.position.z;
    }

    private void LateUpdate()
    {
        CameraSpacePoint closestZPosition = BodySourceView.closestZPosition;
        CameraSpacePoint furtherestZPosition = BodySourceView.baseKinectPosition;

        float deltaZPosition = (furtherestZPosition.Z - closestZPosition.Z) * 100;

        transform.position = 
            new Vector3(0, 
                (closestZPosition.Y * 100) + 30.16f, 
                (startingZPosition + deltaZPosition));

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