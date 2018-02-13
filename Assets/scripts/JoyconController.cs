using UnityEngine;

public class JoyconController : MonoBehaviour {
    private static Joycon j;
    public static bool Shoulder2Pressed;

    // Values made available via Unity
    public float[] stick;
    public Vector3 gyro;
    public Vector3 accel;
    public Quaternion orientation;

    void Start()
    {
        gyro = new Vector3(0, 0, 0);
        accel = new Vector3(0, 0, 0);
        // get the public Joycon object attached to the JoyconManager in scene
        j = JoyconManager.Instance.j;
    }

    // Update is called once per frame
    void Update()
    {
        // make sure the Joycon only gets checked if attached
        if (j != null && j.state > Joycon.state_.ATTACHED)
        {
            // GetButtonDown checks if a button has been released
            if (j.GetButtonUp(Joycon.Button.SHOULDER_2))
            {
                Shoulder2Pressed = false;
            }
            // GetButtonDown checks if a button is currently down (pressed or held)
            if (j.GetButton(Joycon.Button.SHOULDER_2))
            {
                Shoulder2Pressed = true;
            }
        }
    }

    public static void RumbleJoycon(float lowFreq, float higFreq, float amp, int time = 0)
    {
        if (CheckJoyconAvail())
        {
            j.SetRumble(lowFreq, higFreq, amp, time);
        }
    }

    private static bool CheckJoyconAvail()
    {
        // make sure the Joycon only gets checked if attached
        if (j != null && j.state > Joycon.state_.ATTACHED)
        {
            return true;
        }
        return false;
    }
}
