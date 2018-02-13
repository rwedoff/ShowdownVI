﻿using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BackButton : MonoBehaviour {
    public Button backButton;
	// Use this for initialization
	void Start () {
        //Stop rumble just in case.
        JoyconController.RumbleJoycon(0, 0, 0);
        backButton.onClick.AddListener(() => { SceneManager.LoadSceneAsync("Menu", LoadSceneMode.Single); });
    }
}
