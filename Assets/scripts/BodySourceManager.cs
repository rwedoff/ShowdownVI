using UnityEngine;
using Windows.Kinect;
using Microsoft.Kinect.Face;
using System.Collections.Generic;

public class BodySourceManager : MonoBehaviour
{
    private KinectSensor _Sensor;
    private BodyFrameReader _Reader;
    private Body[] _Data = null;
    private FaceFrameResult[] _FaceData;
    private FaceFrameSource[] faceFrameSources = null;
    private FaceFrameReader[] faceFrameReaders = null;

    public Body[] GetData()
    {
        return _Data;
    }

    public FaceFrameResult[] GetFaceData()
    {
        return _FaceData;
    }

    void Start()
    {
        _Sensor = KinectSensor.GetDefault();

        if (_Sensor != null)
        {
            _Reader = _Sensor.BodyFrameSource.OpenReader();

            if (!_Sensor.IsOpen)
            {
                _Sensor.Open();
            }
        }

        this.faceFrameSources = new FaceFrameSource[_Sensor.BodyFrameSource.BodyCount];
        this.faceFrameReaders = new FaceFrameReader[_Sensor.BodyFrameSource.BodyCount];
        // specify the required face frame results
        FaceFrameFeatures faceFrameFeatures =
                FaceFrameFeatures.RotationOrientation
                | FaceFrameFeatures.FaceEngagement
                | FaceFrameFeatures.LookingAway;

        for (int i = 0; i < _Sensor.BodyFrameSource.BodyCount; i++)
        {
            // create the face frame source with the required face frame features and an initial tracking Id of 0
            faceFrameSources[i] = FaceFrameSource.Create(_Sensor, 0, faceFrameFeatures);

            // open the corresponding reader
            faceFrameReaders[i] = faceFrameSources[i].OpenReader();
        }
    }

    void Update()
    {
        if (_Reader != null)
        {
            var frame = _Reader.AcquireLatestFrame();
            if (frame != null)
            {
                if (_Data == null)
                {
                    _Data = new Body[_Sensor.BodyFrameSource.BodyCount];
                    _FaceData = new FaceFrameResult[_Sensor.BodyFrameSource.BodyCount];
                }

                frame.GetAndRefreshBodyData(_Data);
                //_FaceData = processFaceData(_Sensor.BodyFrameSource.BodyCount);
                List<FaceFrameResult> res = new List<FaceFrameResult>();
                // iterate through each body and update face source
                for (int i = 0; i < _Sensor.BodyFrameSource.BodyCount; i++)
                {
                    // check if a valid face is tracked in this face source				
                    if (faceFrameSources[i].IsTrackingIdValid)
                    {
                        using (FaceFrame faceFrame = faceFrameReaders[i].AcquireLatestFrame())
                        {
                            if (faceFrame != null)
                            {
                                if (faceFrame.TrackingId == 0)
                                {
                                    continue;
                                }

                                // do something with result
                                var result = faceFrame.FaceFrameResult;
                                res.Add(result);
                            }
                        }
                    }
                    else
                    {
                        // check if the corresponding body is tracked 
                        if (_Data[i].IsTracked)
                        {
                            // update the face frame source to track this body
                            faceFrameSources[i].TrackingId = _Data[i].TrackingId;
                        }
                    }
                }
                if (res.Count > 0)
                {
                    _FaceData = res.ToArray();
                }

                frame.Dispose();
                frame = null;
            }
        }















    }

    private FaceFrameResult[] processFaceData(int bodyCount)
    {
        FaceFrameResult[] results = new FaceFrameResult[bodyCount];
        // create a face frame source + reader to track each face in the FOV
        for (int i = 0; i < bodyCount; i++)
        {
            if (faceFrameSources[i].IsTrackingIdValid)
            {
                using (FaceFrame frame = faceFrameReaders[i].AcquireLatestFrame())
                {
                    if (frame != null)
                    {
                        if (frame.TrackingId == 0)
                        {
                            continue;
                        }
                        results[i] = frame.FaceFrameResult;
                    }
                }
            }
        }
        return results;
    }

    void OnApplicationQuit()
    {
        if (_Reader != null)
        {
            _Reader.Dispose();
            _Reader = null;
        }

        if (_Sensor != null)
        {
            if (_Sensor.IsOpen)
            {
                _Sensor.Close();
            }

            _Sensor = null;
        }
    }
}
