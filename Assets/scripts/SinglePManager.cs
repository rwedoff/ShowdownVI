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

    #region Button Objects
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
    private Button howToPlayButton;

    //Change Diff Menu
    private Button easyButton;
    private Button mediumButton;
    private Button hardButton;
    private Button diffBack;

    #endregion

    //Button list
    private List<Button> currButtonList;
    private int currButtonIndex;
    private List<AudioSource> currAudioList;

    private AudioSource ballSound;
    private AudioSource batSound;
    private Queue<AudioSource> playingAudioQueue = new Queue<AudioSource>();
    private AudioSource playingAudio;
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
    private List<AudioSource> settingsAudioList;
    private List<AudioSource> mainMenuAudioList;
    private List<AudioSource> calibMenuAudioList;
    private List<AudioSource> handAudioList;
    private List<AudioSource> diffAudioList;

    #region AudioFile Fields
    ////Calibrate Menu
    private AudioSource calibrateAudio;
    private AudioSource nowCalibAudio;

    ////Hand Menu
    private AudioSource rightHandAudio;
    private AudioSource leftHandAudio;

    ////Main Menu
    private AudioSource mainMenuAudio;
    private AudioSource singlePlayerAudio;
    private AudioSource settingsAudio;
    private AudioSource quitAudio;
    private AudioSource backAudio;

    ////Settings Menu
    private AudioSource changeDiffAudio;
    private AudioSource changeHandAudio;
    private AudioSource reCalibAudio;
    private AudioSource howToPlayShortAudio;
    private AudioSource howToPlayLongAudio;

    ////Change Diff Menu
    private AudioSource changeDiffMenuAudio;
    private AudioSource easyAudio;
    private AudioSource mediumAudio;
    private AudioSource hardAudio;
    #endregion

    void Start () {
        GameUtils.playState = GameUtils.GamePlayState.Menu;
        if (JoyconManager.Instance == null)
        {
            JoyconManager.SinglePlayerMode = true;
            SceneManager.LoadSceneAsync("GlobalInit", LoadSceneMode.Single);
            return;
        }
        SetupChildComponents();
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
        KeyBoardMenuControl();
        CheckAndPlayAudio();
    }

    /// <summary>
    /// Controls that cycles through the single player menu based on keyboard presses
    /// </summary>
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

    /// <summary>
    /// Gets all the child game objects of this gameobject, which is buttons and audioSources.
    /// These are then broken out so click listeners can be added.
    /// </summary>
    private void SetupChildComponents()
    {
        //Main Menu
        var buttons = mainMenuGO.GetComponentsInChildren<Button>(true);
        singlePlayerButton = buttons[0];
        settingsButton = buttons[1];
        quitButton = buttons[2];
        SetupMainMenuClick();
        mainMenuButtonList = new List<Button>() { singlePlayerButton, settingsButton, quitButton };
        var audios = mainMenuGO.GetComponents<AudioSource>();
        mainMenuAudio = audios[0];
        singlePlayerAudio = audios[1];
        settingsAudio = audios[2];
        quitAudio = audios[3];
        var thisAuds = GetComponents<AudioSource>();
        backAudio = thisAuds[1];
        nowCalibAudio = thisAuds[0];

        mainMenuAudioList = new List<AudioSource>() { singlePlayerAudio, settingsAudio, quitAudio };


        //Calibrate Menu
        buttons = calibrateMenuGO.GetComponentsInChildren<Button>(true);
        calibrateButton = buttons[0];
        calibrateBackButton = buttons[1];
        SetupCalibClick();
        calibMenuButtonList = new List<Button>() { calibrateButton, calibrateBackButton };
        audios = calibrateMenuGO.GetComponents<AudioSource>();
        calibrateAudio = audios[0];
        calibrateMenuGO.SetActive(false);
        calibMenuAudioList = new List<AudioSource>() { calibrateAudio, backAudio };

        //Hand Menu
        buttons = handMenuGO.GetComponentsInChildren<Button>(true);
        rightHandButton = buttons[0];
        leftHandButton = buttons[1];
        handBackButton = buttons[2];
        SetupHandClick();
        handButtonList = new List<Button>() { rightHandButton, leftHandButton, handBackButton };
        audios = handMenuGO.GetComponents<AudioSource>();
        rightHandAudio = audios[0];
        leftHandAudio = audios[1];
        handMenuGO.SetActive(false);
        handAudioList = new List<AudioSource>() { rightHandAudio, leftHandAudio, backAudio };

        //Settings Menu
        buttons = settingsMenuGO.GetComponentsInChildren<Button>(true);
        changeHandButton = buttons[0];
        changeDifficultButton = buttons[1];
        settingsCalibButton = buttons[2];
        howToPlayButton = buttons[3];
        settingsBack = buttons[4];
        SetupSettingsMenuClick();
        settingsButtonList = new List<Button>() { changeHandButton, changeDifficultButton, settingsCalibButton, howToPlayButton, settingsBack };
        audios = settingsMenuGO.GetComponents<AudioSource>();
        changeHandAudio = audios[0];
        changeDiffAudio = audios[1];
        reCalibAudio = audios[2];
        howToPlayShortAudio = audios[3];
        howToPlayLongAudio = audios[4];
        settingsMenuGO.SetActive(false);
        settingsAudioList = new List<AudioSource>() { changeHandAudio, changeDiffAudio, reCalibAudio, howToPlayShortAudio, backAudio };

        //Change Diff Menu
        buttons = diffucultMenuGO.GetComponentsInChildren<Button>(true);
        easyButton = buttons[0];
        mediumButton = buttons[1];
        hardButton = buttons[2];
        diffBack = buttons[3];
        SetupDiffClick();
        diffButtonList = new List<Button>() { easyButton, mediumButton, hardButton, diffBack };
        audios = diffucultMenuGO.GetComponents<AudioSource>();
        changeDiffAudio = audios[0];
        easyAudio = audios[1];
        mediumAudio = audios[2];
        hardAudio = audios[3];
        diffucultMenuGO.SetActive(false);
        diffAudioList = new List<AudioSource>() { easyAudio, mediumAudio, hardAudio, backAudio };
    }

    /// <summary>
    /// Difficulty menu click listeners
    /// </summary>
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

    /// <summary>
    /// Settings menu click listeners
    /// </summary>
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
        howToPlayButton.onClick.AddListener(() =>
        {
            //Play Audio File
            howToPlayLongAudio.Play();
        });
        settingsBack.onClick.AddListener(() =>
        {
            SetMainMenu();
            settingsMenuGO.SetActive(false);
        });
    }

    /// <summary>
    /// Main menu click listeners
    /// </summary>
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

    /// <summary>
    /// Starts the game and closes the menu
    /// </summary>
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

    /// <summary>
    /// Hand menu click listeners
    /// </summary>
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

    /// <summary>
    /// Calibration button menu click listeners
    /// </summary>
    private void SetupCalibClick()
    {
        calibrateButton.onClick.AddListener(() =>
        {
            CalibrateGame();
            gameInit = false;
            calibrateMenuGO.SetActive(false);
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
    
    /// <summary>
    /// Helper to calibrate the game with the Kinect.
    /// </summary>
    private void CalibrateGame()
    {
        PaddleScript.TableEdge = BodySourceView.baseKinectPosition.Z;
        PaddleScript.CenterX = BodySourceView.baseKinectPosition.X;
        AddAudioToPlayingList(nowCalibAudio);
    }

    /// <summary>
    /// Sets the main menu gameobject/UI active
    /// </summary>
    private void SetMainMenu()
    {
        mainMenuGO.SetActive(true);
        titleText.text = MENUTEXT;
        currButtonList = mainMenuButtonList;
        currButtonIndex = 0;
        currAudioList = mainMenuAudioList;
        ToggleTopButton();
    }

    /// <summary>
    /// Sets the calibration gameobject/UI active
    /// </summary>
    private void SetCalibMenu()
    {
        calibrateMenuGO.SetActive(true);
        titleText.text = CALIBTEXT;
        currButtonIndex = 0;
        currButtonList = calibMenuButtonList;
        currAudioList = calibMenuAudioList;
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

    /// <summary>
    /// Sets the difficulty gameobject/UI active
    /// </summary>
    private void SetDiffMenu()
    {
        diffucultMenuGO.SetActive(true);
        titleText.text = DIFFTEXT;
        currButtonList = diffButtonList;
        currButtonIndex = 0;
        currAudioList = diffAudioList;
        ToggleTopButton();
    }

    /// <summary>
    /// Sets the settings gameobject/UI active
    /// </summary>
    private void SetSettingsMenu()
    {
        settingsMenuGO.SetActive(true);
        titleText.text = SETTINGSTEXT;
        currButtonList = settingsButtonList;
        currButtonIndex = 0;
        currAudioList = settingsAudioList;
        ToggleTopButton();
    }

    /// <summary>
    /// Sets the change hand gameobject/UI active
    /// </summary>
    private void SetHandMenu()
    {
        handMenuGO.SetActive(true);
        titleText.text = HANDTEXT;
        currButtonList = handButtonList;
        currButtonIndex = 0;
        currAudioList = handAudioList;
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
    
    /// <summary>
    /// Toggles the top button in the menu active
    /// </summary>
    private void ToggleTopButton()
    {
        var tempButton = currButtonList[currButtonIndex];
        var cb = tempButton.colors;
        cb.normalColor = Color.cyan;
        tempButton.colors = cb;
    }

    /// <summary>
    /// Changes the difficulty of the game
    /// </summary>
    /// <param name="diff"></param>
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
    
    /// <summary>
    /// Clicks the button in the menu. This is called from a keyboard button
    /// press to click the menu button.
    /// </summary>
    public void ClickSelectedButton()
    {
        var button = currButtonList[currButtonIndex];
        button.onClick.Invoke();
        ColorBlock cb = button.colors;
        cb.normalColor = Color.grey;
        button.colors = cb;
        if(button != howToPlayButton)
        {
            AddAudioToPlayingList(currAudioList[currButtonIndex]);
        }
    }
    
    /// <summary>
    /// Highlights the currently selected button in the menu. 
    /// Also includes logic to cycle though the menus once the 
    /// </summary>
    /// <param name="upArrow"></param>
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
        AddAudioToPlayingList(currAudioList[currButtonIndex]);
    }

    /// <summary>
    /// Changes the hand for the whole game.
    /// </summary>
    /// <param name="isLefty"></param>
    private void ChangeHand(bool isLefty)
    {
        var script = bodySourceViewGO.GetComponent<BodySourceView>();
        script.SetLeftyToggle(isLefty);
        if (isLefty)
            handText.text = "Hand: Left";
        else
            handText.text = "Hand: Right";
    }

    /// <summary>
    /// Reads the first serve of the game
    /// </summary>
    /// <returns></returns>
    private IEnumerator ReadInitServe()
    {
        AudioSource serveAudio = NumberSpeech.PlayAudio(GameUtils.PlayerServe ? "yourserve" : "oppserve");
        yield return new WaitForSeconds(serveAudio.clip.length);
    }

    /// <summary>
    /// Resets the game back to the single player menu
    /// </summary>
    public static void ResetGameToMenu()
    {
        WholeMenuInstance.SetActive(true);
        GameUtils.playState = GameUtils.GamePlayState.Menu;
    }

    /// <summary>
    /// Adds an AudioSource to a queue to play for the accessible menu
    /// </summary>
    /// <param name="newAudio"></param>
    private void AddAudioToPlayingList(AudioSource newAudio)
    {
        playingAudioQueue.Enqueue(newAudio);
    }

    /// <summary>
    /// Plays audio if there is audio in the AudioSource queue
    /// </summary>
    private void CheckAndPlayAudio()
    {
        if(playingAudioQueue.Count != 0)
        {
            if (playingAudio == null || !playingAudio.isPlaying)
            {
                playingAudio = playingAudioQueue.Dequeue();
                playingAudio.Play();
            }
        }
    }

}
