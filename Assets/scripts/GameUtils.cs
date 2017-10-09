using System;
using System.Collections.Generic;
using UnityEngine;

public class GameUtils
{
    private const double FaceRotationIncrementInDegrees = 1.0;
    private static int playerSide = 0;
    public static bool playerServe = true;

    public static float Scale(float OldMin, float OldMax, float NewMin, float NewMax, float OldValue)
    {
        float OldRange = (OldMax - OldMin);
        float NewRange = (NewMax - NewMin);
        return (((OldValue - OldMin) * NewRange) / OldRange) + NewMin;
    }

    public static void ResetBall(GameObject ball)
    {
        playerSide++;
        Rigidbody rb = ball.GetComponent<Rigidbody>();
        rb.velocity = new Vector3(0, 0, 0);
        if (playerSide % 4 < 2)
        {
            playerServe = true;
            rb.MovePosition(new Vector3(0, 3, -120f));
        }
        else
        {
            playerServe = false;
            rb.MovePosition(new Vector3(0, 3, 120f));
        }
    }

    ///NOT CURRENTLY USED, COULD COME IN HANDY

    /// <summary>
    /// Converts rotation quaternion to Euler angles 
    /// And then maps them to a specified range of values to control the refresh rate
    /// </summary>
    /// <param name="rotQuaternion">face rotation quaternion</param>
    /// <param name="pitch">rotation about the X-axis</param>
    /// <param name="yaw">rotation about the Y-axis</param>
    /// <param name="roll">rotation about the Z-axis</param>
    private static void ExtractFaceRotationInDegrees(Windows.Kinect.Vector4 rotQuaternion, out int pitch, out int yaw, out int roll)
    {
        double x = rotQuaternion.X;
        double y = rotQuaternion.Y;
        double z = rotQuaternion.Z;
        double w = rotQuaternion.W;

        // convert face rotation quaternion to Euler angles in degrees
        double yawD, pitchD, rollD;
        pitchD = Math.Atan2(2 * ((y * z) + (w * x)), (w * w) - (x * x) - (y * y) + (z * z)) / Math.PI * 180.0;
        yawD = Math.Asin(2 * ((w * y) - (x * z))) / Math.PI * 180.0;
        rollD = Math.Atan2(2 * ((x * y) + (w * z)), (w * w) + (x * x) - (y * y) - (z * z)) / Math.PI * 180.0;

        // clamp the values to a multiple of the specified increment to control the refresh rate
        double increment = FaceRotationIncrementInDegrees;
        pitch = (int)(Math.Floor((pitchD + ((increment / 2.0) * (pitchD > 0 ? 1.0 : -1.0))) / increment) * increment);
        yaw = (int)(Math.Floor((yawD + ((increment / 2.0) * (yawD > 0 ? 1.0 : -1.0))) / increment) * increment);
        roll = (int)(Math.Floor((rollD + ((increment / 2.0) * (rollD > 0 ? 1.0 : -1.0))) / increment) * increment);
    }


    private Quaternion SmoothFilter(Queue<Quaternion> quaternions, Quaternion lastMedian)
    {
        Quaternion median = new Quaternion(0, 0, 0, 0);

        foreach (Quaternion quaternion in quaternions)
        {
            float weight = 1 - (Quaternion.Dot(lastMedian, quaternion) / (Mathf.PI / 2)); // 0 degrees of difference => weight 1. 180 degrees of difference => weight 0.
            Quaternion weightedQuaternion = Quaternion.Lerp(lastMedian, quaternion, weight);

            median.x += weightedQuaternion.x;
            median.y += weightedQuaternion.y;
            median.z += weightedQuaternion.z;
            median.w += weightedQuaternion.w;
        }

        median.x /= quaternions.Count;
        median.y /= quaternions.Count;
        median.z /= quaternions.Count;
        median.w /= quaternions.Count;

        return NormalizeQuaternion(median);
    }

    public Quaternion NormalizeQuaternion(Quaternion quaternion)
    {
        float x = quaternion.x, y = quaternion.y, z = quaternion.z, w = quaternion.w;
        float length = 1.0f / (w * w + x * x + y * y + z * z);
        return new Quaternion(x * length, y * length, z * length, w * length);
    }
}
