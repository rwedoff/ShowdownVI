using UnityEngine;
using Windows.Kinect;

public class CameraController : MonoBehaviour
{
    public float smooth = 0.01F;
    private float yVelocity = 0.0F;
    private float xVelocity = 0.0F;
    private float zVelocity = 0.0F;

    private void LateUpdate()
    {
        CameraSpacePoint spine = BodySourceView.spineMidPosition;

        transform.position = new Vector3(0, (spine.Y * 100) + 30.16f, transform.position.z);

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