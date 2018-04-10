using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class JoyconManager: MonoBehaviour
{

    // Settings accessible via Unity
    public bool EnableIMU = true;
    public bool EnableLocalize = true;
    public byte LEDs = 0xff;

    public Joycon j;
    static JoyconManager instance;

    public static JoyconManager Instance
    {
        get { return instance; }
    }

    void Awake()
    {
        if (instance != null) Destroy(gameObject);
        instance = this;
        j = new Joycon();
        DontDestroyOnLoad(instance);
        SceneManager.LoadSceneAsync("Master", LoadSceneMode.Single);
    }

    void Start()
    {
        j.Attach(leds_: LEDs, imu: EnableIMU, localize: EnableLocalize);
		j.Begin ();
    }

    void Update()
    {
        j.Update();
    }

    void OnApplicationQuit()
    {
        j.Detach();
        Destroy(instance);
        Destroy(this);
    }
}
