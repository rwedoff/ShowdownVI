using System.Collections;
using UnityEngine;

public class SinglePManager : MonoBehaviour {
    public GameObject BallObj;
    public GameObject BatObj;

    private AudioSource ballSound;
    private AudioSource batSound;
    private bool gameInit;
    private bool calibrated;
    private bool calibratedWait;

    public static float TableEdge { get; private set; }
    public static float CenterX { get; private set; }

    void Start () {
        BallScript.GameInit = true;
        GameUtils.PlayerServe = true;
        gameInit = true;
        ballSound = BallObj.GetComponent<AudioSource>();
        batSound = BatObj.GetComponent<AudioSource>();
        ballSound.mute = true;
        batSound.mute = true;
        calibrated = false;
        calibratedWait = false;
        StartCoroutine(GameUtils.PlayIntroMusic());
    }

    // Update is called once per frame
    void Update () {
        //Check ball state
        if (!gameInit)
        {
            //Debug ONLY
            //GameUtils.playState = GameUtils.GamePlayState.InPlay;
            //END DEBUG
            return;
        }

        if(!calibrated && JoyconController.ButtonPressed)
        {
            TableEdge = BodySourceView.baseKinectPosition.Z;
            CenterX = BodySourceView.baseKinectPosition.X;
            calibrated = true;
            return;
        }

        if(calibrated && !calibratedWait && !JoyconController.ButtonPressed)
        {
            calibratedWait = true;
            StartCoroutine(ReadInitServe());
            return;
        }

        if (calibratedWait && JoyconController.ButtonPressed)
        {
            GameUtils.playState = GameUtils.GamePlayState.SettingBall;
            Time.timeScale = 1;
            BallScript.GameInit = false;
            ballSound.mute = false;
            batSound.mute = false;
            gameInit = false;
        }
    }

    private IEnumerator ReadInitServe()
    {
        AudioSource serveAudio = NumberSpeech.PlayAudio(GameUtils.PlayerServe ? "yourserve" : "oppserve");
        yield return new WaitForSeconds(serveAudio.clip.length);
        calibratedWait = true;
    }
}
