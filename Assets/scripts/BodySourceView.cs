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

    private bool leftyMode;
    public Toggle leftyToggle;

    public static CameraSpacePoint handPosition;
    public static CameraSpacePoint wristPosition;
    public static CameraSpacePoint baseKinectPosition;
    public static CameraSpacePoint spineMidPosition;
    public static Quaternion handRotation;

    public static Quaternion faceRotation;


    private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
    {
        { Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },
        { Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },
        { Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
        { Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },

        { Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },
        { Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },
        { Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
        { Kinect.JointType.HipRight, Kinect.JointType.SpineBase },

        { Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
        { Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
        { Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
        { Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },

        { Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
        { Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },
        { Kinect.JointType.HandRight, Kinect.JointType.WristRight },
        { Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
        { Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
        { Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },

        { Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
        { Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
        { Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
        { Kinect.JointType.Neck, Kinect.JointType.Head },
    };

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

        leftyToggle.isOn = leftyToggle;
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
            return;
        }

        FaceFrameResult[] faceData = _BodyManager.GetFaceData();


        //TODO what happens if there is more than one face?

        if (faceData[0] != null)
        {
            if (faceData[0].FaceRotationQuaternion != null)
            {
                faceRotation = new Quaternion(faceData[0].FaceRotationQuaternion.X, faceData[0].FaceRotationQuaternion.Y, faceData[0].FaceRotationQuaternion.Z, faceData[0].FaceRotationQuaternion.W);
            }
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

    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
    {
        if (leftyToggle.isOn)
        {
            handPosition = body.Joints[Kinect.JointType.HandTipLeft].Position;
            wristPosition = body.Joints[Kinect.JointType.HandLeft].Position;
        }
        else
        {
            handPosition = body.Joints[Kinect.JointType.HandTipRight].Position;
            wristPosition = body.Joints[Kinect.JointType.HandRight].Position;
        }

        float maxZDistance = 
            Math.Max(body.Joints[JointType.Head].Position.Z, 
            Math.Max(body.Joints[JointType.Head].Position.Z, 
            Math.Max(body.Joints[JointType.Neck].Position.Z, 
            Math.Max(body.Joints[JointType.SpineMid].Position.Z, 
            Math.Max(body.Joints[JointType.SpineShoulder].Position.Z, 
            Math.Max(body.Joints[JointType.HipLeft].Position.Z,
                body.Joints[JointType.HipRight].Position.Z))))));

        baseKinectPosition = new CameraSpacePoint()
        {
            X = body.Joints[JointType.Head].Position.X,
            Y = body.Joints[JointType.Head].Position.Y,
            Z = maxZDistance
        };

        spineMidPosition = body.Joints[JointType.SpineMid].Position;

    }
}
