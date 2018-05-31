﻿using UnityEngine;
using UnityEngine.SceneManagement;

public class JoyconController : MonoBehaviour {
    private static Joycon j;

    public GameObject singlePlayerMenuGO;
    private SinglePManager spScript;
    private bool spInit;

    /// <summary>
    /// ButtonPressed is the bool that sets calibration.
    /// Originally it was when the player presses any button for the first time,
    /// they calibrated the system. This is used in FreePlay, but not in ExpMode
    /// </summary>
    public static bool ButtonPressed;

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
        spInit = false;
    }

    // Update is called once per frame
    void Update()
    {
        //DEBUG Simulate button press on Keyboard
        if (Input.GetKeyUp(KeyCode.Home))
        {
            ButtonPressed = false;
        }
        if (Input.GetKeyDown(KeyCode.Home))
        {
            ButtonPressed = true;
        }
        //END DEBUG

        // make sure the Joycon only gets checked if attached
        if (j != null && j.state > Joycon.state_.ATTACHED && GameUtils.playState != GameUtils.GamePlayState.ExpMode)
        {
            if (GameUtils.playState == GameUtils.GamePlayState.Menu && InitSinglePlayerMenu())
            {
                if (j.GetButtonUp(Joycon.Button.DPAD_UP))
                {
                    spScript.ToggleHighlightedButton(true);
                }
                if (j.GetButtonUp(Joycon.Button.DPAD_DOWN))
                {
                    spScript.ToggleHighlightedButton(false);
                }
                if (j.GetButtonUp(Joycon.Button.SHOULDER_2))
                {
                    spScript.ClickSelectedButton();
                }
            }

            //Back button while playing the game
            if(j.GetButtonUp(Joycon.Button.HOME) || j.GetButtonUp(Joycon.Button.CAPTURE))
            {
                SceneManager.LoadSceneAsync("SinglePlayer", LoadSceneMode.Single);
            }


            // GetButtonDown checks if a button has been released
            if (j.GetButtonUp(Joycon.Button.SHOULDER_2) ||
                j.GetButtonUp(Joycon.Button.DPAD_UP) ||
                j.GetButtonUp(Joycon.Button.DPAD_DOWN) ||
                j.GetButtonUp(Joycon.Button.DPAD_RIGHT) ||
                j.GetButtonUp(Joycon.Button.DPAD_LEFT) ||
                j.GetButtonUp(Joycon.Button.SHOULDER_1) ||
                j.GetButtonUp(Joycon.Button.PLUS) ||
                j.GetButtonUp(Joycon.Button.MINUS))
            {
                ButtonPressed = false;
                //ExperimentLog.Log("Button Released", "Joycon");
            }
            // GetButtonDown checks if a button is currently down (pressed or held)
            if (j.GetButton(Joycon.Button.SHOULDER_2) ||
                j.GetButton(Joycon.Button.DPAD_UP) ||
                j.GetButton(Joycon.Button.DPAD_DOWN) ||
                j.GetButton(Joycon.Button.DPAD_RIGHT) ||
                j.GetButton(Joycon.Button.DPAD_LEFT) ||
                j.GetButton(Joycon.Button.SHOULDER_1) ||
                j.GetButton(Joycon.Button.PLUS) ||
                j.GetButton(Joycon.Button.MINUS))
            {
                ButtonPressed = true;
                //ExperimentLog.Log("Button Pressed", "Joycon");
            }
        }
    }

    private bool InitSinglePlayerMenu()
    {
        if(!spInit)
        {
            spScript = singlePlayerMenuGO.GetComponent<SinglePManager>();
            spInit = true;
            return false;
        }
        return true;
    }

    public static void RumbleJoycon(float lowFreq, float higFreq, float amp, int time = 0)
    {
        if (CheckJoyconAvail() && BodySourceView.BodyFound)
        //if(CheckJoyconAvail())
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
