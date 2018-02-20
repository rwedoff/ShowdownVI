using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;
using UnityEngine.UI;
using Microsoft.Kinect.Face;
using System;
using Windows.Kinect;

public class BodySourceView : MonoBehaviour
{
    public GameObject BodySourceManager;

    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private BodySourceManager _BodyManager;

    private const double FaceRotationIncrementInDegrees = 0.01;

    public static bool leftyMode;
    public Toggle leftyToggle;

    public static CameraSpacePoint handPosition;
    public static CameraSpacePoint wristPosition;
    public static CameraSpacePoint baseKinectPosition;
    public static CameraSpacePoint headPosition;
    public static CameraSpacePoint closestZPosition;
    public static float MaxZDistance;

    public static Quaternion faceRotation;
    public static bool BodyFound;

    public void Start()
    {
        if(PlayerPrefs.GetInt("hand") == 0)
        {
            leftyMode = false;
        }
        else
        {
            leftyMode = true;
        }

        leftyToggle.isOn = leftyMode;
    }


    void Update()
    {
        if (BodySourceManager == null)
        {
            return;
        }

        _BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
        if (_BodyManager == null)
        {
            return;
        }

        Body[] data = _BodyManager.GetData();
        if (data == null)
        {
            BodyFound = false;
            return;
        }

        FaceFrameResult[] faceData = _BodyManager.GetFaceData();

        if (faceData[0] != null)
        {
            faceRotation = new Quaternion(faceData[0].FaceRotationQuaternion.X, faceData[0].FaceRotationQuaternion.Y, faceData[0].FaceRotationQuaternion.Z, faceData[0].FaceRotationQuaternion.W);
        }


        List<ulong> trackedIds = new List<ulong>();
        foreach (var body in data)
        {
            if (body == null)
            {
                continue;
            }

            if (body.IsTracked)
            {
                trackedIds.Add(body.TrackingId);
            }
        }

        List<ulong> knownIds = new List<ulong>(_Bodies.Keys);

        // First delete untracked bodies
        foreach (ulong trackingId in knownIds)
        {
            if (!trackedIds.Contains(trackingId))
            {
                Destroy(_Bodies[trackingId]);
                _Bodies.Remove(trackingId);
            }
        }

        foreach (var body in data)
        {
            if (body == null)
            {
                continue;
            }

            if (body.IsTracked)
            {
                BodyFound = true;
                if (!_Bodies.ContainsKey(body.TrackingId))
                {
                    _Bodies[body.TrackingId] = CreateBodyObject(body.TrackingId);
                }

                RefreshBodyObject(body, _Bodies[body.TrackingId]);
            }
        }
    }

    private GameObject CreateBodyObject(ulong id)
    {
        GameObject body = new GameObject("Body:" + id);
        return body;
    }

    private void RefreshBodyObject(Body body, GameObject bodyObject)
    {
        if (leftyToggle.isOn) //Left handed
        {
            handPosition = body.Joints[JointType.HandTipLeft].Position;
            wristPosition = body.Joints[JointType.HandLeft].Position;
            leftyMode = true;
        }
        else
        {
            handPosition = body.Joints[JointType.HandTipRight].Position;
            wristPosition = body.Joints[JointType.HandRight].Position;
            leftyMode = false;
        }

        headPosition = body.Joints[JointType.Head].Position;

        MaxZDistance = 
            Math.Max(body.Joints[JointType.Head].Position.Z, 
            Math.Max(body.Joints[JointType.Head].Position.Z, 
            Math.Max(body.Joints[JointType.Neck].Position.Z, 
            Math.Max(body.Joints[JointType.SpineMid].Position.Z, 
            Math.Max(body.Joints[JointType.SpineShoulder].Position.Z, 
            Math.Max(body.Joints[JointType.HipLeft].Position.Z,
                body.Joints[JointType.HipRight].Position.Z))))));

        float minZBodyDist =
           Math.Min(body.Joints[JointType.Head].Position.Z,
           Math.Min(body.Joints[JointType.Head].Position.Z,
           Math.Min(body.Joints[JointType.Neck].Position.Z,
           Math.Min(body.Joints[JointType.SpineMid].Position.Z,
           Math.Min(body.Joints[JointType.SpineShoulder].Position.Z,
           Math.Min(body.Joints[JointType.HipLeft].Position.Z,
               body.Joints[JointType.HipRight].Position.Z))))));

        float minZDistance =
            Math.Min(body.Joints[JointType.Head].Position.Z,
            Math.Min(body.Joints[JointType.Head].Position.Z,
            Math.Min(body.Joints[JointType.Neck].Position.Z,
                body.Joints[JointType.SpineShoulder].Position.Z)));

        baseKinectPosition = new CameraSpacePoint()
        {
            X = body.Joints[JointType.SpineShoulder].Position.X,
            Y = body.Joints[JointType.Head].Position.Y,
            Z = minZBodyDist
        };

        closestZPosition = new CameraSpacePoint()
        {
            X = body.Joints[JointType.SpineMid].Position.X,
            Y = body.Joints[JointType.SpineMid].Position.Y,
            Z = minZDistance
        };
    }
}
