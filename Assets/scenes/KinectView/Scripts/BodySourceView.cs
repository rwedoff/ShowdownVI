using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;
using UnityEngine.UI;
using Microsoft.Kinect.Face;
using System;

public class BodySourceView : MonoBehaviour
{
    public Material BoneMaterial;
    public GameObject BodySourceManager;

    private Dictionary<ulong, GameObject> _Bodies = new Dictionary<ulong, GameObject>();
    private BodySourceManager _BodyManager;
    public Text yawText;
    public Text rollText;
    public Text pitchText;
    private const double FaceRotationIncrementInDegrees = 0.01;

    private bool leftyMode = false;

    public static float pitch = 0;
    public static float yaw = 0;
    public static float roll = 0;

    public static Kinect.CameraSpacePoint handPosition;
    public static Kinect.CameraSpacePoint neckPosition;
    public static Kinect.CameraSpacePoint wristPosition;
    public static Kinect.CameraSpacePoint spineMidPosition;
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

    public Toggle leftyToggle;

    private void Start()
    {
        
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

        Kinect.Body[] data = _BodyManager.GetData();
        if (data == null)
        {
            return;
        }

        FaceFrameResult[] faceData = _BodyManager.GetFaceData();


        //TODO what happens if there is more than one face?
        //TODO Static vars?

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

        //for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        //{
        //    GameObject jointObj = GameObject.CreatePrimitive(PrimitiveType.Cube);

        //    LineRenderer lr = jointObj.AddComponent<LineRenderer>();
        //    lr.SetVertexCount(2);
        //    lr.material = BoneMaterial;
        //    lr.SetWidth(0.05f, 0.05f);

        //    jointObj.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        //    jointObj.name = jt.ToString();
        //    jointObj.transform.parent = body.transform;
        //}

        return body;
    }

    private void RefreshBodyObject(Kinect.Body body, GameObject bodyObject)
    {
        if (leftyToggle.isOn)
        {
            handPosition = body.Joints[Kinect.JointType.HandLeft].Position;
            wristPosition = body.Joints[Kinect.JointType.HandLeft].Position;
        }
        else
        {
            handPosition = body.Joints[Kinect.JointType.HandTipRight].Position;
            wristPosition = body.Joints[Kinect.JointType.HandRight].Position;
        }
        neckPosition = body.Joints[Kinect.JointType.Neck].Position;
        spineMidPosition = body.Joints[Kinect.JointType.SpineMid].Position;

        //float h = wristPosition.Z - handPosition.Z;
        //float a = wristPosition.X - handPosition.X;
        //float angle = Mathf.Rad2Deg * Mathf.Atan2(h, a);

        //yawText.text = "HAND : " + handPosition.Z;
        //pitchText.text = "WRIST : " + wristPosition.Z;
        //rollText.text = "Angle : " + angle;


        //for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
        //{
        //    Kinect.Joint sourceJoint = body.Joints[jt];
        //    Kinect.Joint? targetJoint = null;

        //    if (_BoneMap.ContainsKey(jt))
        //    {
        //        targetJoint = body.Joints[_BoneMap[jt]];
        //    }

        //    Transform jointObj = bodyObject.transform.Find(jt.ToString());
        //    jointObj.localPosition = GetVector3FromJoint(sourceJoint);

        //    LineRenderer lr = jointObj.GetComponent<LineRenderer>();
        //    if (targetJoint.HasValue)
        //    {
        //        lr.SetPosition(0, jointObj.localPosition);
        //        lr.SetPosition(1, GetVector3FromJoint(targetJoint.Value));
        //        lr.SetColors(GetColorForState(sourceJoint.TrackingState), GetColorForState(targetJoint.Value.TrackingState));
        //    }
        //    else
        //    {
        //        lr.enabled = false;
        //    }
        //}
    }

    private static UnityEngine.Color GetColorForState(Kinect.TrackingState state)
    {
        switch (state)
        {
            case Kinect.TrackingState.Tracked:
                return UnityEngine.Color.green;

            case Kinect.TrackingState.Inferred:
                return UnityEngine.Color.red;

            default:
                return UnityEngine.Color.black;
        }
    }

    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }
}
