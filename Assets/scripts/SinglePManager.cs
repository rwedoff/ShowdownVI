using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SinglePManager : MonoBehaviour {

    public static GameObject WholeMenuInstance { get; private set; }

    public GameObject BallObj;
    public GameObject BatObj;
    public GameObject bodySourceViewGO;
    public GameObject handMenuGO;
    public GameObject mainMenuGO;
    public GameObject calibrateMenuGO;
    public GameObject diffucultMenuGO;
    public GameObject settingsMenuGO;
    public GameObject wholeMenuGO;
    public Text titleText;
    public GameObject diffTextGO;
    public GameObject handTextGO;

    private TextMesh diffText;
    private TextMesh handText;

    //Calibrate Menu
    private Button calibrateButton;
    private Button calibrateBackButton;

    //Hand Menu
    private Button rightHandButton;
    private Button leftHandButton;
    private Button handBackButton;

    //Main Menu
    private Button singlePlayerButton;
    private Button settingsButton;
    private Button quitButton;

    //Settings Menu
    private Button changeDifficultButton;
    private Button changeHandButton;
    private Button settingsBack;
    private Button settingsCalibButton;

    //Change Diff Menu
    private Button easyButton;
    private Button mediumButton;
    private Button hardButton;
    private Button diffBack;

    //Button list
    private List<Button> currButtonList;
    private int currButtonIndex;

    private AudioSource ballSound;
    private AudioSource batSound;
    private bool gameInit;
    private const string MENUTEXT = "Virtual Showdown";
    private const string DIFFTEXT = "Set Difficulty";
    private const string SETTINGSTEXT = "Settings";
    private const string HANDTEXT = "Set Hand";
    private const string CALIBTEXT = "Set Calibration";
    private List<Button> settingsButtonList;
    private List<Button> mainMenuButtonList;
    private List<Button> calibMenuButtonList;
    private List<Button> handButtonList;
    private List<Button> diffButtonList;

    void Start () {
        GameUtils.playState = GameUtils.GamePlayState.Menu;
        if (JoyconManager.Instance == null)
        {
            JoyconManager.SinglePlayerMode = true;
            SceneManager.LoadSceneAsync("GlobalInit", LoadSceneMode.Single);
            return;
        }
        SetupChildButtons();
        BallScript.GameInit = true;
        GameUtils.PlayerServe = true;
        gameInit = true;
        ballSound = BallObj.GetComponent<AudioSource>();
        batSound = BatObj.GetComponent<AudioSource>();
        ballSound.mute = true;
        batSound.mute = true;
        diffText = diffTextGO.GetComponent<TextMesh>();
        handText = handTextGO.GetComponent<TextMesh>();
        StartCoroutine(GameUtils.PlayIntroMusic());
        SetHandMenu();
        mainMenuGO.SetActive(false);
        WholeMenuInstance = wholeMenuGO;
    }

    // Update is called once per frame
    void Update()
    {
        ////Check ball state
        //if (!gameInit)
        //{
        //    //Debug ONLY
        //    //GameUtils.playState = GameUtils.GamePlayState.InPlay;
        //    //END DEBUG
        //    GameUtils.playState = GameUtils.GamePlayState.Menu;
        //    return;
        //}

        //if (!calibrated && JoyconController.ButtonPressed)
        //{
        //    TableEdge = BodySourceView.baseKinectPosition.Z;
        //    CenterX = BodySourceView.baseKinectPosition.X;
        //    calibrated = true;
        //    return;
        //}

        //if (calibrated && !calibratedWait && !JoyconController.ButtonPressed)
        //{
        //    calibratedWait = true;
        //    StartCoroutine(ReadInitServe());
        //    return;
        //}

        //if (calibratedWait && JoyconController.ButtonPressed)
        //{
        //    GameUtils.playState = GameUtils.GamePlayState.SettingBall;
        //    Time.timeScale = 1;
        //    BallScript.GameInit = false;
        //    ballSound.mute = false;
        //    batSound.mute = false;
        //    gameInit = false;
        //}

        KeyBoardMenuControl();
    }

    private void KeyBoardMenuControl()
    {
        if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            ToggleHighlightedButton(true);
        }
        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            ToggleHighlightedButton(false);
        }
        if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))
        {
            ClickSelectedButton();
        }
    }

    private void SetupChildButtons()
    {
        //Calibrate Menu
        var buttons = calibrateMenuGO.GetComponentsInChildren<Button>(true);
        calibrateButton = buttons[0];
        calibrateBackButton = buttons[1];
        calibrateMenuGO.SetActive(false);
        SetupCalibClick();
        calibMenuButtonList = new List<Button>() { calibrateButton, calibrateBackButton };

        //Hand Menu
        buttons = handMenuGO.GetComponentsInChildren<Button>(true);
        rightHandButton = buttons[0];
        leftHandButton = buttons[1];
        handBackButton = buttons[2];
        handMenuGO.SetActive(false);
        SetupHandClick();
        handButtonList = new List<Button>() { rightHandButton, leftHandButton, handBackButton };

        //Main Menu
        buttons = mainMenuGO.GetComponentsInChildren<Button>(true);
        singlePlayerButton = buttons[0];
        settingsButton = buttons[1];
        quitButton = buttons[2];
        SetupMainMenuClick();
        mainMenuButtonList = new List<Button>() { singlePlayerButton, settingsButton, quitButton };

        //Settings Menu
        buttons = settingsMenuGO.GetComponentsInChildren<Button>(true);
        changeHandButton = buttons[0];
        changeDifficultButton = buttons[1];
        settingsCalibButton = buttons[2];
        settingsBack = buttons[3];
        settingsMenuGO.SetActive(false);
        SetupSettingsMenuClick();
        settingsButtonList = new List<Button>() { changeHandButton, changeDifficultButton, settingsCalibButton, settingsBack };

        //Change Diff Menu
        buttons = diffucultMenuGO.GetComponentsInChildren<Button>(true);
        easyButton = buttons[0];
        mediumButton = buttons[1];
        hardButton = buttons[2];
        diffBack = buttons[3];
        diffucultMenuGO.SetActive(false);
        SetupDiffClick();
        diffButtonList = new List<Button>() { easyButton, mediumButton, hardButton, diffBack };

    }

    private void SetupDiffClick()
    {
        diffBack.onClick.AddListener(() =>
        {
            diffucultMenuGO.SetActive(false);
            SetSettingsMenu();
        });
        easyButton.onClick.AddListener(() =>
        {
            ChangeDifficulty(0);
            diffucultMenuGO.SetActive(false);
            SetMainMenu();
        });
        mediumButton.onClick.AddListener(() =>
        {
            ChangeDifficulty(1);
            diffucultMenuGO.SetActive(false);
            SetMainMenu();
        });
        hardButton.onClick.AddListener(() =>
        {
            ChangeDifficulty(2);
            diffucultMenuGO.SetActive(false);
            SetMainMenu();
        });
    }

    private void SetupSettingsMenuClick()
    {
        changeDifficultButton.onClick.AddListener(() => {
            settingsMenuGO.SetActive(false);
            SetDiffMenu();
        });
        changeHandButton.onClick.AddListener(() => {
            settingsMenuGO.SetActive(false);
            SetHandMenu();
        });
        settingsCalibButton.onClick.AddListener(() =>
        {
            settingsMenuGO.SetActive(false);
            SetCalibMenu();
        });
        settingsBack.onClick.AddListener(() =>
        {
            SetMainMenu();
            settingsMenuGO.SetActive(false);
        });
    }

    private void SetupMainMenuClick()
    {
        singlePlayerButton.onClick.AddListener(() =>
        {
            SetMainMenu();
            wholeMenuGO.SetActive(false);
            StartGame();
        });
        settingsButton.onClick.AddListener(() =>
        {
            mainMenuGO.SetActive(false);
            SetSettingsMenu();
        });
        quitButton.onClick.AddListener(() =>
        {
            Application.Quit();
        });
    }

    private void StartGame()
    {
        StartCoroutine(ReadInitServe());
        BallObj.transform.position = new Vector3(0, BallObj.transform.position.y, 0);
        GameUtils.playState = GameUtils.GamePlayState.SettingBall;
        BallScript.GameInit = false;
        ballSound.mute = false;
        batSound.mute = false;
        GoalScript.gameOver = false;
    }

    private void SetupHandClick()
    {
        leftHandButton.onClick.AddListener(() =>
        {
            PlayerPrefs.SetInt("hand", 1);
            ChangeHand(true);
            handMenuGO.SetActive(false);
            if (gameInit)
            {
                SetCalibMenu();
            }
            else
            {
                SetMainMenu();
            }
        });
        rightHandButton.onClick.AddListener(() =>
        {
            PlayerPrefs.SetInt("hand", 0);
            ChangeHand(false);
            handMenuGO.SetActive(false);
            handMenuGO.SetActive(false);
            if (gameInit)
            {
                SetCalibMenu();
            }
            else
            {
                SetMainMenu();
            }
        });
        handBackButton.onClick.AddListener(() =>
        {
            handMenuGO.SetActive(false);
            SetSettingsMenu();
        });
    }

    private void SetupCalibClick()
    {
        calibrateButton.onClick.AddListener(() =>
        {
            CalibrateGame();
            calibrateMenuGO.SetActive(false);
            gameInit = false;
            //PLAY AUDIO, Calibrated game
            SetMainMenu();
        });
        calibrateBackButton.onClick.AddListener(() =>
        {
            calibrateMenuGO.SetActive(false);
            if (gameInit)
            {
                SetHandMenu();
            }
            else
            {
                SetMainMenu();
            }
        });
    }

    private void CalibrateGame()
    {
        PaddleScript.TableEdge = BodySourceView.baseKinectPosition.Z;
        PaddleScript.CenterX = BodySourceView.baseKinectPosition.X;
    }

    private void SetMainMenu()
    {
        mainMenuGO.SetActive(true);
        titleText.text = MENUTEXT;
        currButtonList = mainMenuButtonList;
        currButtonIndex = 0;
        ToggleTopButton();
    }

    private void SetCalibMenu()
    {
        calibrateMenuGO.SetActive(true);
        titleText.text = CALIBTEXT;
        currButtonIndex = 0;
        currButtonList = calibMenuButtonList;
        ToggleTopButton();
        if (gameInit)
        {
            calibrateBackButton.gameObject.SetActive(false);
        }
        else
        {
            calibrateBackButton.gameObject.SetActive(true);
        }
    }

    private void SetDiffMenu()
    {
        diffucultMenuGO.SetActive(true);
        titleText.text = DIFFTEXT;
        currButtonList = diffButtonList;
        currButtonIndex = 0;
        ToggleTopButton();
    }

    private void SetSettingsMenu()
    {
        settingsMenuGO.SetActive(true);
        titleText.text = SETTINGSTEXT;
        currButtonList = settingsButtonList;
        currButtonIndex = 0;
        ToggleTopButton();
    }

    private void SetHandMenu()
    {
        handMenuGO.SetActive(true);
        titleText.text = HANDTEXT;
        currButtonList = handButtonList;
        currButtonIndex = 0;
        ToggleTopButton();
        if (gameInit)
        {
            handBackButton.gameObject.SetActive(false);
        }
        else
        {
            handBackButton.gameObject.SetActive(true);
        }
    }

    private void ToggleTopButton()
    {
        var tempButton = currButtonList[currButtonIndex];
        var cb = tempButton.colors;
        cb.normalColor = Color.cyan;
        tempButton.colors = cb;
    }

    private void ChangeDifficulty(int diff)
    {
        var str = "Difficulty: ";
        AIColliderScript.SaveDifficulty(diff);
        if (diff == 0) //Easy
        {
            diffText.text = str + "Easy";
        }
        else if (diff == 1) //Medium
        {
            diffText.text = str + "Medium";
        }
        else if (diff == 2) //Hard
        {
            diffText.text = str + "Hard";
        }
    }

    public void ClickSelectedButton()
    {
        var button = currButtonList[currButtonIndex];
        button.onClick.Invoke();
        ColorBlock cb = button.colors;
        cb.normalColor = Color.grey;
        button.colors = cb;
    }

    public void ToggleHighlightedButton(bool upArrow)
    {
        int endOfList = gameInit ? currButtonList.Count - 1 : currButtonList.Count;
        var tempButton = currButtonList[currButtonIndex];
        ColorBlock cb = tempButton.colors;
        cb.normalColor = Color.grey;
        tempButton.colors = cb;

        if (!upArrow)
        {
            currButtonIndex++;
            if (currButtonIndex >= endOfList)
            {
                currButtonIndex = 0;
            }
        }
        else
        {
            currButtonIndex--;
            if(currButtonIndex < 0)
            {
                currButtonIndex = endOfList - 1;
            }
        }
        tempButton = currButtonList[currButtonIndex];
        cb = tempButton.colors;
        cb.normalColor = Color.cyan;
        tempButton.colors = cb;
    }

    private void ChangeHand(bool isLefty)
    {
        var script = bodySourceViewGO.GetComponent<BodySourceView>();
        script.SetLeftyToggle(isLefty);
        if (isLefty)
            handText.text = "Hand: Left";
        else
            handText.text = "Hand: Right";
    }

    private IEnumerator ReadInitServe()
    {
        AudioSource serveAudio = NumberSpeech.PlayAudio(GameUtils.PlayerServe ? "yourserve" : "oppserve");
        yield return new WaitForSeconds(serveAudio.clip.length);
    }

    public static void ResetGameToMenu()
    {
        WholeMenuInstance.SetActive(true);
        GameUtils.playState = GameUtils.GamePlayState.Menu;
    }
}
