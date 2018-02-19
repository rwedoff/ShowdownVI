using UnityEngine;

public class SinglePManager : MonoBehaviour {
    public GameObject BallObj;
    public GameObject BatObj;

    private AudioSource ballSound;
    private AudioSource batSound;
    private bool gameInit;

    void Start () {
        BallScript.GameInit = true;
        GameUtils.PlayerServe = true;
        gameInit = true;
        ballSound = BallObj.GetComponent<AudioSource>();
        batSound = BatObj.GetComponent<AudioSource>();
        ballSound.mute = true;
        batSound.mute = true;
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

        if (JoyconController.Shoulder2Pressed)
        {
            GameUtils.playState = GameUtils.GamePlayState.SettingBall;
            Time.timeScale = 1;
            BallScript.GameInit = false;
            ballSound.mute = false;
            batSound.mute = false;
            gameInit = false;
        }
    }
}
