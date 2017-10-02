//using UnityEngine.Windows.Speech;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using System;
//using UnityEngine.SceneManagement;

//public class SpeechScript : MonoBehaviour {

//    KeywordRecognizer keywordRecognizer;
//    Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();

//    private void Start()
//    {
//        //Create keywords for keyword recognizer
//        keywords.Add("game pause", () =>
//        {
//            GoToAction();
//        });
//        keywords.Add("game play", () =>
//        {
//            PlayGame();
//        });
//        keywords.Add("Start Tutorial", () =>
//        {
//            StartTutorial();
//        });
//        keywords.Add("Start Single Player", () =>
//        {
//            StartSinglePlayer();
//        });
//        keywords.Add("Change difficulty", () => {

//        });
//        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());
//        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
//        keywordRecognizer.Start();
//    }

//    private void StartSinglePlayer()
//    {
//        SceneManager.LoadSceneAsync("mini game");
//    }

//    private void StartTutorial()
//    {
//        SceneManager.LoadSceneAsync("tutorial");
//    }

//    private void PlayGame()
//    {
//        Time.timeScale = 1;
//        AudioListener.pause = false;
//    }

//    private void GoToAction()
//    {
//        AudioListener.pause = true;
//        Time.timeScale = 0;
//    }

//    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
//    {
//        System.Action keywordAction;
//        // if the keyword recognized is in our dictionary, call that Action.
//        if (keywords.TryGetValue(args.text, out keywordAction))
//        {
//            keywordAction.Invoke();
//        }
//    }
//}
