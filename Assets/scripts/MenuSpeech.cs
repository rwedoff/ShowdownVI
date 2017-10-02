﻿using UnityEngine.Windows.Speech;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuSpeech : MonoBehaviour
{
    public Text menuText;

    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, Action> keywords = new Dictionary<string, Action>();
    private AudioSource[] audioSources;
    private AudioSource currentAudioSource;

    private float oldTime;
    private bool timerStarted;


    private string mainTextString = "Welcome to Virtual Showdown! \n Say \"Help\", \"Single Player\", or \"Settings\"";

    public enum MenuState
    {
        Main, Settings, Hand, Difficulty, InPlay
    }
    public MenuState menuState;

    private void Start()
    {
        if (!PlayerPrefs.HasKey("hand"))
        {
            PlayerPrefs.SetInt("hand", 0);
        }
        if (!PlayerPrefs.HasKey("diff"))
        {
            PlayerPrefs.SetInt("diff", 0);
        }

        audioSources = GetComponentsInChildren<AudioSource>();

        oldTime = 0;
        timerStarted = true;

        FillKeyWords();
        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        keywordRecognizer.Start();

        currentAudioSource = audioSources[7];
        audioSources[7].Play();
    }

    private void Update()
    {
        //if (timerStarted)
        //{
            //If ball is still in zone after 20 seconds then hit
            if (Time.time > oldTime + 20)
            {
                Debug.Log("Repeat");
                currentAudioSource.Play();
            }
        //}
    }

    private void ChangeDifficultyMenu()
    {
        menuState = MenuState.Difficulty;
        currentAudioSource = audioSources[8];
        audioSources[8].Play();
        menuText.text = "Change Difficulty: \n Say, \"Easy\", \"Medium\" or \"Hard\"";
    }

    private void ChangeHand()
    {
        menuState = MenuState.Hand;
        audioSources[1].Play();
        currentAudioSource = audioSources[1];
        menuText.text = "Change hand: \n Say, \"Right\" or \"Left\"";
    }

    private void StartSettings()
    {
        menuState = MenuState.Settings;
        menuText.text = "Settings: \n Say, \"Change Hand\" or \"Change Difficulty\"";
        audioSources[0].Play();
        currentAudioSource = audioSources[0];
    }

    private void StartSinglePlayer()
    {
        SceneManager.LoadSceneAsync("SinglePlayer");
    }

    private void StartTutorial()
    {
        SceneManager.LoadSceneAsync("tutorial");
    }

    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        Action keywordAction;
        // if the keyword recognized is in our dictionary, call that Action.
        if (keywords.TryGetValue(args.text, out keywordAction))
        {
            keywordAction.Invoke();
        }
        oldTime = Time.time;
    }

    private void FillKeyWords()
    {
        //Create keywords for keyword recognizer
        keywords.Add("Help", () =>
        {
            if (menuState == MenuState.Main)
            {
                StartTutorial();
            }
        });
        keywords.Add("Single Player", () =>
        {
            if (menuState == MenuState.Main)
            {
                StartSinglePlayer();
            }
        });
        keywords.Add("Settings", () => {
            if (menuState == MenuState.Main)
            {
                StartSettings();
            }
        });
        keywords.Add("Change Hand", () => {
            if (menuState == MenuState.Settings)
            {
                ChangeHand();
            }
        });
        keywords.Add("Change Difficulty", () =>
        {
            if (menuState == MenuState.Settings)
            {
                ChangeDifficultyMenu();
            }
        });
        keywords.Add("Left", () =>
        {
            if(menuState == MenuState.Hand)
                ChangeHandToLeft();
        });
        keywords.Add("Right", () =>
        {
            if (menuState == MenuState.Hand)
                ChangeHandToRight();
        });
        keywords.Add("Easy", () =>
        {
            if (menuState == MenuState.Difficulty)
                ChangeDifficulty(0);
        });
        keywords.Add("Medium", () =>
        {
            if (menuState == MenuState.Difficulty)
                ChangeDifficulty(1);
        });
        keywords.Add("Hard", () =>
        {
            if (menuState == MenuState.Difficulty)
                ChangeDifficulty(2);
        });
        keywords.Add("Main Menu", () =>
        {
            ReturnToMain();
        });
        keywords.Add("Play Game", () =>
        {
            if(menuState == MenuState.InPlay)
            {
                Time.timeScale = 1;
                AudioListener.pause = false;
            }
        });
        keywords.Add("Pause Game", () =>
        {
            if(menuState == MenuState.InPlay)
            {
                AudioListener.pause = true;
                Time.timeScale = 0;
            }
        });
    }

    private void ChangeDifficulty(int diff)
    {
        if (diff == 0) //Easy
        {
            PlayerPrefs.SetInt("diff", 0);
            menuText.text = "Difficulty is set to Easy. \n Say \"Main Menu\" to go back.";
            currentAudioSource = audioSources[4];
            audioSources[4].Play();
        }
        else if(diff == 1) //Medium
        {
            PlayerPrefs.SetInt("diff", 1);
            menuText.text = "Difficulty is set to Medium. \n Say \"Main Menu\" to go back.";
            audioSources[5].Play();
            currentAudioSource = audioSources[5];
        }
        else if(diff == 2) //Hard
        {
            PlayerPrefs.SetInt("diff", 2);
            menuText.text = "Difficulty is set to Hard. \n Say \"Main Menu\" to go back.";
            audioSources[6].Play();
            currentAudioSource = audioSources[6];
        }
    }

    private void ReturnToMain()
    {
        menuText.text = mainTextString;
        audioSources[7].Play();
        menuState = MenuState.Main;
        currentAudioSource = audioSources[7];
    }

    private void ChangeHandToRight()
    {
        audioSources[2].Play();
        PlayerPrefs.SetInt("hand", 0);
        currentAudioSource = audioSources[2];
        menuText.text = "Your hand is set to Right. \n Say \"Main Menu\" to go back.";
    }

    private void ChangeHandToLeft()
    {
        audioSources[3].Play();
        PlayerPrefs.SetInt("hand", 1);
        currentAudioSource = audioSources[3];
        menuText.text = "Your hand is set to Left. \n Say \"Main Menu\" to go back.";
    }
}
